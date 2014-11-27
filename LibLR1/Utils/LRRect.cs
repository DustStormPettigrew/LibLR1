using LibLR1.IO;

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

		public static LRRect Read(LRBinaryReader p_reader)
		{
			LRRect output = new LRRect();
			output.X      = p_reader.ReadIntWithHeader();
			output.Y      = p_reader.ReadIntWithHeader();
			output.Width  = p_reader.ReadIntWithHeader();
			output.Height = p_reader.ReadIntWithHeader();
			return output;
		}

		public static void Write(LRBinaryWriter p_writer, LRRect p_rect)
		{
			p_writer.WriteIntWithHeader(p_rect.X);
			p_writer.WriteIntWithHeader(p_rect.Y);
			p_writer.WriteIntWithHeader(p_rect.Width);
			p_writer.WriteIntWithHeader(p_rect.Height);
		}
	}
}