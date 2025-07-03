using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class TRB
	{
		private const byte
			ID_UNKNOWN_27 = 0x27;

		private TRB_Unknown27[] m_unknown27;

		public TRB_Unknown27[] Unknown27
		{
			get { return m_unknown27; }
			set { m_unknown27 = value; }
		}

		public TRB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public TRB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_UNKNOWN_27:
					{
						m_unknown27 = p_reader.ReadStructArrayBlock<TRB_Unknown27>(
							TRB_Unknown27.Read,
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

	public class TRB_Unknown27
	{
		private const byte
			PROPERTY_29 = 0x29,
			PROPERTY_2A = 0x2A,
			PROPERTY_2B = 0x2B,
			PROPERTY_2D = 0x2D,
			PROPERTY_2E = 0x2E,
			PROPERTY_2F = 0x2F;

		public LRVector3 Vec29;
		public float     Float2A;
		public int       Int2B;
		public string    String2D;
		public int       Int2E;
		public bool      Bool2F;

		public static TRB_Unknown27 Read(LRBinaryReader p_reader)
		{
			TRB_Unknown27 val = new TRB_Unknown27();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_29:
					{
						val.Vec29 = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_2A:
					{
						val.Float2A = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_2B:
					{
						val.Int2B = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_2D:
					{
						val.String2D = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_2E:
					{
						val.Int2E = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_2F:
					{
						val.Bool2F = true;
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