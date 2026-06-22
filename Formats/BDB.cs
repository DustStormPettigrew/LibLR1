using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	/// <summary>
	/// Bounding box / visibility tree database format.
	/// </summary>
	public class BDB
	{
		private const byte
			ID_TREE = 0x27,
			ID_BOUNDING_BOXES = 0x2A,
			ID_VISIBLE_REGIONS = 0x2B;

		private BDB_TreeNode[] m_tree;
		private BDB_BoundingBox[] m_boundingBoxes;
		private int[] m_visibleRegions;

		public BDB_TreeNode[] Tree
		{
			get { return m_tree; }
			set { m_tree = value; }
		}
		public BDB_BoundingBox[] BoundingBoxes
		{
			get { return m_boundingBoxes; }
			set { m_boundingBoxes = value; }
		}
		public int[] VisibleRegions
		{
			get { return m_visibleRegions; }
			set { m_visibleRegions = value; }
		}

		public BDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public BDB(LRBinaryReader p_reader)
		{
			m_visibleRegions = new int[0];
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TREE:
					{
						m_tree = p_reader.ReadArrayBlock<BDB_TreeNode>(
							BDB_TreeNode.Read
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
					case ID_VISIBLE_REGIONS:
					{
						m_visibleRegions = p_reader.ReadIntArrayBlock();
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

	public abstract class BDB_TreeNode
	{
		public const byte
			BLOCK_TREE_PARENT = 0x28,
			BLOCK_TREE_LEAF = 0x29;

		public virtual byte Type { get { return 0; } }

		public static BDB_TreeNode Read(LRBinaryReader p_reader)
		{
			byte type = p_reader.ReadByte();
			switch (type)
			{
				case BLOCK_TREE_PARENT:
				{
					return BDB_TreeParent.Read(p_reader);
				}
				case BLOCK_TREE_LEAF:
				{
					return BDB_TreeLeaf.Read(p_reader);
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

	public class BDB_TreeParent : BDB_TreeNode
	{
		public override byte Type
		{
			get { return BLOCK_TREE_PARENT; }
		}

		public int Parent;
		public int Child1;
		public int Child2;
		public float SelectorX;
		public float SelectorY;
		public float SelectorZ;
		public float SelectorValue;

		public static new BDB_TreeParent Read(LRBinaryReader p_reader)
		{
			BDB_TreeParent val = new BDB_TreeParent();
			val.Parent = p_reader.ReadIntegralWithHeader();
			val.Child1 = p_reader.ReadIntegralWithHeader();
			val.Child2 = p_reader.ReadIntegralWithHeader();
			val.SelectorX = p_reader.ReadFloatWithHeader();
			val.SelectorY = p_reader.ReadFloatWithHeader();
			val.SelectorZ = p_reader.ReadFloatWithHeader();
			val.SelectorValue = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class BDB_TreeLeaf : BDB_TreeNode
	{
		public override byte Type
		{
			get { return BLOCK_TREE_LEAF; }
		}

		public ushort Parent;
		public ushort GraphOffset;
		public ushort GraphLength;
		public Fract16Bit CarInRegion;
		public Fract16Bit VisibleRegionOffset;
		public Fract16Bit VisibleRegionLength;

		public static new BDB_TreeLeaf Read(LRBinaryReader p_reader)
		{
			BDB_TreeLeaf val = new BDB_TreeLeaf();
			val.Parent = p_reader.ReadUShortWithHeader();
			val.GraphOffset = p_reader.ReadUShortWithHeader();
			val.GraphLength = p_reader.ReadUShortWithHeader();
			val.CarInRegion = p_reader.ReadFract16BitWithHeader();
			val.VisibleRegionOffset = p_reader.ReadFract16BitWithHeader();
			val.VisibleRegionLength = p_reader.ReadFract16BitWithHeader();
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
