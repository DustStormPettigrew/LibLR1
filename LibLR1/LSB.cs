using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.IO;

namespace LibLR1
{
	public class LSB
	{
		private const byte
			ID_LOADSCREEN = 0x27;

		private LSB_LoadScreen m_loadScreen;

		public LSB_LoadScreen LoadScreen
		{
			get { return m_loadScreen; }
			set { m_loadScreen = value; }
		}

		public LSB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public LSB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_LOADSCREEN:
					{
						m_loadScreen = p_reader.ReadStruct<LSB_LoadScreen>(
							LSB_LoadScreen.Read
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
			p_writer.WriteByte(ID_LOADSCREEN);
			p_writer.WriteStruct<LSB_LoadScreen>(
				LSB_LoadScreen.Write,
				m_loadScreen
			);
		}
	}

	public class LSB_LoadScreen
	{
		private const byte
			PROPERTY_IMAGE = 0x28,
			PROPERTY_ICONS = 0x29,
			PROPERTY_ORDINAL = 0x2A;

		public string ImageName;
		public LRVector2[] Icons;
		public int Ordinal;

		public static LSB_LoadScreen Read(LRBinaryReader p_reader)
		{
			LSB_LoadScreen val = new LSB_LoadScreen();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_IMAGE:
					{
						val.ImageName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_ICONS:
					{
						val.Icons = p_reader.ReadVector2fArrayBlock();
						break;
					}
					case PROPERTY_ORDINAL:
					{
						val.Ordinal = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, LSB_LoadScreen p_value)
		{
			p_writer.WriteByte(PROPERTY_IMAGE);
			p_writer.WriteStringWithHeader(p_value.ImageName);
			p_writer.WriteByte(PROPERTY_ICONS);
			p_writer.WriteVector2fArrayBlock(p_value.Icons);
			p_writer.WriteByte(PROPERTY_ORDINAL);
			p_writer.WriteIntWithHeader(p_value.Ordinal);
		}
	}
}