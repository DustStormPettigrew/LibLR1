using LibLR1.IO;

namespace LibLR1.Utils
{
	public class LRQuaternion
	{
		private float m_x, m_y, m_z, m_w;
		
		public float X { get { return m_x; } set { m_x = value; } }
		public float Y { get { return m_y; } set { m_y = value; } }
		public float Z { get { return m_z; } set { m_z = value; } }
		public float W { get { return m_w; } set { m_w = value; } }
		
		public LRQuaternion()
			: this(0, 0, 0, 1)
		{
		}

		public LRQuaternion(float p_x, float p_y, float p_z, float p_w)
		{
			m_x = p_x;
			m_y = p_y;
			m_z = p_z;
			m_w = p_w;
		}

		public static LRQuaternion Read(LRBinaryReader p_reader)
		{
			LRQuaternion val = new LRQuaternion();
			val.X = p_reader.ReadFloatWithHeader();
			val.Y = p_reader.ReadFloatWithHeader();
			val.Z = p_reader.ReadFloatWithHeader();
			val.W = p_reader.ReadFloatWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, LRQuaternion p_quaternion)
		{
			p_writer.WriteFloatWithHeader(p_quaternion.X);
			p_writer.WriteFloatWithHeader(p_quaternion.Y);
			p_writer.WriteFloatWithHeader(p_quaternion.Z);
			p_writer.WriteFloatWithHeader(p_quaternion.W);
		}
	}
}