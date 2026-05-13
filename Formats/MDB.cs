using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Material database format.
	/// </summary>
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
			PROPERTY_2F = 0x2F,
			PROPERTY_38 = 0x38,
			PROPERTY_3A = 0x3A,
			PROPERTY_3F = 0x3F,
			PROPERTY_41 = 0x41,
			PROPERTY_44 = 0x44,
			PROPERTY_45 = 0x45,
			PROPERTY_ALPHA = 0x46,
			PROPERTY_47 = 0x47,
			PROPERTY_48 = 0x48,
			PROPERTY_49 = 0x49,
			PROPERTY_4A = 0x4A,
			PROPERTY_4B = 0x4B,
			PROPERTY_4C = 0x4C,
			PROPERTY_4D = 0x4D,
			PROPERTY_4E = 0x4E,
			PROPERTY_4F = 0x4F,
			PROPERTY_50 = 0x50;

		public LRColor AmbientColor;
		public LRColor DiffuseColor;
		public bool Bool2A;
		public bool Bool2B;
		public string TextureName;
		public bool Bool2D;
		public bool Bool2E;
		public MDB_Property2F Property2F;
		public MDB_Property38 Property38;
		public bool Bool3A;
		public bool Bool3F;
		public bool Bool41;
		public bool Bool44;
		public bool Bool45;
		public int? Alpha;
		public bool Bool47;
		public bool Bool48;
		public bool Bool49;
		public bool Bool4A;
		public bool Bool4B;
		public bool Bool4C;
		public int? Int4D;
		public int? Int4E;
		public int? Int4F;
		public int? Int50;

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
					case PROPERTY_2F:
					{
						val.Property2F = MDB_Property2F.Read(p_reader);
						break;
					}
					case PROPERTY_38:
					{
						val.Property38 = MDB_Property38.Read(p_reader);
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
					case PROPERTY_44:
					{
						val.Bool44 = true;
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
					case PROPERTY_47:
					{
						val.Bool47 = true;
						break;
					}
					case PROPERTY_48:
					{
						val.Bool48 = true;
						break;
					}
					case PROPERTY_49:
					{
						val.Bool49 = true;
						break;
					}
					case PROPERTY_4A:
					{
						val.Bool4A = true;
						break;
					}
					case PROPERTY_4B:
					{
						val.Bool4B = true;
						break;
					}
					case PROPERTY_4C:
					{
						val.Bool4C = true;
						break;
					}
					case PROPERTY_4D:
					{
						val.Int4D = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_4E:
					{
						val.Int4E = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_4F:
					{
						val.Int4F = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_50:
					{
						val.Int50 = p_reader.ReadIntWithHeader();
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
			if (p_value.Property2F != null)
			{
				p_writer.WriteByte(PROPERTY_2F);
				MDB_Property2F.Write(p_writer, p_value.Property2F);
			}
			if (p_value.Property38 != null)
			{
				p_writer.WriteByte(PROPERTY_38);
				MDB_Property38.Write(p_writer, p_value.Property38);
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
			if (p_value.Bool44)
			{
				p_writer.WriteByte(PROPERTY_44);
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
			if (p_value.Bool47)
			{
				p_writer.WriteByte(PROPERTY_47);
			}
			if (p_value.Bool48)
			{
				p_writer.WriteByte(PROPERTY_48);
			}
			if (p_value.Bool49)
			{
				p_writer.WriteByte(PROPERTY_49);
			}
			if (p_value.Bool4A)
			{
				p_writer.WriteByte(PROPERTY_4A);
			}
			if (p_value.Bool4B)
			{
				p_writer.WriteByte(PROPERTY_4B);
			}
			if (p_value.Bool4C)
			{
				p_writer.WriteByte(PROPERTY_4C);
			}
			if (p_value.Int4D.HasValue)
			{
				p_writer.WriteByte(PROPERTY_4D);
				p_writer.WriteIntWithHeader(p_value.Int4D.Value);
			}
			if (p_value.Int4E.HasValue)
			{
				p_writer.WriteByte(PROPERTY_4E);
				p_writer.WriteIntWithHeader(p_value.Int4E.Value);
			}
			if (p_value.Int4F.HasValue)
			{
				p_writer.WriteByte(PROPERTY_4F);
				p_writer.WriteIntWithHeader(p_value.Int4F.Value);
			}
			if (p_value.Int50.HasValue)
			{
				p_writer.WriteByte(PROPERTY_50);
				p_writer.WriteIntWithHeader(p_value.Int50.Value);
			}
		}
	}

	/// <summary>
	/// Complex property 0x2F: reads a subtoken byte (0x30-0x37).
	/// Subtokens 0x30 and 0x36 are bare flags; the rest carry an integer value.
	/// </summary>
	public class MDB_Property2F
	{
		public const byte
			SUB_30 = 0x30,
			SUB_31 = 0x31,
			SUB_32 = 0x32,
			SUB_33 = 0x33,
			SUB_34 = 0x34,
			SUB_35 = 0x35,
			SUB_36 = 0x36,
			SUB_37 = 0x37;

		public byte SubToken;
		public int? Value;

		public static MDB_Property2F Read(LRBinaryReader p_reader)
		{
			MDB_Property2F val = new MDB_Property2F();
			val.SubToken = p_reader.ReadByte();
			switch (val.SubToken)
			{
				case SUB_30:
				case SUB_36:
					// Bare flags, no value
					break;
				case SUB_31:
				case SUB_32:
				case SUB_33:
				case SUB_34:
				case SUB_35:
				case SUB_37:
					val.Value = p_reader.ReadIntWithHeader();
					break;
				default:
					throw new UnexpectedPropertyException(
						val.SubToken,
						p_reader.BaseStream.Position - 1
					);
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, MDB_Property2F p_value)
		{
			p_writer.WriteByte(p_value.SubToken);
			switch (p_value.SubToken)
			{
				case SUB_30:
				case SUB_36:
					break;
				default:
					if (p_value.Value.HasValue)
						p_writer.WriteIntWithHeader(p_value.Value.Value);
					break;
			}
		}
	}

	/// <summary>
	/// Complex property 0x38: reads TWO subtoken bytes (each 0x39-0x43), both bare markers.
	/// </summary>
	public class MDB_Property38
	{
		public const byte
			SUB_39 = 0x39,
			SUB_3A = 0x3A,
			SUB_3B = 0x3B,
			SUB_3C = 0x3C,
			SUB_3D = 0x3D,
			SUB_3E = 0x3E,
			SUB_3F = 0x3F,
			SUB_40 = 0x40,
			SUB_41 = 0x41,
			SUB_42 = 0x42,
			SUB_43 = 0x43;

		public byte SubToken1;
		public byte SubToken2;

		public static MDB_Property38 Read(LRBinaryReader p_reader)
		{
			MDB_Property38 val = new MDB_Property38();
			val.SubToken1 = p_reader.ReadByte();
			ValidateSubToken(val.SubToken1, p_reader);
			val.SubToken2 = p_reader.ReadByte();
			ValidateSubToken(val.SubToken2, p_reader);
			return val;
		}

		private static void ValidateSubToken(byte p_token, LRBinaryReader p_reader)
		{
			if (p_token < SUB_39 || p_token > SUB_43)
			{
				throw new UnexpectedPropertyException(
					p_token,
					p_reader.BaseStream.Position - 1
				);
			}
		}

		public static void Write(LRBinaryWriter p_writer, MDB_Property38 p_value)
		{
			p_writer.WriteByte(p_value.SubToken1);
			p_writer.WriteByte(p_value.SubToken2);
		}
	}
}