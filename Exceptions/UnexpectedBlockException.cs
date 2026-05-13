using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedBlockException : Exception
	{
		public UnexpectedBlockException(byte p_blockId, long p_streamPosition)
			: base(string.Format("Unexpected block 0x{0:X2} at 0x{1:X8}", p_blockId, p_streamPosition))
		{
		}
	}
}