using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	public class MDB
	{
		public const byte
			ID_MATERIALS = 0x27;

		private Dictionary<string, MDB_Material> m_materials;

		public Dictionary<string, MDB_Material> Materials
		{
			get { return m_materials; }
			set { m_materials = value; }
		}

		public MDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public MDB(LRBinaryReader p_reader)
		{
			m_materials = new Dictionary<string, MDB_Material>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_MATERIALS:
					{
						m_materials = p_reader.ReadDictionaryBlock<MDB_Material>(
							MDB_Material.Read,
							ID_MATERIALS
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
			p_writer.WriteDictionaryBlock<MDB_Material>(
				MDB_Material.Write,
				m_materials,
				ID_MATERIALS
			);
		}
	}

	public class MDB_Material
	{
		public const byte
			PROPERTY_AMBIENT_COLOR = 0x28,
			PROPERTY_DIFFUSE_COLOR = 0x29,
			PROPERTY_2A = 0x2A,
			PROPERTY_2B = 0x2B,
			PROPERTY_TEXTURE_NAME = 0x2C,
			PROPERTY_2D = 0x2D,
			PROPERTY_2E = 0x2E,
			PROPERTY_38 = 0x38,
			PROPERTY_3A = 0x3A,
			PROPERTY_3F = 0x3F,
			PROPERTY_41 = 0x41,
			PROPERTY_45 = 0x45,
			PROPERTY_ALPHA = 0x46,
			PROPERTY_4A = 0x4A;

		public LRColor AmbientColor;
		public LRColor DiffuseColor;
		public bool Bool2A;
		public bool Bool2B;
		public string TextureName;
		public bool Bool2D;
		public bool Bool2E;
		public bool Bool38;
		public bool Bool3A;
		public bool Bool3F;
		public bool Bool41;
		public bool Bool45;
		public int? Alpha;
		public bool Bool4A;

		public static MDB_Material Read(LRBinaryReader p_reader)
		{
			MDB_Material val = new MDB_Material();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_AMBIENT_COLOR:
					{
						val.AmbientColor = LRColor.Read(p_reader);
						break;
					}
					case PROPERTY_DIFFUSE_COLOR:
					{
						val.DiffuseColor = LRColor.Read(p_reader);
						break;
					}
					case PROPERTY_2A:
					{
						val.Bool2A = true;
						break;
					}
					case PROPERTY_2B:
					{
						val.Bool2B = true;
						break;
					}
					case PROPERTY_TEXTURE_NAME:
					{
						val.TextureName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_2D:
					{
						val.Bool2D = true;
						break;
					}
					case PROPERTY_2E:
					{
						val.Bool2E = true;
						break;
					}
					case PROPERTY_38:
					{
						val.Bool38 = true;
						break;
					}
					case PROPERTY_3A:
					{
						val.Bool3A = true;
						break;
					}
					case PROPERTY_3F:
					{
						val.Bool3F = true;
						break;
					}
					case PROPERTY_41:
					{
						val.Bool41 = true;
						break;
					}
					case PROPERTY_45:
					{
						val.Bool45 = true;
						break;
					}
					case PROPERTY_ALPHA:
					{
						val.Alpha = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_4A:
					{
						val.Bool4A = true;
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

		public static void Write(LRBinaryWriter p_writer, MDB_Material p_value)
		{
			if (p_value.AmbientColor != null)
			{
				p_writer.WriteByte(PROPERTY_AMBIENT_COLOR);
				LRColor.Write(p_writer, p_value.AmbientColor);
			}
			if (p_value.DiffuseColor != null)
			{
				p_writer.WriteByte(PROPERTY_DIFFUSE_COLOR);
				LRColor.Write(p_writer, p_value.DiffuseColor);
			}
			if (p_value.Bool2A)
			{
				p_writer.WriteByte(PROPERTY_2A);
			}
			if (p_value.Bool2B)
			{
				p_writer.WriteByte(PROPERTY_2B);
			}
			if (p_value.TextureName != null)
			{
				p_writer.WriteByte(PROPERTY_TEXTURE_NAME);
				p_writer.WriteStringWithHeader(p_value.TextureName);
			}
			if (p_value.Bool2D)
			{
				p_writer.WriteByte(PROPERTY_2D);
			}
			if (p_value.Bool2E)
			{
				p_writer.WriteByte(PROPERTY_2E);
			}
			if (p_value.Bool38)
			{
				p_writer.WriteByte(PROPERTY_38);
			}
			if (p_value.Bool3A)
			{
				p_writer.WriteByte(PROPERTY_3A);
			}
			if (p_value.Bool3F)
			{
				p_writer.WriteByte(PROPERTY_3F);
			}
			if (p_value.Bool41)
			{
				p_writer.WriteByte(PROPERTY_41);
			}
			if (p_value.Bool45)
			{
				p_writer.WriteByte(PROPERTY_45);
			}
			if (p_value.Alpha.HasValue)
			{
				p_writer.WriteByte(PROPERTY_ALPHA);
				p_writer.WriteIntWithHeader(p_value.Alpha.Value);
			}
			if (p_value.Bool4A)
			{
				p_writer.WriteByte(PROPERTY_4A);
			}
		}
	}
}