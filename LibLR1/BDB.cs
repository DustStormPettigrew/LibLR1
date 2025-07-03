using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class BDB
	{
		private const byte
			ID_UNKNOWN_27     = 0x27,
			ID_UNKNOWN_28     = 0x28,
			ID_UNKNOWN_29     = 0x29,
			ID_BOUNDING_BOXES = 0x2A,
			ID_UNKNOWN_2B     = 0x2B;
		
		private BDB_Unknown27[]   m_unknown27;
		private BDB_BoundingBox[] m_boundingBoxes;
		private int[]             m_unknown2B;
		
		public BDB_Unknown27[] Unknown27
		{
			get { return m_unknown27; }
			set { m_unknown27 = value; }
		}
		public BDB_BoundingBox[] BoundingBoxes
		{
			get { return m_boundingBoxes; }
			set { m_boundingBoxes = value; }
		}
		public int[] Unknown2B
		{
			get { return m_unknown2B; }
			set { m_unknown2B = value; }
		}

		public BDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public BDB(LRBinaryReader p_reader)
		{
			m_unknown2B = new int[0];
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_UNKNOWN_27:
					{
						m_unknown27 = p_reader.ReadArrayBlock<BDB_Unknown27>(
							BDB_Unknown27.Read
						);
						break;
					}
					case ID_BOUNDING_BOXES:
					{
						m_boundingBoxes = p_reader.ReadArrayBlock<BDB_BoundingBox>(
							BDB_BoundingBox.Read
						);
						break;
					}
					case ID_UNKNOWN_2B:
					{
						m_unknown2B = p_reader.ReadIntArrayBlock();
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
	
	public abstract class BDB_Unknown27
	{
		public const byte
			ID_UNKNOWN_27 = 0x27,
			ID_UNKNOWN_28 = 0x28,
			ID_UNKNOWN_29 = 0x29;
		
		public virtual byte Type { get { return 0; } }

		public static BDB_Unknown27 Read(LRBinaryReader p_reader)
		{
			byte type = p_reader.ReadByte();
			switch (type)
			{
				case ID_UNKNOWN_28:
				{
					return BDB_Unknown28.Read(p_reader);
				}
				case ID_UNKNOWN_29:
				{
					return BDB_Unknown29.Read(p_reader);
				}
				default:
				{
					throw new UnexpectedBlockException(
						type,
						p_reader.BaseStream.Position - 1
					);
				}
			}
		}
	}
	
	public class BDB_Unknown28 : BDB_Unknown27
	{
		public override byte Type
		{
			get { return ID_UNKNOWN_28; }
		}
		
		public int   a, b, c;
		public float d, e, f, g;

		public static new BDB_Unknown28 Read(LRBinaryReader p_reader)
		{
			BDB_Unknown28 val = new BDB_Unknown28();
			val.a = p_reader.ReadIntegralWithHeader();
			val.b = p_reader.ReadIntegralWithHeader();
			val.c = p_reader.ReadIntegralWithHeader();
			val.d = p_reader.ReadFloatWithHeader();
			val.e = p_reader.ReadFloatWithHeader();
			val.d = p_reader.ReadFloatWithHeader();
			val.g = p_reader.ReadFloatWithHeader();
			return val;
		}
	}
	
	public class BDB_Unknown29 : BDB_Unknown27
	{
		public override byte Type
		{
			get { return ID_UNKNOWN_29; }
		}
		
		public ushort     a, b, c;
		public Fract16Bit d, e, f;

		public static new BDB_Unknown29 Read(LRBinaryReader p_reader)
		{
			BDB_Unknown29 val = new BDB_Unknown29();
			val.a = p_reader.ReadUShortWithHeader();
			val.b = p_reader.ReadUShortWithHeader();
			val.c = p_reader.ReadUShortWithHeader();
			val.d = p_reader.ReadFract16BitWithHeader();
			val.e = p_reader.ReadFract16BitWithHeader();
			val.f = p_reader.ReadFract16BitWithHeader();
			return val;
		}
	}
	
	public class BDB_BoundingBox
	{
		public LRVector3 MinPoint, MaxPoint;

		public static BDB_BoundingBox Read(LRBinaryReader p_reader)
		{
			BDB_BoundingBox val = new BDB_BoundingBox();
			val.MinPoint = LRVector3.Read(p_reader);
			val.MaxPoint = LRVector3.Read(p_reader);
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, BDB_BoundingBox p_value)
		{
			LRVector3.Write(p_writer, p_value.MinPoint);
			LRVector3.Write(p_writer, p_value.MaxPoint);
		}
	}
}