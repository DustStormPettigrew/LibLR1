using System.IO;

namespace LibLR1.Utils
{
	public class LRRect
	{
		private int m_X, m_Y, m_Width, m_Height;
		
		public int X      { get { return m_X;      } set { m_X      = value; } }
		public int Y      { get { return m_Y;      } set { m_Y      = value; } }
		public int Width  { get { return m_Width;  } set { m_Width  = value; } }
		public int Height { get { return m_Height; } set { m_Height = value; } }
		
		public LRRect()
			: this(0, 0, 0, 0)
		{
		}
		
		public LRRect(int x, int y, int width, int height)
		{
			m_X      = x;
			m_Y      = y;
			m_Width  = width;
			m_Height = height;
		}
		
		public static LRRect FromStream(Stream stream)
		{
			LRRect output = new LRRect();
			output.X      = BinaryFileHelper.ReadIntWithHeader(stream);
			output.Y      = BinaryFileHelper.ReadIntWithHeader(stream);
			output.Width  = BinaryFileHelper.ReadIntWithHeader(stream);
			output.Height = BinaryFileHelper.ReadIntWithHeader(stream);
			return output;
		}
		
		public static void ToStream(Stream stream, LRRect rect)
		{
			BinaryFileHelper.WriteIntWithHeader(stream, rect.X);
			BinaryFileHelper.WriteIntWithHeader(stream, rect.Y);
			BinaryFileHelper.WriteIntWithHeader(stream, rect.Width);
			BinaryFileHelper.WriteIntWithHeader(stream, rect.Height);
		}
	}
}