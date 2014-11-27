using System.IO;
using LibLR1.Utils;

namespace LibLR1 {
    public class SRF {
        private string[] m_Strings;

        public string[] Strings { get { return m_Strings; } set { m_Strings = value; } }

        public SRF(Stream stream) {
            const int SIZE_USHORT = sizeof(ushort);
            ushort numStrings = BinaryFileHelper.ReadUShort(stream);
            ushort numChars = BinaryFileHelper.ReadUShort(stream);
            ushort[] offsets = new ushort[numStrings];
            m_Strings = new string[numStrings];
            for (int i = 0; i < numStrings; i++)
                offsets[i] = BinaryFileHelper.ReadUShort(stream);
            for (int i = 0; i < numStrings; i++) {
                stream.Position = (SIZE_USHORT * (offsets[i] + numStrings + 2));
                m_Strings[i] = ReadNullTerminatedUCS2String(stream);
            }
        }

        public SRF(string path)
            : this(new FileStream(path, FileMode.Open, FileAccess.Read)) { }

        public void Save(Stream stream) {
            ushort numStrings = (ushort)m_Strings.Length;
            ushort numChars = 0;
            for (int i = 0; i < m_Strings.Length; i++)
                numChars += (ushort)(m_Strings[i].Length + 1);
            BinaryFileHelper.WriteUShort(stream, numStrings);
            BinaryFileHelper.WriteUShort(stream, numChars);
            for (int i = 0; i < m_Strings.Length; i++) {
                ushort offset = 0;
                for (int j = 0; j < i; j++)
                    offset += (ushort)(m_Strings[j].Length + 1);
                BinaryFileHelper.WriteUShort(stream, offset);
            }
            for (int i = 0; i < m_Strings.Length; i++) {
                WriteNullTerminatedUCS2String(stream, m_Strings[i]);
            }
        }

        public void Save(string path) {
            Save(new FileStream(path, FileMode.Create, FileAccess.Write));
        }

        private static string ReadNullTerminatedUCS2String(Stream stream) {
            string buffer = "";
            ushort currChar;
            while ((currChar = BinaryFileHelper.ReadUShort(stream)) != 0)
                buffer += (char)currChar;
            return buffer;
        }

        private static void WriteNullTerminatedUCS2String(Stream stream, string value) {
            for (int c = 0; c < value.Length; c++)
                BinaryFileHelper.WriteUShort(stream, (ushort)value[c]);
            BinaryFileHelper.WriteUShort(stream, 0);    // null-terminator
        }
    }
}