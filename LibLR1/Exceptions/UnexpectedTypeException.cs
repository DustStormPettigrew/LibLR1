using LibLR1.Utils;
using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedTypeException : Exception
	{
		public UnexpectedTypeException(Token p_typeId, long p_streamPosition)
			: base(string.Format("Unexpected type 0x{0:X2} at 0x{1:X8}", p_typeId, p_streamPosition))
		{
		}
	}
}