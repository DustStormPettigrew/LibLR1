using System;

namespace LibLR1.Exceptions
{
	public class InvalidLRSMagicException : Exception
	{
		public InvalidLRSMagicException(byte p_b0, byte p_b1)
			: base(string.Format(
				"Expected LRS magic \"LR\" (0x4C 0x52); got 0x{0:X2} 0x{1:X2}",
				p_b0, p_b1))
		{
		}
	}
}
