using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// 2D Image List format. Defines named 2D images used in the HUD and menus.
	/// </summary>
	public class IDB
	{
		private const byte
			ID_IMAGES = 0x27;

		private Dictionary<string, IDB_Image> m_images;

		public Dictionary<string, IDB_Image> Images
		{
			get { return m_images; }
			set { m_images = value; }
		}

		public IDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public IDB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_IMAGES:
					{
						m_images = p_reader.ReadDictionaryBlock<IDB_Image>(
							IDB_Image.Read,
							ID_IMAGES
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
			p_writer.WriteByte(ID_IMAGES);
			p_writer.WriteDictionaryBlock<IDB_Image>(
				IDB_Image.Write,
				m_images,
				ID_IMAGES
			);
		}
	}

	public class IDB_Image
	{
		private const byte
			PROPERTY_TRANSPARENT = 0x29,
			PROPERTY_COLOR = 0x2B;

		public bool Transparent;
		public bool HasColor;
		public int ColorR;
		public int ColorG;
		public int ColorB;

		public static IDB_Image Read(LRBinaryReader p_reader)
		{
			IDB_Image val = new IDB_Image();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_TRANSPARENT:
					{
						val.Transparent = true;
						break;
					}
					case PROPERTY_COLOR:
					{
						val.HasColor = true;
						val.ColorR = p_reader.ReadIntWithHeader();
						val.ColorG = p_reader.ReadIntWithHeader();
						val.ColorB = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, IDB_Image p_value)
		{
			if (p_value.Transparent)
			{
				p_writer.WriteByte(PROPERTY_TRANSPARENT);
			}

			if (p_value.HasColor)
			{
				p_writer.WriteByte(PROPERTY_COLOR);
				p_writer.WriteIntWithHeader(p_value.ColorR);
				p_writer.WriteIntWithHeader(p_value.ColorG);
				p_writer.WriteIntWithHeader(p_value.ColorB);
			}
		}
	}
}
