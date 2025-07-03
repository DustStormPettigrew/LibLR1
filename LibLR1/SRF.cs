using LibLR1.IO;
using System.IO;

namespace LibLR1 {
	public class SRF
	{
		private string[] m_strings;

		public string[] Strings { get { return m_strings; } set { m_strings = value; } }

		public SRF(string p_filepath)
			: this(new LRBinaryReader(File.OpenRead(p_filepath)))
		{
		}

		public SRF(LRBinaryReader p_reader)
		{
			const int SIZE_USHORT = sizeof(ushort);
			ushort numStrings = p_reader.ReadUShort();
			ushort numChars = p_reader.ReadUShort();
			ushort[] offsets = new ushort[numStrings];
			m_strings = new string[numStrings];
			for (int i = 0; i < numStrings; i++)
			{
				offsets[i] = p_reader.ReadUShort();
			}
			for (int i = 0; i < numStrings; i++)
			{
				p_reader.BaseStream.Position = (SIZE_USHORT * (offsets[i] + numStrings + 2));
				m_strings[i] = ReadNullTerminatedUCS2String(p_reader);
			}
		}

		public void Save(string p_filepath)
		{
			using (LRBinaryWriter writer = new LRBinaryWriter(File.OpenWrite(p_filepath)))
			{
				Save(writer);
			}
		}

		public void Save(LRBinaryWriter p_writer)
		{
			ushort numStrings = (ushort)m_strings.Length;
			ushort numChars = 0;
			for (int i = 0; i < m_strings.Length; i++)
			{
				numChars += (ushort)(m_strings[i].Length + 1);
			}
			p_writer.WriteUShort(numStrings);
			p_writer.WriteUShort(numChars);
			for (int i = 0; i < m_strings.Length; i++)
			{
				ushort offset = 0;
				for (int j = 0; j < i; j++)
				{
					offset += (ushort)(m_strings[j].Length + 1);
				}
				p_writer.WriteUShort(offset);
			}
			for (int i = 0; i < m_strings.Length; i++)
			{
				WriteNullTerminatedUCS2String(p_writer, m_strings[i]);
			}
		}

		private static string ReadNullTerminatedUCS2String(LRBinaryReader p_reader)
		{
			string buffer = "";
			ushort currChar;
			while ((currChar = p_reader.ReadUShort()) != 0)
			{
				buffer += (char)currChar;
			}
			return buffer;
		}

		private static void WriteNullTerminatedUCS2String(LRBinaryWriter p_writer, string p_value)
		{
			for (int c = 0; c < p_value.Length; c++)
			{
				p_writer.WriteUShort((ushort)p_value[c]);
			}
			p_writer.WriteUShort(0);   // null-terminator
		}
	}
}