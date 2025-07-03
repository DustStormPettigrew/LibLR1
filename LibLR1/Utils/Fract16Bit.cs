using LibLR1.IO;

namespace LibLR1.Utils
{
	public class Fract16Bit
	{
		private short m_value;
		
		public short Value { get { return m_value; } set { m_value = value; } }
		public float AsFloat
		{
			get { return m_value / 256f; }
			set { m_value = (short)(value * 256); }
		}
		
		public Fract16Bit()
			: this(0f)
		{
		}

		public Fract16Bit(float p_value)
		{
			AsFloat = p_value;
		}

		public Fract16Bit(short p_value)
		{
			m_value = p_value;
		}

		public static Fract16Bit Read(LRBinaryReader p_reader)
		{
			return new Fract16Bit(p_reader.ReadShort());
		}

		public static void Write(LRBinaryWriter p_writer, Fract16Bit p_value)
		{
			p_writer.WriteShort(p_value.Value);
		}
	}
}