using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibLR1.Utils
{
	public enum Token
	{
		STRING        = 0x02,
		FLOAT         = 0x03,
		INT32         = 0x04,
		LEFT_CURLY    = 0x05,
		RIGHT_CURLY   = 0x06,
		LEFT_BRACKET  = 0x07,
		RIGHT_BRACKET = 0x08,
		FRACT8        = 0x0B,
		SBYTE         = 0x0B,
		BYTE          = 0x0C,
		FRACT16       = 0x0D,
		SHORT         = 0x0D,
		USHORT        = 0x0E,
		ARRAY         = 0x14, // compression pass only
		STRUCT        = 0x16, // compression pass only
	}
}
