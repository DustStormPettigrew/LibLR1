using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static LibLR1.BuilderFormatSerialization;

namespace LibLR1
{
	/// <summary>Raw-preserving L_COLORS.LEB reader.</summary>
	public sealed class LColors
	{
		private readonly byte[] m_bytes;
		private readonly string[] m_names;
		public IReadOnlyList<string> Names { get { return m_names; } }

		public LColors(string p_path) : this(File.ReadAllBytes(p_path)) { }
		public LColors(Stream p_stream) : this(ReadAll(p_stream)) { }
		public LColors(byte[] p_bytes)
		{
			m_bytes = Copy(p_bytes);
			m_names = ParseStringArray(m_bytes, 0x2D);
		}

		public int IndexOf(string p_name) { return Array.IndexOf(m_names, p_name); }
		public string NameOf(int p_index) { return p_index >= 0 && p_index < m_names.Length ? m_names[p_index] : null; }
		public void Save(Stream p_stream) { p_stream.Write(m_bytes, 0, m_bytes.Length); }
	}

	/// <summary>Editable, order-preserving *_CSET.LEB roster.</summary>
	public sealed class CSet
	{
		private readonly byte[] m_originalBytes;
		private Dictionary<string, HashSet<string>> m_validColorsByPiece;
		public string ChassisTag { get; private set; }
		public List<(string PieceName, string ColorName)> Entries { get; }
		public Dictionary<string, HashSet<string>> ValidColorsByPiece
		{
			get
			{
				if (m_validColorsByPiece == null)
				{
					m_validColorsByPiece = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
					foreach ((string PieceName, string ColorName) entry in Entries)
					{
						if (!m_validColorsByPiece.TryGetValue(entry.PieceName, out HashSet<string> colors))
							m_validColorsByPiece.Add(entry.PieceName, colors = new HashSet<string>(StringComparer.OrdinalIgnoreCase));
						colors.Add(entry.ColorName);
					}
				}
				return m_validColorsByPiece;
			}
		}

		public CSet(string p_path) : this(File.ReadAllBytes(p_path)) { }
		public CSet(Stream p_stream) : this(ReadAll(p_stream)) { }
		public CSet(byte[] p_bytes)
		{
			m_originalBytes = Copy(p_bytes);
			ChassisTag = ParseCSet(m_originalBytes, out List<(string PieceName, string ColorName)> entries);
			Entries = entries;
		}

		public void AddEntry(string p_pieceName, string p_colorName)
		{
			if (p_pieceName == null) throw new ArgumentNullException(nameof(p_pieceName));
			if (p_colorName == null) throw new ArgumentNullException(nameof(p_colorName));
			Entries.Add((p_pieceName, p_colorName));
			m_validColorsByPiece = null;
		}

		public void RemoveEntry(string p_pieceName, string p_colorName)
		{
			int index = Entries.FindIndex(e => e.PieceName == p_pieceName && e.ColorName == p_colorName);
			if (index >= 0) { Entries.RemoveAt(index); m_validColorsByPiece = null; }
		}

