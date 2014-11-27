using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1
{
	public class GHB
	{
		private const byte
			ID_GHOST_PATH = 0x2C;
		
		private GHB_GhostPath m_GhostPath;
		
		public GHB_GhostPath GhostPath { get { return m_GhostPath; } set { m_GhostPath = value; } }
		
		public GHB(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_GHOST_PATH:
					{
						m_GhostPath = BinaryFileHelper.ReadStruct<GHB_GhostPath>(
							stream,
							new BinaryFileHelper.ReadObject<GHB_GhostPath>(
								GHB_GhostPath.FromStream
							)
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(block_id, stream.Position - 1);
					}
				}
			}
		}
		
		public GHB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
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
		
		public static GHB_GhostPath FromStream(Stream stream)
		{
			GHB_GhostPath val = new GHB_GhostPath();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_NODES:
					{
						val.Nodes = BinaryFileHelper.ReadArrayBlock<GHB_Node>(
							stream,
							new BinaryFileHelper.ReadObject<GHB_Node>(
								GHB_Node.FromStream
							)
						);
						break;
					}
					case PROPERTY_POSITION:
					{
						val.InitialPosition = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_ROTATION:
					{
						val.InitialRotation = LRQuaternion.FromStream(stream);
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = new int[3];
						for (int i = 0; i < val.Unknown2A.Length; i++)
						{
							val.Unknown2A[i] = BinaryFileHelper.ReadIntWithHeader(stream);
						}
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(property_id, stream.Position - 1);
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
		
		public static GHB_Node FromStream(Stream stream)
		{
			GHB_Node val = new GHB_Node();
			val.px = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			val.py = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			val.pz = BinaryFileHelper.ReadFract16BitWithHeader(stream);
			val.rx = BinaryFileHelper.ReadFract8BitWithHeader(stream);
			val.ry = BinaryFileHelper.ReadFract8BitWithHeader(stream);
			val.rz = BinaryFileHelper.ReadFract8BitWithHeader(stream);
			val.rw = BinaryFileHelper.ReadFract8BitWithHeader(stream);
			return val;
		}
	}
}