using System;

namespace LibLR1.Exceptions
{
	public class UnexpectedPropertyException : Exception
	{
		public UnexpectedPropertyException(byte p_propertyId, long p_streamPosition)
			: base(string.Format("Unexpected property 0x{0:X2} at 0x{1:X8}", p_propertyId, p_streamPosition))
		{
		}
	}
}