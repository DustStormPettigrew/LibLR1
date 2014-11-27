using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedPropertyException : Exception
	{
		public UnexpectedPropertyException(byte property_id, long stream_position)
			: base(string.Format("Unexpected property 0x{0:X2} at 0x{1:X8}", property_id, stream_position))
		{
		}
	}
}