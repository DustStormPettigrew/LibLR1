using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedTypeException : Exception
	{
		public UnexpectedTypeException(byte type_id, long stream_position)
			: base(string.Format("Unexpected type 0x{0:X2} at 0x{1:X8}", type_id, stream_position))
		{
		}
	}
}