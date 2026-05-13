using LibLR1.IO;

namespace LibLR1.Utils
{
	public class LRVector2
	{
		private float m_x, m_y;
		
		public float X { get { return m_x; } set { m_x = value; } }
		public float Y { get { return m_y; } set { m_y = value; } }
		
		public LRVector2()
			: this(0, 0)
		{
		}

		public LRVector2(float p_x, float p_y)
		{
			m_x = p_x;
			m_y = p_y;
		}

		public static LRVector2 Read(LRBinaryReader p_reader)
		{
			LRVector2 val = new LRVector2();
			val.X = p_reader.ReadFloatWithHeader();
			val.Y = p_reader.ReadFloatWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, LRVector2 p_vec)
		{
			p_writer.WriteFloatWithHeader(p_vec.X);
			p_writer.WriteFloatWithHeader(p_vec.Y);
		}
		
		public override string ToString()
		{
			return string.Format("({0}, {1})", m_x, m_y);
		}
	}
}