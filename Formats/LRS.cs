using LibLR1.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibLR1
{
	/// <summary>
	/// Lego Racers save / leaderboard format.
	/// Applies to Save\&lt;slot&gt;\LEGORac&lt;N&gt;, MENUDATA\DEFAULT.LRS, and MENUDATA\QBUILD.LRS.
	/// Does NOT use the token-based binary encoding — raw fixed-struct format.
	/// </summary>
	public class LRS
	{
		public const int HeaderSize = 1152;  // 0x480
		public const int RecordSize = 640;   // 0x280
		public const int NameSize = 28;
		public const int ConfigBytesSize = 4;
		public const int ChassisTagSize = 9;
		public const int HeaderPadSize = 5;
		public const int KartMetadataSize = 3;

		private const int NameOffset = 0x00;
		private const int ConfigBytesOffset = 0x1C;
		private const int RecordIdOffset = 0x20;
		private const int ChassisTagOffset = 0x21;
		private const int BrickCountOffset = 0x2A;
		private const int HeaderPadOffset = 0x2B;
		private const int KartMetadataOffset = 0x30;
		private const int BrickRecordsOffset = 0x33;

		private byte[] m_header;
		private LRSHeader m_decodedHeader;
		private List<LRSCar> m_cars;

		/// <summary>
		/// Raw 1152-byte file header. Store verbatim for round-trip fidelity.
		/// </summary>
		public byte[] Header
		{
			get { return m_header; }
			set
			{
				m_header = value;
				m_decodedHeader = value != null && value.Length == HeaderSize ? new LRSHeader(value) : null;
			}
		}

		/// <summary>Decoded write-through view of the raw 1152-byte header.</summary>
		public LRSHeader DecodedHeader
		{
			get { return m_decodedHeader; }
		}

		public List<LRSCar> Cars
		{
			get { return m_cars; }
			set { m_cars = value; }
		}

		/// <summary>
		/// When true, zero physical record slots after Header.ActiveRacerCount on write.
		/// Defaults to false so engine residual bytes remain byte-identical on round-trip.
		/// </summary>
		public bool NormalizeOnSave { get; set; }

		public LRS(string p_filepath)
		{
			using (FileStream fs = File.OpenRead(p_filepath))
			{
				Init(fs);
			}
		}

		public LRS(Stream p_stream)
		{
			Init(p_stream);
		}

		private void Init(Stream p_stream)
		{
			long fileSize = p_stream.Length;
			long recordArea = fileSize - HeaderSize;
			if (recordArea < 0 || recordArea % RecordSize != 0)
			{
				throw new InvalidDataException(string.Format(
					"LRS file size {0} does not align to header ({1}) + N×{2} record layout.",
					fileSize, HeaderSize, RecordSize));
			}
			int recordCount = (int)(recordArea / RecordSize);

			using (BinaryReader reader = new BinaryReader(p_stream, Encoding.ASCII, leaveOpen: true))
			{
				ValidateMagic(reader);

				p_stream.Seek(0, SeekOrigin.Begin);
				m_header = reader.ReadBytes(HeaderSize);
				m_decodedHeader = new LRSHeader(m_header);

				m_cars = new List<LRSCar>(recordCount);
				for (int i = 0; i < recordCount; i++)
				{
					m_cars.Add(ReadCar(reader));
				}
			}
		}

		public void Write(string p_filepath)
		{
			using (FileStream fs = File.Open(p_filepath, FileMode.Create, FileAccess.Write))
				Write(fs);
		}

		public void Write(Stream p_stream)
		{
			using (BinaryWriter writer = new BinaryWriter(p_stream, Encoding.ASCII, leaveOpen: true))
			{
				if (m_header == null || m_header.Length != HeaderSize)
				{
					throw new InvalidDataException(string.Format("LRS header must be exactly {0} bytes.", HeaderSize));
				}
				if (m_cars == null)
				{
					throw new InvalidDataException("LRS car list is null.");
				}
				RecomputeBlockChecksums(m_header);
				writer.Write(m_header);
				int activeRacerCount = m_decodedHeader.ActiveRacerCount;
				for (int i = 0; i < m_cars.Count; i++)
				{
					if (NormalizeOnSave && i >= activeRacerCount)
						writer.Write(new byte[RecordSize]);
					else
						WriteCar(writer, m_cars[i]);
				}
			}
		}

		private static void ValidateMagic(BinaryReader p_reader)
		{
			byte b0 = p_reader.ReadByte();
			byte b1 = p_reader.ReadByte();
			if (b0 != (byte)'L' || b1 != (byte)'R')
			{
				throw new InvalidLRSMagicException(b0, b1);
			}
		}

		private static LRSCar ReadCar(BinaryReader p_reader)
		{
			long recordStart = p_reader.BaseStream.Position;
			byte[] record = p_reader.ReadBytes(RecordSize);
			if (record.Length != RecordSize)
			{
				throw new EndOfStreamException(string.Format(
					"Expected {0} bytes for LRS car record at offset 0x{1:X}.",
					RecordSize, recordStart));
			}

			byte brickCount = record[BrickCountOffset];
			int storedBrickCount = GetStoredBrickCount(brickCount);
			if (BrickRecordsOffset + checked(storedBrickCount * BrickPlacement.Size) > RecordSize)
			{
				throw new InvalidDataException(string.Format(
					"LRS car record at offset 0x{0:X} declares {1} stored brick records, which exceeds the {2}-byte record.",
					recordStart, storedBrickCount, RecordSize));
			}

			List<BrickPlacement> bricks = new List<BrickPlacement>(storedBrickCount);
			int trailingOffset = BrickRecordsOffset;
			for (int i = 0; i < storedBrickCount; i++)
			{
				int brickOffset = trailingOffset;
				if (brickOffset + BrickPlacement.Size > RecordSize)
				{
					throw new InvalidDataException(string.Format(
						"LRS brick record {0} at absolute offset 0x{1:X} exceeds the {2}-byte record.",
						i,
						recordStart + brickOffset,
						RecordSize));
				}

				byte brickTypeToken = record[brickOffset];
				if (!BrickPlacement.IsKnownBrickTypeToken(brickTypeToken))
				{
					if (CanPreserveUnsupportedBuildTail(record, brickOffset))
					{
						break;
					}
					throw new InvalidDataException(string.Format(
						"LRS brick type token is unsupported at record offset 0x{0:X2} (absolute 0x{1:X}): got 0x{2:X2}.",
						brickOffset,
						recordStart + brickOffset,
						brickTypeToken));
				}
				bricks.Add(BrickPlacement.Read(record.AsSpan(brickOffset, BrickPlacement.Size)));
				trailingOffset += BrickPlacement.Size;
			}

			return new LRSCar
			{
				Name = DecodeUTF16LEName(record.AsSpan(NameOffset, NameSize)),
				ConfigBytes = CopyBytes(record, ConfigBytesOffset, ConfigBytesSize),
				RecordIdRaw = record[RecordIdOffset],
				ChassisTag = DecodeNullTerminatedASCII(record, ChassisTagOffset, ChassisTagSize),
				BrickCount = brickCount,
				HeaderPad = CopyBytes(record, HeaderPadOffset, HeaderPadSize),
				KartMetadata = CopyBytes(record, KartMetadataOffset, KartMetadataSize),
				Bricks = bricks,
				TrailingBytes = CopyBytes(record, trailingOffset, RecordSize - trailingOffset)
			};
		}

		private static void WriteCar(BinaryWriter p_writer, LRSCar p_car)
		{
			if (p_car == null)
			{
				throw new InvalidDataException("LRS car record is null.");
			}
			if (p_car.ConfigBytes == null || p_car.ConfigBytes.Length != 4)
			{
				throw new InvalidDataException("LRS car ConfigBytes must be exactly 4 bytes.");
			}
			if (p_car.ChassisTag == null)
			{
				throw new InvalidDataException("LRS car ChassisTag is null.");
			}
			if (p_car.HeaderPad == null || p_car.HeaderPad.Length != HeaderPadSize)
			{
				throw new InvalidDataException("LRS car HeaderPad must be exactly 5 bytes.");
			}
			if (p_car.KartMetadata == null || p_car.KartMetadata.Length != KartMetadataSize)
			{
				throw new InvalidDataException("LRS car KartMetadata must be exactly 3 bytes.");
			}
			if (p_car.Bricks == null)
			{
				throw new InvalidDataException("LRS car Bricks list is null.");
			}
			if (p_car.TrailingBytes == null)
			{
				throw new InvalidDataException("LRS car TrailingBytes is null.");
			}

			int expectedStoredBrickCount = GetStoredBrickCount(p_car.BrickCount);
			if (p_car.Bricks.Count > expectedStoredBrickCount)
			{
				throw new InvalidDataException(string.Format(
					"LRS car `{0}` BrickCount={1} allows at most {2} stored BrickPlacement records, got {3}.",
					p_car.Name,
					p_car.BrickCount,
					expectedStoredBrickCount,
					p_car.Bricks.Count));
			}

			byte[] record = new byte[RecordSize];
			Buffer.BlockCopy(EncodeUTF16LEName(p_car.Name), 0, record, NameOffset, NameSize);
			Buffer.BlockCopy(p_car.ConfigBytes, 0, record, ConfigBytesOffset, ConfigBytesSize);
			record[RecordIdOffset] = p_car.RecordIdRaw;
			Buffer.BlockCopy(EncodeChassisTag(p_car.ChassisTag), 0, record, ChassisTagOffset, ChassisTagSize);
			record[BrickCountOffset] = p_car.BrickCount;
			Buffer.BlockCopy(p_car.HeaderPad, 0, record, HeaderPadOffset, HeaderPadSize);
			Buffer.BlockCopy(p_car.KartMetadata, 0, record, KartMetadataOffset, KartMetadataSize);

			int cursor = BrickRecordsOffset;
			foreach (BrickPlacement brick in p_car.Bricks)
			{
				if (brick == null)
				{
					throw new InvalidDataException(string.Format("LRS car `{0}` has a null BrickPlacement.", p_car.Name));
				}
				brick.Write(record.AsSpan(cursor, BrickPlacement.Size));
				cursor += BrickPlacement.Size;
			}

			if (p_car.TrailingBytes.Length > RecordSize - cursor)
			{
				throw new InvalidDataException(string.Format(
					"LRS car `{0}` TrailingBytes length {1} exceeds remaining record capacity {2}.",
					p_car.Name,
					p_car.TrailingBytes.Length,
					RecordSize - cursor));
			}
			Buffer.BlockCopy(p_car.TrailingBytes, 0, record, cursor, p_car.TrailingBytes.Length);

			RecomputeBlockChecksums(record);
			p_writer.Write(record);
		}

		// On-disk LRS layout: 128-byte blocks — 127 data bytes followed by their byte-sum checksum.
		// FUN_00429810 validates this checksum and returns error 0x11 on mismatch (fatal "Corrupt install").
		// Every write must recompute checksums so the game's decoder accepts the file.
		private static void RecomputeBlockChecksums(byte[] p_buffer)
		{
			int blockCount = p_buffer.Length / 128;
			for (int i = 0; i < blockCount; i++)
			{
				int blockStart = i * 128;
				int sum = 0;
				for (int j = 0; j < 127; j++)
					sum += p_buffer[blockStart + j];
				p_buffer[blockStart + 127] = (byte)sum;
			}
		}

		private static bool CanPreserveUnsupportedBuildTail(byte[] p_record, int p_offset)
		{
			byte value = p_record[p_offset];
			if (value == 0x14)
			{
				return true;
			}
			if (value >= 0x0E && value <= 0x16)
			{
				return true;
			}
			if (value == 0)
			{
				for (int i = p_offset; i < p_record.Length; i++)
				{
					if (p_record[i] != 0)
					{
						return false;
					}
				}
				return true;
			}
			return false;
		}

		private static int GetStoredBrickCount(byte p_brickCount)
		{
			return p_brickCount == 0 ? 0 : p_brickCount - 1;
		}

		private static byte[] CopyBytes(byte[] p_bytes, int p_offset, int p_length)
		{
			byte[] result = new byte[p_length];
			Buffer.BlockCopy(p_bytes, p_offset, result, 0, p_length);
			return result;
		}

		private static string DecodeUTF16LEName(ReadOnlySpan<byte> p_bytes)
		{
			// 28 bytes = 14 UTF-16LE code units, null-terminated, zero-padded
			StringBuilder sb = new StringBuilder(14);
			for (int i = 0; i < 14; i++)
			{
				ushort ch = (ushort)(p_bytes[i * 2] | (p_bytes[i * 2 + 1] << 8));
				if (ch == 0)
				{
					break;
				}
				sb.Append((char)ch);
			}
			return sb.ToString();
		}

		private static byte[] EncodeUTF16LEName(string p_name)
		{
			// Always 28 bytes; characters past index 13 are silently truncated
			byte[] result = new byte[28]; // zero-initialized
			if (p_name == null)
			{
				return result;
			}
			for (int i = 0; i < p_name.Length && i < 14; i++)
			{
				ushort ch = (ushort)p_name[i];
				result[i * 2] = (byte)(ch & 0xFF);
				result[i * 2 + 1] = (byte)(ch >> 8);
			}
			return result;
		}

		private static string DecodeNullTerminatedASCII(byte[] p_bytes, int p_offset, int p_length)
		{
			int length = 0;
			while (length < p_length && p_bytes[p_offset + length] != 0)
			{
				length++;
			}
			return Encoding.ASCII.GetString(p_bytes, p_offset, length);
		}

		private static byte[] EncodeChassisTag(string p_value)
		{
			byte[] result = new byte[ChassisTagSize];
			int byteCount = Encoding.ASCII.GetByteCount(p_value);
			if (byteCount > ChassisTagSize - 1)
			{
				throw new InvalidDataException(string.Format(
					"LRS ChassisTag `{0}` is too long; maximum is {1} ASCII bytes plus a null terminator.",
					p_value,
					ChassisTagSize - 1));
			}

			Encoding.ASCII.GetBytes(p_value, 0, p_value.Length, result, 0);
			result[byteCount] = 0;
			return result;
		}
	}
}
