using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedBlockException : Exception
	{
		public UnexpectedBlockException(byte block_id, long stream_position)
			: base(string.Format("Unexpected block 0x{0:X2} at 0x{1:X8}", block_id, stream_position))
		{
		}
	}
}