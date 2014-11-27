using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class BVB
	{
		private const byte
			ID_MATERIALS      = 0x27,
			ID_POLYGONS       = 0x2D,
			ID_VERTICES       = 0x34,
			ID_POLYGON_RANGES = 0x8E;
		
		private string[]           m_materials;
		private LRVector3[]        m_vertices;
		private BVB_Polygon[]      m_polygons;
		private BVB_PolygonRange[] m_polygonRanges;
		
		public string[] Materials
		{
			get { return m_materials; }
			set { m_materials = value; }
		}
		public LRVector3[] Vertices
		{
			get { return m_vertices; }
			set { m_vertices = value; }
		}
		public BVB_Polygon[] Polygons
		{
			get { return m_polygons; }
			set { m_polygons = value; }
		}
		public BVB_PolygonRange[] PolygonRanges
		{
			get { return m_polygonRanges; }
			set { m_polygonRanges = value; }
		}

		public BVB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public BVB(LRBinaryReader p_reader)
		{
			m_materials = new string[0];
			m_vertices = new LRVector3[0];
			m_polygons = new BVB_Polygon[0];
			m_polygonRanges = new BVB_PolygonRange[0];
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_MATERIALS:
					{
						m_materials = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_POLYGONS:
					{
						m_polygons = p_reader.ReadArrayBlock<BVB_Polygon>(
							BVB_Polygon.Read
						);
						break;
					}
					case ID_VERTICES:
					{
						m_vertices = p_reader.ReadVector3fArrayBlock();
						break;
					}
					case ID_POLYGON_RANGES:
					{
						m_polygonRanges = p_reader.ReadArrayBlock<BVB_PolygonRange>(
							BVB_PolygonRange.Read
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
	
	public class BVB_Polygon
	{
		public int
			V0,
			V1,
			V2,
			Material;

		public static BVB_Polygon Read(LRBinaryReader p_reader)
		{
			BVB_Polygon val = new BVB_Polygon();
			val.V0       = p_reader.ReadIntegralWithHeader();
			val.V1       = p_reader.ReadIntegralWithHeader();
			val.V2       = p_reader.ReadIntegralWithHeader();
			val.Material = p_reader.ReadIntegralWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, BVB_Polygon p_value)
		{
			p_writer.WriteIntegralWithHeader(p_value.V0);
			p_writer.WriteIntegralWithHeader(p_value.V1);
			p_writer.WriteIntegralWithHeader(p_value.V2);
			p_writer.WriteIntegralWithHeader(p_value.Material);
		}
	}
	
	public class BVB_PolygonRange
	{
		public int
			i0,
			i1,
			i2, // color?
			i3, // color?
			i4, // color?
			FirstPoly, // first poly?
			NumPolys; // num polys?

		public static BVB_PolygonRange Read(LRBinaryReader p_reader)
		{
			BVB_PolygonRange val = new BVB_PolygonRange();
			val.i0        = p_reader.ReadIntegralWithHeader();
			val.i1        = p_reader.ReadIntegralWithHeader();
			val.i2        = p_reader.ReadIntegralWithHeader();
			val.i3        = p_reader.ReadIntegralWithHeader();
			val.i4        = p_reader.ReadIntegralWithHeader();
			val.FirstPoly = p_reader.ReadIntegralWithHeader();
			val.NumPolys  = p_reader.ReadIntegralWithHeader();
			return val;
		}
	}
}