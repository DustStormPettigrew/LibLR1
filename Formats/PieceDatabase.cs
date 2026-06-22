using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibLR1
{
	/// <summary>Opaque LPIECEHI.LEB/LPIECELO.LEB descriptor database. Mesh payload decoding is deliberately out of scope.</summary>
	public sealed class PieceDatabase
	{
		private const byte StringToken = 0x02;
		private const byte ArrayToken = 0x14;
		private const byte Int32Token = 0x04;
		private const byte PiecesBlockToken = 0x27;
		private readonly byte[] m_originalBytes;
		private readonly List<PieceDatabaseEntry> m_entries;

		public byte FileHeaderToken { get; }
		public int DeclaredGeometryElementCount { get; }
		public byte ContentMarker { get; }
		public IReadOnlyList<PieceDatabaseEntry> Entries { get { return m_entries; } }

		public PieceDatabase(string p_filepath) : this(File.ReadAllBytes(p_filepath)) { }

		public PieceDatabase(Stream p_stream) : this(ReadAll(p_stream)) { }

		public PieceDatabase(byte[] p_bytes)
		{
			if (p_bytes == null) throw new ArgumentNullException(nameof(p_bytes));
			m_originalBytes = (byte[])p_bytes.Clone();
			if (m_originalBytes.Length < 9 || m_originalBytes[1] != 0x07 || m_originalBytes[2] != Int32Token || m_originalBytes[7] != 0x08)
				throw new InvalidDataException("Invalid LPIECE header.");
			FileHeaderToken = m_originalBytes[0];
			DeclaredGeometryElementCount = BitConverter.ToInt32(m_originalBytes, 3);
			ContentMarker = m_originalBytes[8];
			m_entries = ParseDescriptorTable(m_originalBytes);
			if (m_entries.Count != 161)
				throw new InvalidDataException("LPIECE descriptor table must contain 161 entries; found " + m_entries.Count + ".");
		}

		public IReadOnlyList<string> GetPieceNames()
		{
			List<string> names = new List<string>(149);
			for (int i = 0; i < 149; i++) names.Add(m_entries[i].Name);
			return names;
		}

		public IReadOnlyList<string> GetChassisNames()
		{
			List<string> names = new List<string>(12);
			for (int i = 149; i < 161; i++) names.Add(m_entries[i].Name);
			return names;
		}

		/// <summary>Writes the original token-encoded bytes unchanged.</summary>
		public void Write(string p_filepath) { File.WriteAllBytes(p_filepath, m_originalBytes); }
		public void Save(Stream p_stream) { p_stream.Write(m_originalBytes, 0, m_originalBytes.Length); }

		private static byte[] ReadAll(Stream p_stream)
		{
			using (MemoryStream output = new MemoryStream())
			{
				p_stream.CopyTo(output);
				return output.ToArray();
			}
		}

		private static List<PieceDatabaseEntry> ParseDescriptorTable(byte[] p_bytes)
		{
			int offset = FindPiecesBlock(p_bytes);
			offset += 1;
			Expect(p_bytes, ref offset, 0x07);
			Expect(p_bytes, ref offset, Int32Token);
			int count = ReadInt32(p_bytes, ref offset);
			Expect(p_bytes, ref offset, 0x08);
			Expect(p_bytes, ref offset, 0x05);
			List<PieceDatabaseEntry> result = new List<PieceDatabaseEntry>(count);
			for (int i = 0; i < count; i++)
			{
				Expect(p_bytes, ref offset, StringToken);
				string name = ReadNullTerminatedAscii(p_bytes, ref offset);
				int geometryOffset = offset;
				Expect(p_bytes, ref offset, ArrayToken);
				ushort arrayCount = ReadUInt16(p_bytes, ref offset);
				Expect(p_bytes, ref offset, Int32Token);
				int payloadBytes = checked(arrayCount * 4);
				if (offset + payloadBytes > p_bytes.Length) throw new EndOfStreamException("LPIECE descriptor payload exceeds file length.");
				byte[] blob = new byte[3 + payloadBytes];
				Buffer.BlockCopy(p_bytes, geometryOffset, blob, 0, blob.Length);
				offset += payloadBytes;
				result.Add(new PieceDatabaseEntry(name, blob, geometryOffset));
			}
			return result;
		}

		private static int FindPiecesBlock(byte[] p_bytes)
		{
			for (int i = 0; i <= p_bytes.Length - 9; i++)
				if (p_bytes[i] == PiecesBlockToken && p_bytes[i + 1] == 0x07 && p_bytes[i + 2] == Int32Token && p_bytes[i + 7] == 0x08 && p_bytes[i + 8] == 0x05)
					return i;
			throw new InvalidDataException("LPIECE descriptor block 0x27 was not found.");
		}

		private static void Expect(byte[] p_bytes, ref int p_offset, byte p_expected) { if (p_offset >= p_bytes.Length || p_bytes[p_offset++] != p_expected) throw new InvalidDataException("Malformed LPIECE descriptor table."); }
		private static ushort ReadUInt16(byte[] p_bytes, ref int p_offset) { if (p_offset + 2 > p_bytes.Length) throw new EndOfStreamException(); ushort value = BitConverter.ToUInt16(p_bytes, p_offset); p_offset += 2; return value; }
		private static int ReadInt32(byte[] p_bytes, ref int p_offset) { if (p_offset + 4 > p_bytes.Length) throw new EndOfStreamException(); int value = BitConverter.ToInt32(p_bytes, p_offset); p_offset += 4; return value; }
		private static string ReadNullTerminatedAscii(byte[] p_bytes, ref int p_offset) { int start = p_offset; while (p_offset < p_bytes.Length && p_bytes[p_offset] != 0) p_offset++; if (p_offset == p_bytes.Length) throw new EndOfStreamException(); string value = Encoding.ASCII.GetString(p_bytes, start, p_offset - start); p_offset++; return value; }
	}

	public sealed class PieceDatabaseEntry
	{
		internal PieceDatabaseEntry(string p_name, byte[] p_geometryBlob, long p_geometryOffset) { Name = p_name; GeometryBlob = p_geometryBlob; GeometryOffset = p_geometryOffset; }
		public string Name { get; }
		public byte[] GeometryBlob { get; }
		public long GeometryOffset { get; }
	}
}
