using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
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
		
		private string[]           m_Materials;
		private LRVector3[]        m_Vertices;
		private BVB_Polygon[]      m_Polygons;
		private BVB_PolygonRange[] m_PolygonRanges;
		
		public string[] Materials
		{
			get { return m_Materials; }
			set { m_Materials = value; }
		}
		public LRVector3[] Vertices
		{
			get { return m_Vertices; }
			set { m_Vertices = value; }
		}
		public BVB_Polygon[] Polygons
		{
			get { return m_Polygons; }
			set { m_Polygons = value; }
		}
		public BVB_PolygonRange[] PolygonRanges
		{
			get { return m_PolygonRanges; }
			set { m_PolygonRanges = value; }
		}
		
		public BVB(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_MATERIALS:
					{
						m_Materials = BinaryFileHelper.ReadStringArrayBlock(stream);
						break;
					}
					case ID_POLYGONS:
					{
						m_Polygons = BinaryFileHelper.ReadArrayBlock<BVB_Polygon>(
							stream,
							new BinaryFileHelper.ReadObject<BVB_Polygon>(
								BVB_Polygon.FromStream
							)
						);
						break;
					}
					case ID_VERTICES:
					{
						m_Vertices = BinaryFileHelper.ReadVector3fArrayBlock(stream);
						break;
					}
					case ID_POLYGON_RANGES:
					{
						m_PolygonRanges = BinaryFileHelper.ReadArrayBlock<BVB_PolygonRange>(
							stream,
							new BinaryFileHelper.ReadObject<BVB_PolygonRange>(
								BVB_PolygonRange.FromStream
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
		
		public BVB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
		}
	}
	
	public class BVB_Polygon
	{
		public int
			V0,
			V1,
			V2,
			Material;
		
		public static BVB_Polygon FromStream(Stream stream)
		{
			BVB_Polygon val = new BVB_Polygon();
			val.V0       = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.V1       = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.V2       = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.Material = BinaryFileHelper.ReadIntegralWithHeader(stream);
			return val;
		}
		
		public static void ToStream(Stream stream, BVB_Polygon value)
		{
			BinaryFileHelper.WriteIntegralWithHeader(stream, value.V0);
			BinaryFileHelper.WriteIntegralWithHeader(stream, value.V1);
			BinaryFileHelper.WriteIntegralWithHeader(stream, value.V2);
			BinaryFileHelper.WriteIntegralWithHeader(stream, value.Material);
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
		
		public static BVB_PolygonRange FromStream(Stream stream)
		{
			BVB_PolygonRange val = new BVB_PolygonRange();
			val.i0        = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.i1        = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.i2        = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.i3        = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.i4        = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.FirstPoly = BinaryFileHelper.ReadIntegralWithHeader(stream);
			val.NumPolys  = BinaryFileHelper.ReadIntegralWithHeader(stream);
			return val;
		}
	}
}