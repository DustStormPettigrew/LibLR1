using System.IO;

namespace LibLR1.Utils
{
	public class LRRect
	{
		private int m_x, m_y, m_width, m_height;
		
		public int X      { get { return m_x;      } set { m_x      = value; } }
		public int Y      { get { return m_y;      } set { m_y      = value; } }
		public int Width  { get { return m_width;  } set { m_width  = value; } }
		public int Height { get { return m_height; } set { m_height = value; } }
		
		public LRRect()
			: this(0, 0, 0, 0)
		{
		}

		public LRRect(int p_x, int p_y, int p_width, int p_height)
		{
			m_x      = p_x;
			m_y      = p_y;
			m_width  = p_width;
			m_height = p_height;
		}

		public static LRRect FromStream(Stream p_stream)
		{
			LRRect output = new LRRect();
			output.X      = BinaryFileHelper.ReadIntWithHeader(p_stream);
			output.Y      = BinaryFileHelper.ReadIntWithHeader(p_stream);
			output.Width  = BinaryFileHelper.ReadIntWithHeader(p_stream);
			output.Height = BinaryFileHelper.ReadIntWithHeader(p_stream);
			return output;
		}

		public static void ToStream(Stream p_stream, LRRect p_rect)
		{
			BinaryFileHelper.WriteIntWithHeader(p_stream, p_rect.X);
			BinaryFileHelper.WriteIntWithHeader(p_stream, p_rect.Y);
			BinaryFileHelper.WriteIntWithHeader(p_stream, p_rect.Width);
			BinaryFileHelper.WriteIntWithHeader(p_stream, p_rect.Height);
		}
	}
}