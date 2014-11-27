using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.IO;

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

		private string[]            m_materials;
		private GDB_Vertex_Normal[] m_vertexNormals;
		private GDB_Vertex_Color[]  m_vertexColors;
		private GDB_Polygon[]       m_polygons;
		private float               m_scale;
		private GDB_Meta[]          m_meta;

		public string[]            Materials     { get { return m_materials;     } set { m_materials     = value; } }
		public GDB_Vertex_Normal[] VertexNormals { get { return m_vertexNormals; } set { m_vertexNormals = value; } }
		public GDB_Vertex_Color[]  VertexColors  { get { return m_vertexColors;  } set { m_vertexColors  = value; } }
		public GDB_Polygon[]       Polygons      { get { return m_polygons;      } set { m_polygons      = value; } }
		public float               Scale         { get { return m_scale;         } set { m_scale         = value; } }
		public GDB_Meta[]          Meta          { get { return m_meta;          } set { m_meta          = value; } }

		public GDB()
		{
			m_materials     = new string[0];
			m_vertexNormals = new GDB_Vertex_Normal[0];
			m_vertexColors  = new GDB_Vertex_Color[0];
			m_polygons      = new GDB_Polygon[0];
			m_scale         = 1;
			m_meta          = new GDB_Meta[0];
		}

		public GDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public GDB(LRBinaryReader p_reader)
		{
			m_materials     = new string[0];
			m_vertexNormals = new GDB_Vertex_Normal[0];
			m_vertexColors  = new GDB_Vertex_Color[0];
			m_polygons      = new GDB_Polygon[0];
			m_scale         = 1;
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
					case ID_SCALE:
					{
						m_scale = p_reader.ReadFloatWithHeader();
						break;
					}
					case ID_VERTEX_NORMALED:
					{
						m_vertexNormals = p_reader.ReadArrayBlock<GDB_Vertex_Normal>(
							GDB_Vertex_Normal.Read
						);
						break;
					}
					case ID_VERTEX_COLORED:
					{
						m_vertexColors = p_reader.ReadArrayBlock<GDB_Vertex_Color>(
							GDB_Vertex_Color.Read
						);
						break;
					}
					case ID_INDICES:
					{
						m_polygons = p_reader.ReadArrayBlock<GDB_Polygon>(
							GDB_Polygon.Read
						);
						break;
					}
					case ID_INDICES_META:
					{
						m_meta = p_reader.ReadArrayBlock<GDB_Meta>(
							GDB_Meta.Read
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

		public void Save(string p_filepath)
		{
			using (LRBinaryWriter writer = new LRBinaryWriter(File.OpenWrite(p_filepath)))
			{
				Save(writer);
			}
		}

		public void Save(LRBinaryWriter p_writer)
		{
			p_writer.WriteByte(ID_MATERIALS);
			p_writer.WriteStringArrayBlock(m_materials);
			if (m_scale != 1f)
			{
				p_writer.WriteByte(ID_SCALE);
				p_writer.WriteFloatWithHeader(m_scale);
			}
			if (m_vertexNormals.Length > 0)
			{
				p_writer.WriteByte(ID_VERTEX_NORMALED);
				p_writer.WriteArrayBlock<GDB_Vertex_Normal>(
					GDB_Vertex_Normal.Write,
					m_vertexNormals
				);
			}
			if (m_vertexColors.Length > 0)
			{
				p_writer.WriteByte(ID_VERTEX_COLORED);
				p_writer.WriteArrayBlock<GDB_Vertex_Color>(
					GDB_Vertex_Color.Write,
					m_vertexColors
				);
			}
			p_writer.WriteByte(ID_INDICES);
			p_writer.WriteArrayBlock<GDB_Polygon>(
				GDB_Polygon.Write,
				m_polygons
			);
			if (m_meta != null)
			{
				p_writer.WriteByte(ID_INDICES_META);
				p_writer.WriteArrayBlock<GDB_Meta>(
					GDB_Meta.Write,
					m_meta
				);
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

		public static GDB_Vertex_Normal Read(LRBinaryReader p_reader)
		{
			GDB_Vertex_Normal val = new GDB_Vertex_Normal();
			val.Position  = LRVector3.Read(p_reader);
			val.TexCoords = LRVector2.Read(p_reader);
			val.Normal    = LRVector3.Read(p_reader);
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Vertex_Normal p_value)
		{
			LRVector3.Write(p_writer, p_value.Position);
			LRVector2.Write(p_writer, p_value.TexCoords);
			LRVector3.Write(p_writer, p_value.Normal);
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

		public static GDB_Vertex_Color Read(LRBinaryReader p_reader)
		{
			GDB_Vertex_Color val = new GDB_Vertex_Color();
			val.Position  = LRVector3.Read(p_reader);
			val.TexCoords = LRVector2.Read(p_reader);
			val.Color     = LRColor.Read(p_reader);
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Vertex_Color p_value)
		{
			LRVector3.Write(p_writer, p_value.Position);
			LRVector2.Write(p_writer, p_value.TexCoords);
			LRColor.Write(p_writer, p_value.Color);
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

		public static GDB_Polygon Read(LRBinaryReader p_reader)
		{
			GDB_Polygon val = new GDB_Polygon();
			val.V0 = p_reader.ReadByteWithHeader();
			val.V1 = p_reader.ReadByteWithHeader();
			val.V2 = p_reader.ReadByteWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Polygon p_value)
		{
			p_writer.WriteByteWithHeader(p_value.V0);
			p_writer.WriteByteWithHeader(p_value.V1);
			p_writer.WriteByteWithHeader(p_value.V2);
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

		public static GDB_Meta Read(LRBinaryReader p_reader)
		{
			byte type = p_reader.ReadByte();
			switch (type)
			{
				case PROPERTY_MATERIAL_ID:
				{
					return GDB_Meta_Material.Read(p_reader);
				}
				case PROPERTY_INDICES_META:
				{
					return GDB_Meta_Indices.Read(p_reader);
				}
				case PROPERTY_VERTEX_META:
				{
					return GDB_Meta_Vertices.Read(p_reader);
				}
				case PROPERTY_BONE_ID:
				{
					return GDB_Meta_Bone.Read(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, GDB_Meta p_value)
		{
			switch (p_value.Type)
			{
				case PROPERTY_MATERIAL_ID:
				{
					GDB_Meta_Material.Write(p_writer, (GDB_Meta_Material)p_value);
					break;
				}
				case PROPERTY_INDICES_META:
				{
					GDB_Meta_Indices.Write(p_writer, (GDB_Meta_Indices)p_value);
					break;
				}
				case PROPERTY_VERTEX_META:
				{
					GDB_Meta_Vertices.Write(p_writer, (GDB_Meta_Vertices)p_value);
					break;
				}
				case PROPERTY_BONE_ID:
				{
					GDB_Meta_Bone.Write(p_writer, (GDB_Meta_Bone)p_value);
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

		public static new GDB_Meta_Material Read(LRBinaryReader p_reader)
		{
			GDB_Meta_Material val = new GDB_Meta_Material();
			val.MaterialId = p_reader.ReadUShortWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Meta_Material p_value)
		{
			p_writer.WriteByte(p_value.Type);
			p_writer.WriteUShortWithHeader(p_value.MaterialId);
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

		public static new GDB_Meta_Indices Read(LRBinaryReader p_reader)
		{
			GDB_Meta_Indices val = new GDB_Meta_Indices();
			val.Offset = p_reader.ReadUShortWithHeader();
			val.Length = p_reader.ReadUShortWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Meta_Indices p_value)
		{
			p_writer.WriteByte(p_value.Type);
			p_writer.WriteUShortWithHeader(p_value.Offset);
			p_writer.WriteUShortWithHeader(p_value.Length);
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

		public static new GDB_Meta_Vertices Read(LRBinaryReader p_reader)
		{
			GDB_Meta_Vertices val = new GDB_Meta_Vertices();
			val.UnknownByte = p_reader.ReadByteWithHeader();
			val.Offset      = p_reader.ReadUShortWithHeader();
			val.Length      = p_reader.ReadUShortWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Meta_Vertices p_value)
		{
			p_writer.WriteByte(p_value.Type);
			p_writer.WriteByteWithHeader(p_value.UnknownByte);
			p_writer.WriteUShortWithHeader(p_value.Offset);
			p_writer.WriteUShortWithHeader(p_value.Length);
		}
	}

	public class GDB_Meta_Bone : GDB_Meta
	{
		public override byte Type
		{
			get { return PROPERTY_BONE_ID; }
		}

		public ushort BoneId;

		public static new GDB_Meta_Bone Read(LRBinaryReader p_reader)
		{
			GDB_Meta_Bone val = new GDB_Meta_Bone();
			val.BoneId = p_reader.ReadUShortWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, GDB_Meta_Bone p_value)
		{
			p_writer.WriteByte(p_value.Type);
			p_writer.WriteUShortWithHeader(p_value.BoneId);
		}
	}
}