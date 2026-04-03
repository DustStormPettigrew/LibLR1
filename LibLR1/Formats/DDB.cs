using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Drivers Database.
	/// </summary>
	public class DDB
	{
		private const byte
			ID_DRIVERS = 0x27;

		private Dictionary<string, DDB_Driver> m_drivers;

		public Dictionary<string, DDB_Driver> Drivers
		{
			get { return m_drivers; }
			set { m_drivers = value; }
		}

		public DDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public DDB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_DRIVERS:
					{
						m_drivers = p_reader.ReadDictionaryBlock<DDB_Driver>(
							DDB_Driver.Read,
							ID_DRIVERS
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
			p_writer.WriteByte(ID_DRIVERS);
			p_writer.WriteDictionaryBlock<DDB_Driver>(
				DDB_Driver.Write,
				m_drivers,
				ID_DRIVERS
			);
		}
	}

	public class DDB_Driver
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_UNKNOWN_2A = 0x2A,
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_UNKNOWN_2C = 0x2C,
			PROPERTY_UNKNOWN_2D = 0x2D,
			PROPERTY_UNKNOWN_2E = 0x2E,
			PROPERTY_UNKNOWN_2F = 0x2F,
			PROPERTY_UNKNOWN_30 = 0x30,
			PROPERTY_UNKNOWN_31 = 0x31,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_UNKNOWN_34 = 0x34,
			PROPERTY_UNKNOWN_35 = 0x35,
			PROPERTY_UNKNOWN_36 = 0x36,
			PROPERTY_UNKNOWN_37 = 0x37,
			PROPERTY_UNKNOWN_38 = 0x38,
			PROPERTY_UNKNOWN_3A = 0x3A;

		public string Unknown28;
		public string Unknown29;
		public string Unknown2A;
		public string Unknown2B;
		public int Unknown2C;
		public int Unknown2D;
		public int Unknown2E;
		public int Unknown2F;
		public int Unknown30;
		public int Unknown31;
		public int Unknown33;
		public int Unknown34;
		public int Unknown35;
		public int Unknown36;
		public int Unknown37;
		public int Unknown38;
		public bool HasUnknown3A = false;
		public int Unknown3A_0;
		public int Unknown3A_1;

		public static DDB_Driver Read(LRBinaryReader p_reader)
		{
			DDB_Driver val = new DDB_Driver();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.Unknown29 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.Unknown2C = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2D:
					{
						val.Unknown2D = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2E:
					{
						val.Unknown2E = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2F:
					{
						val.Unknown2F = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_30:
					{
						val.Unknown30 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_31:
					{
						val.Unknown31 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown33 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_34:
					{
						val.Unknown34 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_35:
					{
						val.Unknown35 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_36:
					{
						val.Unknown36 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_37:
					{
						val.Unknown37 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_38:
					{
						val.Unknown38 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3A:
					{
						val.HasUnknown3A = true;
						val.Unknown3A_0 = p_reader.ReadIntWithHeader();
						val.Unknown3A_1 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, DDB_Driver p_value)
		{
			p_writer.WriteByte(PROPERTY_UNKNOWN_28);
			p_writer.WriteStringWithHeader(p_value.Unknown28);

			p_writer.WriteByte(PROPERTY_UNKNOWN_29);
			p_writer.WriteStringWithHeader(p_value.Unknown29);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2A);
			p_writer.WriteStringWithHeader(p_value.Unknown2A);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2B);
			p_writer.WriteStringWithHeader(p_value.Unknown2B);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2C);
			p_writer.WriteIntWithHeader(p_value.Unknown2C);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2D);
			p_writer.WriteIntWithHeader(p_value.Unknown2D);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2E);
			p_writer.WriteIntWithHeader(p_value.Unknown2E);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2F);
			p_writer.WriteIntWithHeader(p_value.Unknown2F);

			p_writer.WriteByte(PROPERTY_UNKNOWN_30);
			p_writer.WriteIntWithHeader(p_value.Unknown30);

			p_writer.WriteByte(PROPERTY_UNKNOWN_31);
			p_writer.WriteIntWithHeader(p_value.Unknown31);

			p_writer.WriteByte(PROPERTY_UNKNOWN_33);
			p_writer.WriteIntWithHeader(p_value.Unknown33);

			p_writer.WriteByte(PROPERTY_UNKNOWN_34);
			p_writer.WriteIntWithHeader(p_value.Unknown34);

			p_writer.WriteByte(PROPERTY_UNKNOWN_35);
			p_writer.WriteIntWithHeader(p_value.Unknown35);

			p_writer.WriteByte(PROPERTY_UNKNOWN_36);
			p_writer.WriteIntWithHeader(p_value.Unknown36);

			p_writer.WriteByte(PROPERTY_UNKNOWN_37);
			p_writer.WriteIntWithHeader(p_value.Unknown37);

			p_writer.WriteByte(PROPERTY_UNKNOWN_38);
			p_writer.WriteIntWithHeader(p_value.Unknown38);

			if (p_value.HasUnknown3A)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3A);
				p_writer.WriteIntWithHeader(p_value.Unknown3A_0);
				p_writer.WriteIntWithHeader(p_value.Unknown3A_1);
			}
		}
	}
}