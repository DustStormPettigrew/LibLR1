using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;

namespace LibLR1
{
	public class GCB
	{
		private const byte
			ID_VERTICES = 0x28,
			ID_MODELS = 0x2C;

		private GCB_Vertex[] m_vertices;
		private Dictionary<string, GCB_ModelSet> m_modelSets;

		public GCB_Vertex[] Vertices
		{
			get { return m_vertices; }
			set { m_vertices = value; }
		}
		public Dictionary<string, GCB_ModelSet> Models
		{
			get { return m_modelSets; }
			set { m_modelSets = value; }
		}

		public GCB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public GCB(LRBinaryReader p_reader)
		{
			m_vertices = new GCB_Vertex[0];
			m_modelSets = new Dictionary<string, GCB_ModelSet>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_VERTICES:
					{
						m_vertices = p_reader.ReadArrayBlock<GCB_Vertex>(
							GCB_Vertex.Read
						);
						break;
					}
					case ID_MODELS:
					{
						m_modelSets = p_reader.ReadDictionaryBlock<GCB_ModelSet>(
							GCB_ModelSet.Read,
							ID_MODELS
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

	public class GCB_Vertex
	{
		public Fract16Bit a, b, c, d, e;
		public Fract8Bit f, g, h;

		public static GCB_Vertex Read(LRBinaryReader p_reader)
		{
			GCB_Vertex val = new GCB_Vertex();
			val.a = p_reader.ReadFract16BitWithHeader();
			val.b = p_reader.ReadFract16BitWithHeader();
			val.c = p_reader.ReadFract16BitWithHeader();
			val.d = p_reader.ReadFract16BitWithHeader();
			val.e = p_reader.ReadFract16BitWithHeader();
			val.f = p_reader.ReadFract8BitWithHeader();
			val.g = p_reader.ReadFract8BitWithHeader();
			val.h = p_reader.ReadFract8BitWithHeader();
			return val;
		}
	}

	public class GCB_ModelSet
	{
		private const byte
			PROPERTY_MODELS = 0x2B,
			PROPERTY_UNKNOWN_2D = 0x2D;

		public GCB_Model[] Models;
		public float Unknown2D;

		public static GCB_ModelSet Read(LRBinaryReader p_reader)
		{
			GCB_ModelSet val = new GCB_ModelSet();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_MODELS:
					{
						val.Models = p_reader.ReadStructArrayBlock<GCB_Model>(
							GCB_Model.Read,
							PROPERTY_MODELS
						);
						break;
					}
					case PROPERTY_UNKNOWN_2D:
					{
						val.Unknown2D = p_reader.ReadFloatWithHeader();
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

	public class GCB_Model
	{
		private const byte
			PROPERTY_NAME = 0x27,
			PROPERTY_POLYS = 0x2A;

		public string Name;
		public GCB_Polygon[] Polys;

		public static GCB_Model Read(LRBinaryReader p_reader)
		{
			GCB_Model val = new GCB_Model();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_NAME:
					{
						val.Name = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_POLYS:
					{
						val.Polys = p_reader.ReadArrayBlock<GCB_Polygon>(
							GCB_Polygon.Read
						);
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

	public class GCB_Polygon
	{
		public int V0, V1, V2;

		public static GCB_Polygon Read(LRBinaryReader p_reader)
		{
			GCB_Polygon val = new GCB_Polygon();
			val.V0 = p_reader.ReadIntegralWithHeader();
			val.V1 = p_reader.ReadIntegralWithHeader();
			val.V2 = p_reader.ReadIntegralWithHeader();
			return val;
		}
	}
}