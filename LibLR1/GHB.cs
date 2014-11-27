using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class GHB
	{
		private const byte
			ID_GHOST_PATH = 0x2C;
		
		private GHB_GhostPath m_ghostPath;

		public GHB_GhostPath GhostPath
		{
			get { return m_ghostPath; }
			set { m_ghostPath = value; }
		}
		
		public GHB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public GHB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_GHOST_PATH:
					{
						m_ghostPath = p_reader.ReadStruct<GHB_GhostPath>(
							GHB_GhostPath.Read
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
	
	public class GHB_GhostPath
	{
		private const byte
			PROPERTY_NODES      = 0x27,
			PROPERTY_POSITION   = 0x28,
			PROPERTY_ROTATION   = 0x29,
			PROPERTY_UNKNOWN_2A = 0x2A,
			PROPERTY_UNKNOWN_2B = 0x2B;
		
		public GHB_Node[]   Nodes;
		public LRVector3    InitialPosition;
		public LRQuaternion InitialRotation;
		public int[]        Unknown2A;
		public int          Unknown2B;
		
		public static GHB_GhostPath Read(LRBinaryReader p_reader)
		{
			GHB_GhostPath val = new GHB_GhostPath();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_NODES:
					{
						val.Nodes = p_reader.ReadArrayBlock<GHB_Node>(
							GHB_Node.Read
						);
						break;
					}
					case PROPERTY_POSITION:
					{
						val.InitialPosition = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_ROTATION:
					{
						val.InitialRotation = LRQuaternion.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = new int[3];
						for (int i = 0; i < val.Unknown2A.Length; i++)
						{
							val.Unknown2A[i] = p_reader.ReadIntWithHeader();
						}
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = p_reader.ReadIntWithHeader();
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
	
	public class GHB_Node
	{
		public Fract16Bit px, py, pz;
		public Fract8Bit  rx, ry, rz, rw;
		
		public static GHB_Node Read(LRBinaryReader p_reader)
		{
			GHB_Node val = new GHB_Node();
			val.px = p_reader.ReadFract16BitWithHeader();
			val.py = p_reader.ReadFract16BitWithHeader();
			val.pz = p_reader.ReadFract16BitWithHeader();
			val.rx = p_reader.ReadFract8BitWithHeader();
			val.ry = p_reader.ReadFract8BitWithHeader();
			val.rz = p_reader.ReadFract8BitWithHeader();
			val.rw = p_reader.ReadFract8BitWithHeader();
			return val;
		}
	}
}