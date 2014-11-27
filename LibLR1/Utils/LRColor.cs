using LibLR1.IO;

namespace LibLR1.Utils
{
	public class LRColor
	{
		private byte m_r, m_g, m_b, m_a;
		
		public byte R { get { return m_r; } set { m_r = value; } }
		public byte G { get { return m_g; } set { m_g = value; } }
		public byte B { get { return m_b; } set { m_b = value; } }
		public byte A { get { return m_a; } set { m_a = value; } }
		
		public LRColor()
			: this(0, 0, 0, 0)
		{
		}

		public LRColor(byte p_r, byte p_g, byte p_b, byte p_a)
		{
			m_r = p_r;
			m_g = p_g;
			m_b = p_b;
			m_a = p_a;
		}

		public static LRColor Read(LRBinaryReader p_reader)
		{
			LRColor output = new LRColor();
			output.R = (byte)p_reader.ReadIntegralWithHeader();
			output.G = (byte)p_reader.ReadIntegralWithHeader();
			output.B = (byte)p_reader.ReadIntegralWithHeader();
			output.A = (byte)p_reader.ReadIntegralWithHeader();
			return output;
		}

		public static LRColor ReadNoAlpha(LRBinaryReader p_reader)
		{
			LRColor output = new LRColor();
			output.R = (byte)p_reader.ReadIntegralWithHeader();
			output.G = (byte)p_reader.ReadIntegralWithHeader();
			output.B = (byte)p_reader.ReadIntegralWithHeader();
			return output;
		}

		public static void Write(LRBinaryWriter p_writer, LRColor p_value)
		{
			WriteNoAlpha(p_writer, p_value);
			p_writer.WriteByteWithHeader(p_value.A);
		}

		public static void WriteNoAlpha(LRBinaryWriter p_writer, LRColor p_value)
		{
			p_writer.WriteByteWithHeader(p_value.R);
			p_writer.WriteByteWithHeader(p_value.G);
			p_writer.WriteByteWithHeader(p_value.B);
		}
	}
}