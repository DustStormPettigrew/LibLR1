using System.IO;

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

		public static LRQuaternion FromStream(Stream p_stream)
		{
			LRQuaternion val = new LRQuaternion();
			val.X = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			val.Y = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			val.Z = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			val.W = BinaryFileHelper.ReadFloatWithHeader(p_stream);
			return val;
		}

		public static void ToStream(Stream p_stream, LRQuaternion p_quaternion)
		{
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_quaternion.X);
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_quaternion.Y);
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_quaternion.Z);
			BinaryFileHelper.WriteFloatWithHeader(p_stream, p_quaternion.W);
		}
	}
}