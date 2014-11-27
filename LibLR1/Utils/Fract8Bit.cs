using LibLR1.IO;
using System;
using System.IO;

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

		[Obsolete]
		public static Fract8Bit FromStream(Stream p_stream)
		{
			return new Fract8Bit(BinaryFileHelper.ReadSByte(p_stream));
		}

		public static Fract8Bit Read(LRBinaryReader p_reader)
		{
			return new Fract8Bit(p_reader.ReadSByte());
		}
	}
}