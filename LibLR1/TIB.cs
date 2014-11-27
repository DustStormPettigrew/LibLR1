using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class TIB
	{
		private const byte
			ID_UNKNOWN_27 = 0x27;

		private TIB_Unknown27[] m_unknown27;

		public TIB_Unknown27[] Unknown27
		{
			get { return m_unknown27; }
			set { m_unknown27 = value; }
		}

		public TIB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public TIB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_UNKNOWN_27:
					{
						m_unknown27 = p_reader.ReadStructArrayBlock<TIB_Unknown27>(
							TIB_Unknown27.Read,
							ID_UNKNOWN_27
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(
							blockId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
		}
	}

	public class TIB_Unknown27
	{
		private const byte
			PROPERTY_28 = 0x28,
			PROPERTY_29 = 0x29,
			PROPERTY_2A = 0x2A,
			PROPERTY_2B = 0x2B,
			PROPERTY_2D = 0x2D;

		public int Int28;
		public int Int29;
		public int Int2A;
		public int Int2D;

		public static TIB_Unknown27 Read(LRBinaryReader p_reader)
		{
			TIB_Unknown27 val = new TIB_Unknown27();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_28:
					{
						if (!p_reader.Next(Token.Int32))
						{
							p_reader.BaseStream.Position++;  // 0x2B bodge
						}
						val.Int28 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_29:
					{
						if (!p_reader.Next(Token.Int32))
						{
							p_reader.BaseStream.Position++;  // 0x2B bodge
						}
						val.Int29 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_2A:
					{
						if (!p_reader.Next(Token.Int32))
						{
							p_reader.BaseStream.Position++;  // 0x2B bodge
						}
						val.Int2A = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_2D:
					{
						if (!p_reader.Next(Token.Int32))
						{
							p_reader.BaseStream.Position++;  // 0x2B bodge
						}
						val.Int2D = p_reader.ReadIntWithHeader();
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(
							propertyId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
			return val;
		}
	}
}