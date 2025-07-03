using LibLR1.IO;

namespace LibLR1.Utils
{
	public class Fract8Bit
	{
		private sbyte m_value;
		
		public sbyte Value { get { return m_value; } set { m_value = value; } }
		public float AsFloat
		{
			get { return m_value / 16f; }
			set { m_value = (sbyte)(value * 16); }
		}
		
		public Fract8Bit()
			: this(0f)
		{
		}
		
		public Fract8Bit(float p_value)
		{
			AsFloat = p_value;
		}
		
		public Fract8Bit(sbyte p_value)
		{
			m_value = p_value;
		}

		public static Fract8Bit Read(LRBinaryReader p_reader)
		{
			return new Fract8Bit(p_reader.ReadSByte());
		}

		public static void Write(LRBinaryWriter p_writer, Fract8Bit p_value)
		{
			p_writer.WriteSByte(p_value.Value);
		}
	}
}