		public void Save(Stream p_stream)
		{
			string originalChassis = ParseCSet(m_originalBytes, out List<(string PieceName, string ColorName)> original);
			bool unchanged = ChassisTag == originalChassis && Entries.Count == original.Count;
			for (int i = 0; unchanged && i < Entries.Count; i++) unchanged = Entries[i].PieceName == original[i].PieceName && Entries[i].ColorName == original[i].ColorName;
			if (unchanged)
			{
				p_stream.Write(m_originalBytes, 0, m_originalBytes.Length);
				return;
			}
			using (BinaryWriter writer = new BinaryWriter(p_stream, Encoding.ASCII, true))
			{
				writer.Write((byte)0x2E); WriteInt(writer, Entries.Select(e => e.PieceName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
				writer.Write((byte)0x31); WriteString(writer, ChassisTag);
				writer.Write((byte)0x30); writer.Write((byte)0x07); WriteInt(writer, Entries.Count); writer.Write((byte)0x08); writer.Write((byte)0x05); writer.Write((byte)0x14); writer.Write((ushort)(Entries.Count * 2)); writer.Write((byte)0x02);
				foreach ((string PieceName, string ColorName) entry in Entries) { WriteRawString(writer, entry.PieceName); WriteRawString(writer, entry.ColorName); }
				writer.Write((byte)0x06);
			}
		}
	}

	/// <summary>Editable CRSTMGR.LEB SetTag registry.</summary>
	public sealed class CrstMgr
	{
		private readonly byte[] m_originalBytes;
		private readonly List<string> m_filenames;
		public IReadOnlyList<string> CSetFilenames { get { return m_filenames; } }
		public CrstMgr(string p_path) : this(File.ReadAllBytes(p_path)) { }
		public CrstMgr(Stream p_stream) : this(ReadAll(p_stream)) { }
		public CrstMgr(byte[] p_bytes) { m_originalBytes = Copy(p_bytes); m_filenames = new List<string>(ParseStringArray(m_originalBytes, 0x2F)); }
		public int IndexOfFilename(string p_filename) { return m_filenames.IndexOf(p_filename); }
		public byte SetTagOf(string p_filename) { int index = IndexOfFilename(p_filename); return index < 0 ? (byte)0 : checked((byte)(0x0B + index)); }
		public void AddEntry(string p_filename) { if (p_filename == null) throw new ArgumentNullException(nameof(p_filename)); m_filenames.Add(p_filename); }
		public void RemoveEntry(string p_filename) { m_filenames.Remove(p_filename); }
		public void Save(Stream p_stream)
		{
			if (m_filenames.SequenceEqual(ParseStringArray(m_originalBytes, 0x2F))) { p_stream.Write(m_originalBytes, 0, m_originalBytes.Length); return; }
			using (BinaryWriter writer = new BinaryWriter(p_stream, Encoding.ASCII, true))
			{
				writer.Write((byte)0x2F); writer.Write((byte)0x07); WriteInt(writer, m_filenames.Count); writer.Write((byte)0x08); writer.Write((byte)0x05); writer.Write((byte)0x14); writer.Write((ushort)m_filenames.Count);
				foreach (string filename in m_filenames) WriteRawString(writer, filename);
				writer.Write((byte)0x06);
			}
		}
	}

	/// <summary>Raw-preserving LPIECEHI/LPIECELO descriptor-table view.</summary>
	public sealed class LPiece
	{
		private const int MetadataLength = 20;
		private readonly byte[] m_bytes;
		private readonly List<(string Name, int OffsetStart, int OffsetEnd)> m_entries;
		private readonly List<LPieceEntry> m_entriesDecoded;
		private readonly Dictionary<string, LPieceEntry> m_entriesByName;
		private readonly Dictionary<ushort, LPieceEntry> m_entriesByBrickKey;
		public IReadOnlyList<string> Names { get; }
		public IReadOnlyList<(string Name, int OffsetStart, int OffsetEnd)> Entries { get { return m_entries; } }
		public IReadOnlyList<LPieceEntry> EntriesDecoded { get { return m_entriesDecoded; } }
		public LPiece(string p_path) : this(File.ReadAllBytes(p_path)) { }
		public LPiece(Stream p_stream) : this(ReadAll(p_stream)) { }
		public LPiece(byte[] p_bytes)
		{
			m_bytes = Copy(p_bytes);
			PieceDatabase database = LoadPieceDatabase(m_bytes);
			m_entries = database.Entries.Select(e => (e.Name, checked((int)e.GeometryOffset), checked((int)e.GeometryOffset + MetadataLength))).ToList();
			Names = m_entries.Select(e => e.Name).ToArray();
			m_entriesDecoded = new List<LPieceEntry>(m_entries.Count);
			m_entriesByName = new Dictionary<string, LPieceEntry>(StringComparer.OrdinalIgnoreCase);
			m_entriesByBrickKey = new Dictionary<ushort, LPieceEntry>();
			foreach ((string name, int start, int end) in m_entries)
			{
				if (end > m_bytes.Length || m_bytes[start] != 0x14 || m_bytes[start + 3] != 0x04)
					throw new InvalidDataException("Malformed LPIECE metadata block.");
				byte[] opaqueBytes = new byte[MetadataLength];
				Buffer.BlockCopy(m_bytes, start, opaqueBytes, 0, MetadataLength);
				uint key = BitConverter.ToUInt32(m_bytes, start + 4);
				LPieceEntry entry = new LPieceEntry
				{
					Name = name,
					Token = (byte)((key >> 8) & 0xFF),
					BrickId = (byte)(key & 0xFF),
					VertexCount = BitConverter.ToUInt32(m_bytes, start + 8),
					FaceCount = BitConverter.ToUInt32(m_bytes, start + 12),
					GeometryOffset = BitConverter.ToUInt32(m_bytes, start + 16),
					OpaqueBytes = opaqueBytes
				};
				m_entriesDecoded.Add(entry);
				m_entriesByName.Add(entry.Name, entry);
				m_entriesByBrickKey.Add((ushort)((entry.Token << 8) | entry.BrickId), entry);
			}
		}
		public LPieceEntry? FindByBrickId(byte p_token, byte p_brickId)
		{
			return m_entriesByBrickKey.TryGetValue((ushort)((p_token << 8) | p_brickId), out LPieceEntry entry) ? entry : (LPieceEntry?)null;
		}
		public LPieceEntry? FindByName(string p_name)
		{
			return p_name != null && m_entriesByName.TryGetValue(p_name, out LPieceEntry entry) ? entry : (LPieceEntry?)null;
		}
		public string ResolvePieceName(byte p_token, byte p_brickId)
		{
			LPieceEntry? entry = FindByBrickId(p_token, p_brickId);
			return entry.HasValue ? entry.Value.Name : null;
		}
		public byte[] GetEntryGeometry(int p_index)
		{
			if (p_index < 0 || p_index >= m_entries.Count) throw new ArgumentOutOfRangeException(nameof(p_index));
			(string _, int start, int end) = m_entries[p_index]; byte[] result = new byte[end - start]; Buffer.BlockCopy(m_bytes, start, result, 0, result.Length); return result;
		}
		public void Save(Stream p_stream) { p_stream.Write(m_bytes, 0, m_bytes.Length); }
	}

	public struct LPieceEntry
	{
		public string Name;
		public byte Token;
		public byte BrickId;
		public uint VertexCount;
		public uint FaceCount;
		public uint GeometryOffset;
		public byte[] OpaqueBytes;
	}

	public sealed class ChampionEntry
	{
		public string ChassisTag { get; internal set; }
		public string ModelCode { get; internal set; }
		public string[] ExtraCodes { get; internal set; }
	}

	/// <summary>Raw-preserving CHAMPS.CCB view.</summary>
	public sealed class Champs
	{
		private readonly byte[] m_bytes;
		public IReadOnlyList<ChampionEntry> Champions { get; }
		public Champs(string p_path) : this(File.ReadAllBytes(p_path)) { }
		public Champs(Stream p_stream) : this(ReadAll(p_stream)) { }
		public Champs(byte[] p_bytes)
		{
			m_bytes = Copy(p_bytes);
			string temp = Path.GetTempFileName();
			try
			{
				File.WriteAllBytes(temp, m_bytes);
				CCB ccb = new CCB(temp);
				Champions = ccb.Cars.Values.Select(car => new ChampionEntry { ChassisTag = car.Unknown2B, ModelCode = car.Unknown29, ExtraCodes = new[] { car.Unknown28, car.Unknown2A }.Where(s => !string.IsNullOrEmpty(s)).ToArray() }).ToArray();
			}
			finally { File.Delete(temp); }
		}
		public void Save(Stream p_stream) { p_stream.Write(m_bytes, 0, m_bytes.Length); }
	}

	internal static class BuilderFormatSerialization
	{
		internal static byte[] Copy(byte[] p_bytes) { if (p_bytes == null) throw new ArgumentNullException(nameof(p_bytes)); return (byte[])p_bytes.Clone(); }
		internal static byte[] ReadAll(Stream p_stream) { using (MemoryStream output = new MemoryStream()) { p_stream.CopyTo(output); return output.ToArray(); } }
		internal static void WriteInt(BinaryWriter p_writer, int p_value) { p_writer.Write((byte)0x04); p_writer.Write(p_value); }
		internal static void WriteString(BinaryWriter p_writer, string p_value) { p_writer.Write((byte)0x02); WriteRawString(p_writer, p_value); }
		internal static void WriteRawString(BinaryWriter p_writer, string p_value) { p_writer.Write(Encoding.ASCII.GetBytes(p_value ?? string.Empty)); p_writer.Write((byte)0); }
		internal static string[] ParseStringArray(byte[] p_bytes, byte p_block) { int offset = Array.IndexOf(p_bytes, p_block); if (offset < 0) throw new InvalidDataException("Required block was not found."); offset++; Expect(p_bytes, ref offset, 0x07); Expect(p_bytes, ref offset, 0x04); int count = ReadInt(p_bytes, ref offset); Expect(p_bytes, ref offset, 0x08); Expect(p_bytes, ref offset, 0x05); Expect(p_bytes, ref offset, 0x14); ushort stringCount = ReadUShort(p_bytes, ref offset); if (stringCount != count) throw new InvalidDataException("String-array count mismatch."); Expect(p_bytes, ref offset, 0x02); string[] strings = new string[count]; for (int i = 0; i < count; i++) strings[i] = ReadRawString(p_bytes, ref offset); Expect(p_bytes, ref offset, 0x06); return strings; }
		internal static string ParseCSet(byte[] p_bytes, out List<(string PieceName, string ColorName)> p_entries) { int offset = 0; Expect(p_bytes, ref offset, 0x2E); Expect(p_bytes, ref offset, 0x04); ReadInt(p_bytes, ref offset); Expect(p_bytes, ref offset, 0x31); Expect(p_bytes, ref offset, 0x02); string chassis = ReadRawString(p_bytes, ref offset); string[] strings = ParseStringArrayAt(p_bytes, ref offset, 0x30); p_entries = new List<(string PieceName, string ColorName)>(strings.Length / 2); for (int i = 0; i < strings.Length; i += 2) p_entries.Add((strings[i], strings[i + 1])); return chassis; }
		private static string[] ParseStringArrayAt(byte[] p_bytes, ref int p_offset, byte p_block) { Expect(p_bytes, ref p_offset, p_block); Expect(p_bytes, ref p_offset, 0x07); Expect(p_bytes, ref p_offset, 0x04); int count = ReadInt(p_bytes, ref p_offset); Expect(p_bytes, ref p_offset, 0x08); Expect(p_bytes, ref p_offset, 0x05); Expect(p_bytes, ref p_offset, 0x14); ushort stringCount = ReadUShort(p_bytes, ref p_offset); if (stringCount != count * 2) throw new InvalidDataException("CSET string count mismatch."); Expect(p_bytes, ref p_offset, 0x02); string[] strings = new string[stringCount]; for (int i = 0; i < stringCount; i++) strings[i] = ReadRawString(p_bytes, ref p_offset); Expect(p_bytes, ref p_offset, 0x06); return strings; }
		internal static PieceDatabase LoadPieceDatabase(byte[] p_bytes) { return new PieceDatabase(p_bytes); }
		private static int ReadInt(byte[] p_bytes, ref int p_offset) { int result = BitConverter.ToInt32(p_bytes, p_offset); p_offset += 4; return result; }
		private static ushort ReadUShort(byte[] p_bytes, ref int p_offset) { ushort result = BitConverter.ToUInt16(p_bytes, p_offset); p_offset += 2; return result; }
		private static string ReadRawString(byte[] p_bytes, ref int p_offset) { int start = p_offset; while (p_offset < p_bytes.Length && p_bytes[p_offset] != 0) p_offset++; if (p_offset == p_bytes.Length) throw new EndOfStreamException(); string result = Encoding.ASCII.GetString(p_bytes, start, p_offset - start); p_offset++; return result; }
		private static void Expect(byte[] p_bytes, ref int p_offset, byte p_value) { if (p_offset >= p_bytes.Length || p_bytes[p_offset++] != p_value) throw new InvalidDataException("Malformed builder-format stream."); }
	}
}
