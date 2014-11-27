using System.IO;

namespace LibLR1.Utils
{
	public class LRColor
	{
		private byte m_R, m_G, m_B, m_A;
		
		public byte R { get { return m_R; } set { m_R = value; } }
		public byte G { get { return m_G; } set { m_G = value; } }
		public byte B { get { return m_B; } set { m_B = value; } }
		public byte A { get { return m_A; } set { m_A = value; } }
		
		public LRColor()
			: this(0, 0, 0, 0)
		{
		}
		
		public LRColor(byte r, byte g, byte b, byte a)
		{
			m_R = r;
			m_G = g;
			m_B = b;
			m_A = a;
		}
		
		public static LRColor FromStream(Stream stream)
		{
			LRColor output = new LRColor();
			output = FromStreamNoAlpha(stream);
			byte t_a = BinaryFileHelper.Expect(stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_a)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.A =       BinaryFileHelper.ReadByte(stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.A = (byte)BinaryFileHelper.ReadInt(stream);  break; }
			}
			return output;
		}
		
		public static LRColor FromStreamNoAlpha(Stream stream)
		{
			LRColor output = new LRColor();
			byte t_r = BinaryFileHelper.Expect(stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_r)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.R =       BinaryFileHelper.ReadByte(stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.R = (byte)BinaryFileHelper.ReadInt(stream);  break; }
			}
			byte t_g = BinaryFileHelper.Expect(stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_g)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.G =       BinaryFileHelper.ReadByte(stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.G = (byte)BinaryFileHelper.ReadInt(stream);  break; }
			}
			byte t_b = BinaryFileHelper.Expect(stream, new byte[] { BinaryFileHelper.TYPE_BYTE, BinaryFileHelper.TYPE_INT32 });
			switch (t_b)
			{
				case BinaryFileHelper.TYPE_BYTE:  { output.B =       BinaryFileHelper.ReadByte(stream); break; }
				case BinaryFileHelper.TYPE_INT32: { output.B = (byte)BinaryFileHelper.ReadInt(stream);  break; }
			}
			output.A = 0xFF;
			return output;
		}
		
		public static void ToStream(Stream stream, LRColor color)
		{
			ToStreamNoAlpha(stream, color);
			BinaryFileHelper.WriteByteWithHeader(stream, color.A);
		}
		
		public static void ToStreamNoAlpha(Stream stream, LRColor color)
		{
			BinaryFileHelper.WriteByteWithHeader(stream, color.R);
			BinaryFileHelper.WriteByteWithHeader(stream, color.G);
			BinaryFileHelper.WriteByteWithHeader(stream, color.B);
		}
	}
}