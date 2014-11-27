using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
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
		
		private BDB_Unknown27[]   m_Unknown27;
		private BDB_BoundingBox[] m_BoundingBoxes;
		private int[]             m_Unknown2B;
		
		public BDB_Unknown27[] Unknown27
		{
			get { return m_Unknown27; }
			set { m_Unknown27 = value; }
		}
		public BDB_BoundingBox[] BoundingBoxes
		{
			get { return m_BoundingBoxes; }
			set { m_BoundingBoxes = value; }
		}
		public int[] Unknown2B
		{
			get { return m_Unknown2B; }
			set { m_Unknown2B = value; }
		}
		
		public BDB(Stream stream)
		{
			m_Unknown2B = new int[0];
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_UNKNOWN_27:
					{
						m_Unknown27 = BinaryFileHelper.ReadArrayBlock<BDB_Unknown27>(
							stream,
							new BinaryFileHelper.ReadObject<BDB_Unknown27>(
								BDB_Unknown27.FromStream
							)
						);
						break;
					}
					case ID_BOUNDING_BOXES:
					{
						m_BoundingBoxes = BinaryFileHelper.ReadArrayBlock<BDB_BoundingBox>(
							stream,
							new BinaryFileHelper.ReadObject<BDB_BoundingBox>(
								BDB_BoundingBox.FromStream
							)
						);
						break;
					}
					case ID_UNKNOWN_2B:
					{
						m_Unknown2B = BinaryFileHelper.ReadIntArrayBlock(stream);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(block_id, stream.Position - 1);
					}
				}
			}
		}
		
		public BDB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
		}
	}
	
	public abstract class BDB_Unknown27
	{
		public const byte
			ID_UNKNOWN_27 = 0x27,
			ID_UNKNOWN_28 = 0x28,
			ID_UNKNOWN_29 = 0x29;
		
		public virtual byte Type { get { return 0; } }
		
		public static BDB_Unknown27 FromStream(Stream stream)
		{
			byte type = (byte)stream.ReadByte();
			switch (type)
			{
				case ID_UNKNOWN_28:
				{
					return BDB_Unknown28.FromStream(stream);
				}
				case ID_UNKNOWN_29:
				{
					return BDB_Unknown29.FromStream(stream);
				}
				default:
				{
					throw new UnexpectedBlockException(type, stream.Position - 1);
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
		
		public static new BDB_Unknown28 FromStream(Stream stream)
		{
			BDB_Unknown28 val = new BDB_Unknown28();
			val.a = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.b = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.c = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.d = BinaryFileHelper.ReadFloatWithHeader(stream);
			val.e = BinaryFileHelper.ReadFloatWithHeader(stream);
			val.d = BinaryFileHelper.ReadFloatWithHeader(stream);
			val.g = BinaryFileHelper.ReadFloatWithHeader(stream);
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
		
		public static new BDB_Unknown29 FromStream(Stream stream)
		{
			BDB_Unknown29 val = new BDB_Unknown29();
			val.a = BinaryFileHelper.ReadUShortWithHeader(stream);
			val.b = BinaryFileHelper.ReadUShortWithHeader(stream);
			val.c = BinaryFileHelper.ReadUShortWithHeader(stream);
			val.d = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			val.e = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			val.f = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			return val;
		}
	}
	
	public class BDB_BoundingBox
	{
		public LRVector3 MinPoint, MaxPoint;
		
		public static BDB_BoundingBox FromStream(Stream stream)
		{
			BDB_BoundingBox val = new BDB_BoundingBox();
			val.MinPoint = LRVector3.FromStream(stream);
			val.MaxPoint = LRVector3.FromStream(stream);
			return val;
		}
		
		public static void ToStream(Stream stream, BDB_BoundingBox value)
		{
			LRVector3.ToStream(stream, value.MinPoint);
			LRVector3.ToStream(stream, value.MaxPoint);
		}
	}
}