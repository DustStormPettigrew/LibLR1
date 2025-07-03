using LibLR1.IO;

namespace LibLR1.Utils
{
	public class LRVector3
	{
		private float m_x, m_y, m_z;
		
		public float X { get { return m_x; } set { m_x = value; } }
		public float Y { get { return m_y; } set { m_y = value; } }
		public float Z { get { return m_z; } set { m_z = value; } }
		
		public LRVector3()
			: this(0, 0, 0)
		{
		}

		public LRVector3(float p_x, float p_y, float p_z)
		{
			m_x = p_x;
			m_y = p_y;
			m_z = p_z;
		}

		public static LRVector3 Read(LRBinaryReader p_reader)
		{
			LRVector3 val = new LRVector3();
			val.X = p_reader.ReadFloatWithHeader();
			val.Y = p_reader.ReadFloatWithHeader();
			val.Z = p_reader.ReadFloatWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, LRVector3 p_vec)
		{
			p_writer.WriteFloatWithHeader(p_vec.X);
			p_writer.WriteFloatWithHeader(p_vec.Y);
			p_writer.WriteFloatWithHeader(p_vec.Z);
		}
		
		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", m_x, m_y, m_z);
		}
	}
}