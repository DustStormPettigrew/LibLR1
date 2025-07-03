
namespace LibLR1.Utils
{
	public enum Token
	{
		String       = 0x02,
		Float        = 0x03,
		Int32        = 0x04,
		LeftCurly    = 0x05,
		RightCurly   = 0x06,
		LeftBracket  = 0x07,
		RightBracket = 0x08,
		Fract8       = 0x0B,
		SByte        = 0x0B,
		Byte         = 0x0C,
		Fract16      = 0x0D,
		Short        = 0x0D,
		UShort       = 0x0E,
		Array        = 0x14, // compression pass only
		Struct       = 0x16, // compression pass only
	}
}
