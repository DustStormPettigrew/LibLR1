using System.IO;

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
		
		public static LRColor FromStream(Stream p_stream)
		{
			LRColor output = new LRColor();
			output = FromStreamNoAlpha(p_stream);
			byte t_a = BinaryFileHelper.Expect(p_stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_a)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.A =       BinaryFileHelper.ReadByte(p_stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.A = (byte)BinaryFileHelper.ReadInt(p_stream);  break; }
			}
			return output;
		}
		
		public static LRColor FromStreamNoAlpha(Stream p_stream)
		{
			LRColor output = new LRColor();
			byte t_r = BinaryFileHelper.Expect(p_stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_r)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.R =       BinaryFileHelper.ReadByte(p_stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.R = (byte)BinaryFileHelper.ReadInt(p_stream);  break; }
			}
			byte t_g = BinaryFileHelper.Expect(p_stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_g)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.G =       BinaryFileHelper.ReadByte(p_stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.G = (byte)BinaryFileHelper.ReadInt(p_stream);  break; }
			}
			byte t_b = BinaryFileHelper.Expect(p_stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_b)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.B =       BinaryFileHelper.ReadByte(p_stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.B = (byte)BinaryFileHelper.ReadInt(p_stream);  break; }
			}
			output.A = 0xFF;
			return output;
		}
		
		public static void ToStream(Stream p_stream, LRColor p_color)
		{
			ToStreamNoAlpha(p_stream, p_color);
			BinaryFileHelper.WriteByteWithHeader(p_stream, p_color.A);
		}
		
		public static void ToStreamNoAlpha(Stream p_stream, LRColor p_color)
		{
			BinaryFileHelper.WriteByteWithHeader(p_stream, p_color.R);
			BinaryFileHelper.WriteByteWithHeader(p_stream, p_color.G);
			BinaryFileHelper.WriteByteWithHeader(p_stream, p_color.B);
		}
	}
}