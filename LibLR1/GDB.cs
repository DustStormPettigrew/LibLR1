using System;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1
{
	/// <summary>
	/// 3D Model format. VERY WORK IN PROGRESS.
	/// </summary>
	public class GDB
	{
		private const byte
			ID_MATERIALS          = 0x27,
			ID_VERTEX_NORMALED    = 0x29,
			ID_VERTEX_COLORED     = 0x2A,
			ID_INDICES            = 0x2D,
			ID_INDICES_META       = 0x2E,
			ID_SCALE              = 0x33,
			PROPERTY_MATERIAL_ID  = 0x27,
			PROPERTY_INDICES_META = 0x2D,
			PROPERTY_VERTEX_META  = 0x31,
			PROPERTY_BONE_ID      = 0x32;

		private string[]            m_Materials;
		private GDB_Vertex_Normal[] m_VertexNormals;
		private GDB_Vertex_Color[]  m_VertexColors;
		private GDB_Polygon[]       m_Polygons;
		private float               m_Scale;
		private GDB_Meta[]          m_Meta;

		public string[]            Materials     { get { return m_Materials;     } set { m_Materials     = value; } }
		public GDB_Vertex_Normal[] VertexNormals { get { return m_VertexNormals; } set { m_VertexNormals = value; } }
		public GDB_Vertex_Color[]  VertexColors  { get { return m_VertexColors;  } set { m_VertexColors  = value; } }
		public GDB_Polygon[]       Polygons      { get { return m_Polygons;      } set { m_Polygons      = value; } }
		public float               Scale         { get { return m_Scale;         } set { m_Scale         = value; } }
		public GDB_Meta[]          Meta          { get { return m_Meta;          } set { m_Meta          = value; } }

		public GDB()
		{
			m_Materials     = new string[0];
			m_VertexNormals = new GDB_Vertex_Normal[0];
			m_VertexColors  = new GDB_Vertex_Color[0];
			m_Polygons      = new GDB_Polygon[0];
			m_Scale         = 1;
			m_Meta          = new GDB_Meta[0];
		}

		public GDB(Stream stream)
		{
			m_Materials     = new string[0];
			m_VertexNormals = new GDB_Vertex_Normal[0];
			m_VertexColors  = new GDB_Vertex_Color[0];
			m_Polygons      = new GDB_Polygon[0];
			m_Scale         = 1;
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
					case ID_SCALE:
					{
						m_Scale = BinaryFileHelper.ReadFloatWithHeader(stream);
						break;
					}
					case ID_VERTEX_NORMALED:
					{
						m_VertexNormals = BinaryFileHelper.ReadArrayBlock<GDB_Vertex_Normal>(
							stream,
							new BinaryFileHelper.ReadObject<GDB_Vertex_Normal>(
								GDB_Vertex_Normal.FromStream
							)
						);
						break;
					}
					case ID_VERTEX_COLORED:
					{
						m_VertexColors = BinaryFileHelper.ReadArrayBlock<GDB_Vertex_Color>(
							stream,
							new BinaryFileHelper.ReadObject<GDB_Vertex_Color>(
								GDB_Vertex_Color.FromStream
							)
						);
						break;
					}
					case ID_INDICES:
					{
						m_Polygons = BinaryFileHelper.ReadArrayBlock<GDB_Polygon>(
							stream,
							new BinaryFileHelper.ReadObject<GDB_Polygon>(
								GDB_Polygon.FromStream
							)
						);
						break;
					}
					case ID_INDICES_META:
					{
						m_Meta = BinaryFileHelper.ReadArrayBlock<GDB_Meta>(
							stream,
							new BinaryFileHelper.ReadObject<GDB_Meta>(
								GDB_Meta.FromStream
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

		public GDB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

		public void Save(Stream stream)
		{
			stream.WriteByte(ID_MATERIALS);
			BinaryFileHelper.WriteStringArrayBlock(stream, m_Materials);
			if (m_Scale != 1f)
			{
				stream.WriteByte(ID_SCALE);
				BinaryFileHelper.WriteFloatWithHeader(stream, m_Scale);
			}
			if (m_VertexNormals.Length > 0)
			{
				stream.WriteByte(ID_VERTEX_NORMALED);
				BinaryFileHelper.WriteArrayBlock<GDB_Vertex_Normal>(
					stream,
					new BinaryFileHelper.WriteObject<GDB_Vertex_Normal>(
						GDB_Vertex_Normal.ToStream
					),
					m_VertexNormals
				);
			}
			if (m_VertexColors.Length > 0)
			{
				stream.WriteByte(ID_VERTEX_COLORED);
				BinaryFileHelper.WriteArrayBlock<GDB_Vertex_Color>(
					stream,
					new BinaryFileHelper.WriteObject<GDB_Vertex_Color>(
						GDB_Vertex_Color.ToStream
					),
					m_VertexColors
				);
			}
			stream.WriteByte(ID_INDICES);
			BinaryFileHelper.WriteArrayBlock<GDB_Polygon>(
				stream,
				new BinaryFileHelper.WriteObject<GDB_Polygon>(
					GDB_Polygon.ToStream
				),
				m_Polygons
			);
			if (m_Meta != null)
			{
				stream.WriteByte(ID_INDICES_META);
				BinaryFileHelper.WriteArrayBlock<GDB_Meta>(
					stream,
					new BinaryFileHelper.WriteObject<GDB_Meta>(
						GDB_Meta.ToStream
					),
					m_Meta
				);
			}
		}

		public void Save(string path)
		{
			using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				Save(fsOut);
			}
		}
	}

	public class GDB_Vertex_Normal
	{
		public LRVector3 Position;
		public LRVector2 TexCoords;
		public LRVector3 Normal;

		public GDB_Vertex_Normal()
			: this(new LRVector3(), new LRVector2(), new LRVector3()) { }

		public GDB_Vertex_Normal(LRVector3 position, LRVector2 texcoords, LRVector3 normal)
		{
			Position = position;
			TexCoords = texcoords;
			Normal = normal;
		}

		public static GDB_Vertex_Normal FromStream(Stream stream)
		{
			GDB_Vertex_Normal val = new GDB_Vertex_Normal();
			val.Position  = LRVector3.FromStream(stream);
			val.TexCoords = LRVector2.FromStream(stream);
			val.Normal    = LRVector3.FromStream(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Vertex_Normal value)
		{
			LRVector3.ToStream(stream, value.Position);
			LRVector2.ToStream(stream, value.TexCoords);
			LRVector3.ToStream(stream, value.Normal);
		}
	}

	public class GDB_Vertex_Color
	{
		public LRVector3 Position;
		public LRVector2 TexCoords;
		public LRColor Color;

		public GDB_Vertex_Color()
			: this(new LRVector3(), new LRVector2(), new LRColor()) { }

		public GDB_Vertex_Color(LRVector3 position, LRVector2 texcoords, LRColor color)
		{
			Position  = position;
			TexCoords = texcoords;
			Color     = color;
		}

		public static GDB_Vertex_Color FromStream(Stream stream)
		{
			GDB_Vertex_Color val = new GDB_Vertex_Color();
			val.Position  = LRVector3.FromStream(stream);
			val.TexCoords = LRVector2.FromStream(stream);
			val.Color     = LRColor.FromStream(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Vertex_Color value)
		{
			LRVector3.ToStream(stream, value.Position);
			LRVector2.ToStream(stream, value.TexCoords);
			LRColor.ToStream(stream, value.Color);
		}
	}

	public class GDB_Polygon
	{
		public byte V0, V1, V2;

		public GDB_Polygon()
			: this(0, 0, 0) { }

		public GDB_Polygon(byte v0, byte v1, byte v2)
		{
			V0 = v0;
			V1 = v1;
			V2 = v2;
		}

		public static GDB_Polygon FromStream(Stream stream)
		{
			GDB_Polygon val = new GDB_Polygon();
			val.V0 = BinaryFileHelper.ReadByteWithHeader(stream);
			val.V1 = BinaryFileHelper.ReadByteWithHeader(stream);
			val.V2 = BinaryFileHelper.ReadByteWithHeader(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Polygon value)
		{
			BinaryFileHelper.WriteByteWithHeader(stream, value.V0);
			BinaryFileHelper.WriteByteWithHeader(stream, value.V1);
			BinaryFileHelper.WriteByteWithHeader(stream, value.V2);
		}
	}

	public abstract class GDB_Meta
	{
		public const byte
			PROPERTY_MATERIAL_ID  = 0x27,
			PROPERTY_INDICES_META = 0x2D,
			PROPERTY_VERTEX_META  = 0x31,
			PROPERTY_BONE_ID      = 0x32;

		public virtual byte Type { get { return 0; } }

		public static GDB_Meta FromStream(Stream stream)
		{
			byte type = (byte)stream.ReadByte();
			switch (type)
			{
				case PROPERTY_MATERIAL_ID:
				{
					return GDB_Meta_Material.FromStream(stream);
				}
				case PROPERTY_INDICES_META:
				{
					return GDB_Meta_Indices.FromStream(stream);
				}
				case PROPERTY_VERTEX_META:
				{
					return GDB_Meta_Vertices.FromStream(stream);
				}
				case PROPERTY_BONE_ID:
				{
					return GDB_Meta_Bone.FromStream(stream);
				}
				default:
				{
					throw new UnexpectedBlockException(type, stream.Position - 1);
				}
			}
		}

		public static void ToStream(Stream stream, GDB_Meta value)
		{
			switch (value.Type)
			{
				case PROPERTY_MATERIAL_ID:
				{
					GDB_Meta_Material.ToStream(stream, (GDB_Meta_Material)value);
					break;
				}
				case PROPERTY_INDICES_META:
				{
					GDB_Meta_Indices.ToStream(stream, (GDB_Meta_Indices)value);
					break;
				}
				case PROPERTY_VERTEX_META:
				{
					GDB_Meta_Vertices.ToStream(stream, (GDB_Meta_Vertices)value);
					break;
				}
				case PROPERTY_BONE_ID:
				{
					GDB_Meta_Bone.ToStream(stream, (GDB_Meta_Bone)value);
					break;
				}
				default:
				{
					throw new ArgumentException("Argument `value` doesn't have a valid `Type` attribute.");
				}
			}
		}
	}

	public class GDB_Meta_Material : GDB_Meta
	{
		public override byte Type
		{
			get { return PROPERTY_MATERIAL_ID; }
		}

		public ushort MaterialId;

		public static new GDB_Meta_Material FromStream(Stream stream)
		{
			GDB_Meta_Material val = new GDB_Meta_Material();
			val.MaterialId = BinaryFileHelper.ReadUShortWithHeader(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Meta_Material value)
		{
			stream.WriteByte(value.Type);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.MaterialId);
		}
	}

	public class GDB_Meta_Indices : GDB_Meta
	{
		public override byte Type
		{
			get { return PROPERTY_INDICES_META; }
		}

		public ushort Offset;
		public ushort Length;

		public static new GDB_Meta_Indices FromStream(Stream stream)
		{
			GDB_Meta_Indices val = new GDB_Meta_Indices();
			val.Offset = BinaryFileHelper.ReadUShortWithHeader(stream);
			val.Length = BinaryFileHelper.ReadUShortWithHeader(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Meta_Indices value)
		{
			stream.WriteByte(value.Type);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.Offset);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.Length);
		}
	}

	public class GDB_Meta_Vertices : GDB_Meta
	{
		public override byte Type
		{
			get { return PROPERTY_VERTEX_META; }
		}

		public byte   UnknownByte;
		public ushort Offset;
		public ushort Length;

		public static new GDB_Meta_Vertices FromStream(Stream stream)
		{
			GDB_Meta_Vertices val = new GDB_Meta_Vertices();
			val.UnknownByte = BinaryFileHelper.ReadByteWithHeader(stream);
			val.Offset      = BinaryFileHelper.ReadUShortWithHeader(stream);
			val.Length      = BinaryFileHelper.ReadUShortWithHeader(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Meta_Vertices value)
		{
			stream.WriteByte(value.Type);
			BinaryFileHelper.WriteByteWithHeader(stream, value.UnknownByte);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.Offset);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.Length);
		}
	}

	public class GDB_Meta_Bone : GDB_Meta
	{
		public override byte Type
		{
			get { return PROPERTY_BONE_ID; }
		}

		public ushort BoneId;

		public static new GDB_Meta_Bone FromStream(Stream stream)
		{
			GDB_Meta_Bone val = new GDB_Meta_Bone();
			val.BoneId = BinaryFileHelper.ReadUShortWithHeader(stream);
			return val;
		}

		public static void ToStream(Stream stream, GDB_Meta_Bone value)
		{
			stream.WriteByte(value.Type);
			BinaryFileHelper.WriteUShortWithHeader(stream, value.BoneId);
		}
	}
}