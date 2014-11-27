using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	public class TDB
	{
		private const byte
			ID_TEXTURES       = 0x27,
			PROPERTY_28       = 0x28,
			PROPERTY_BMP_TGA  = 0x2A,
			PROPERTY_COLOR_2C = 0x2C,
			PROPERTY_2D       = 0x2D;

		private Dictionary<string, TDB_Texture> m_textures;

		public Dictionary<string, TDB_Texture> Textures { get { return m_textures; } set { m_textures = value; } }
		
		public TDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public TDB(LRBinaryReader p_reader)
		{
			m_textures = new Dictionary<string, TDB_Texture>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TEXTURES:
					{
						m_textures = p_reader.ReadDictionaryBlock<TDB_Texture>(
							TDB_Texture.Read,
							ID_TEXTURES
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
			p_writer.WriteByte(ID_TEXTURES);
			p_writer.WriteDictionaryBlock<TDB_Texture>(
				TDB_Texture.Write,
				m_textures,
				ID_TEXTURES
			);
		}
	}

	public class TDB_Texture
	{
		private const byte
			PROPERTY_28       = 0x28,
			PROPERTY_BMP_TGA  = 0x2A,
			PROPERTY_2B       = 0x2B,
			PROPERTY_COLOR_2C = 0x2C,
			PROPERTY_2D       = 0x2D;

		public bool    Bool28;
		public bool    IsBitmap;
		public bool    Bool2B;
		public bool    HasColor2C;
		public LRColor Color2C;
		public bool    Bool2D;

		public TDB_Texture()
			: this(false, false, false, false, new LRColor(), false) { }

		public TDB_Texture(bool p_bool28, bool p_isbitmap, bool p_bool2b, bool p_hascolor2c, LRColor p_color2c, bool p_bool2d)
		{
			Bool28     = p_bool28;
			IsBitmap   = p_isbitmap;
			Bool2B     = p_bool2b;
			HasColor2C = p_hascolor2c;
			Color2C    = p_color2c;
			Bool2D     = p_bool2d;
		}

		public static TDB_Texture Read(LRBinaryReader p_reader)
		{
			TDB_Texture val = new TDB_Texture();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_28:
					{
						val.Bool28 = true;
						break;
					}
					case PROPERTY_BMP_TGA:
					{
						val.IsBitmap = true;
						break;
					}
					case PROPERTY_2B:
					{
						val.Bool2B = true;
						break;
					}
					case PROPERTY_COLOR_2C:
					{
						val.HasColor2C = true;
						val.Color2C = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					case PROPERTY_2D:
					{
						val.Bool2D = true;
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

		public static void Write(LRBinaryWriter p_writer, TDB_Texture p_value)
		{
			if (p_value.Bool28)
			{
				p_writer.WriteByte(PROPERTY_28);
			}
			if (p_value.IsBitmap)
			{
				p_writer.WriteByte(PROPERTY_BMP_TGA);
			}
			if (p_value.Bool2B)
			{
				p_writer.WriteByte(PROPERTY_2B);
			}
			if (p_value.HasColor2C)
			{
				p_writer.WriteByte(PROPERTY_COLOR_2C);
				LRColor.WriteNoAlpha(p_writer, p_value.Color2C);
			}
			if (p_value.Bool2D)
			{
				p_writer.WriteByte(PROPERTY_2D);
			}
		}
	}
}