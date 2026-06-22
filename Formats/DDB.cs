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
			PROPERTY_MODEL_REF_1 = 0x28,
			PROPERTY_SKELETON_MODEL = 0x29,
			PROPERTY_MODEL_REF_2 = 0x2A,
			PROPERTY_ASSET_PREFIX = 0x2B,
			PROPERTY_STAT_1 = 0x2C,
			PROPERTY_STAT_2 = 0x2D,
			PROPERTY_STAT_3 = 0x2E,
			PROPERTY_STAT_4 = 0x2F,
			PROPERTY_STAT_5 = 0x30,
			PROPERTY_STAT_6 = 0x31,
			PROPERTY_DRIVER_ID = 0x33,
			PROPERTY_UNKNOWN_34 = 0x34,
			PROPERTY_HAT_INDEX = 0x35,
			PROPERTY_HEAD_INDEX = 0x36,
			PROPERTY_BODY_INDEX = 0x37,
			PROPERTY_LEGS_INDEX = 0x38,
			PROPERTY_BOSS_DATA = 0x3A;

		public string ModelRef1;
		public string SkeletonModel;
		public string ModelRef2;
		public string AssetPrefix;
		// TODO: resolve specific stat/param names via in-game mutation testing
		public int Stat1;
		public int Stat2;
		public int Stat3;
		public int Stat4;
		public int Stat5;
		public int Stat6;
		public int DriverId;
		// TODO: does not cleanly map to circuit number; semantics unresolved
		public int Unknown34;
		public int HatIndex;
		public int HeadIndex;
		public int BodyIndex;
		public int LegsIndex;
		public bool HasBossData = false;
		public int BossCircuit;
		public int BossTrack;

		public static DDB_Driver Read(LRBinaryReader p_reader)
		{
			DDB_Driver val = new DDB_Driver();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_MODEL_REF_1:
					{
						val.ModelRef1 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_SKELETON_MODEL:
					{
						val.SkeletonModel = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_MODEL_REF_2:
					{
						val.ModelRef2 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_ASSET_PREFIX:
					{
						val.AssetPrefix = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_STAT_1:
					{
						val.Stat1 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_STAT_2:
					{
						val.Stat2 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_STAT_3:
					{
						val.Stat3 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_STAT_4:
					{
						val.Stat4 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_STAT_5:
					{
						val.Stat5 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_STAT_6:
					{
						val.Stat6 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DRIVER_ID:
					{
						val.DriverId = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_34:
					{
						val.Unknown34 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_HAT_INDEX:
					{
						val.HatIndex = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_HEAD_INDEX:
					{
						val.HeadIndex = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_BODY_INDEX:
					{
						val.BodyIndex = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_LEGS_INDEX:
					{
						val.LegsIndex = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_BOSS_DATA:
					{
						val.HasBossData = true;
						val.BossCircuit = p_reader.ReadIntWithHeader();
						val.BossTrack = p_reader.ReadIntWithHeader();
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
			p_writer.WriteByte(PROPERTY_MODEL_REF_1);
			p_writer.WriteStringWithHeader(p_value.ModelRef1);

			p_writer.WriteByte(PROPERTY_SKELETON_MODEL);
			p_writer.WriteStringWithHeader(p_value.SkeletonModel);

			p_writer.WriteByte(PROPERTY_MODEL_REF_2);
			p_writer.WriteStringWithHeader(p_value.ModelRef2);

			p_writer.WriteByte(PROPERTY_ASSET_PREFIX);
			p_writer.WriteStringWithHeader(p_value.AssetPrefix);

			p_writer.WriteByte(PROPERTY_STAT_1);
			p_writer.WriteIntWithHeader(p_value.Stat1);

			p_writer.WriteByte(PROPERTY_STAT_2);
			p_writer.WriteIntWithHeader(p_value.Stat2);

			p_writer.WriteByte(PROPERTY_STAT_3);
			p_writer.WriteIntWithHeader(p_value.Stat3);

			p_writer.WriteByte(PROPERTY_STAT_4);
			p_writer.WriteIntWithHeader(p_value.Stat4);

			p_writer.WriteByte(PROPERTY_STAT_5);
			p_writer.WriteIntWithHeader(p_value.Stat5);

			p_writer.WriteByte(PROPERTY_STAT_6);
			p_writer.WriteIntWithHeader(p_value.Stat6);

			p_writer.WriteByte(PROPERTY_DRIVER_ID);
			p_writer.WriteIntWithHeader(p_value.DriverId);

			p_writer.WriteByte(PROPERTY_UNKNOWN_34);
			p_writer.WriteIntWithHeader(p_value.Unknown34);

			p_writer.WriteByte(PROPERTY_HAT_INDEX);
			p_writer.WriteIntWithHeader(p_value.HatIndex);

			p_writer.WriteByte(PROPERTY_HEAD_INDEX);
			p_writer.WriteIntWithHeader(p_value.HeadIndex);

			p_writer.WriteByte(PROPERTY_BODY_INDEX);
			p_writer.WriteIntWithHeader(p_value.BodyIndex);

			p_writer.WriteByte(PROPERTY_LEGS_INDEX);
			p_writer.WriteIntWithHeader(p_value.LegsIndex);

			if (p_value.HasBossData)
			{
				p_writer.WriteByte(PROPERTY_BOSS_DATA);
				p_writer.WriteIntWithHeader(p_value.BossCircuit);
				p_writer.WriteIntWithHeader(p_value.BossTrack);
			}
		}
	}
}
