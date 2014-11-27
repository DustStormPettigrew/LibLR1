using System.IO;

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

		public static LRVector3 FromStream(Stream p_stream)
		{
			LRVector3 val = new LRVector3();
			val.X = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			val.Y = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			val.Z = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			return val;
		}

		public static void ToStream(Stream p_stream, LRVector3 p_vec)
		{
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_vec.X);
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_vec.Y);
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_vec.Z);
		}
		
		public override string ToString()
		{
			return string.Format("({0}, {1}, {2})", p_x, p_y, p_z);
		}
	}
}