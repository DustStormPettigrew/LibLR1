using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Texture database format.
	/// </summary>
	public class TDB
	{
		private const byte
			ID_TEXTURES = 0x27,
			PROPERTY_FLIP_VERTICAL = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_BMP = 0x2A,
			PROPERTY_TGA = 0x2B,
			PROPERTY_CHROMA_KEY = 0x2C,
			PROPERTY_2D = 0x2D,
			PROPERTY_UNKNOWN_2E = 0x2E;

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
			PROPERTY_FLIP_VERTICAL = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_BMP = 0x2A,
			PROPERTY_TGA = 0x2B,
			PROPERTY_CHROMA_KEY = 0x2C,
			PROPERTY_2D = 0x2D,
			PROPERTY_UNKNOWN_2E = 0x2E;

		public bool FlipVertical;
		public bool HasUnknown29;
		public int Unknown29;
		public bool IsBitmap;
		public bool IsTga;
		public bool HasChromaKey;
		public LRColor ChromaKey;
		public bool Bool2D;
		public bool Unknown2E;

		public TDB_Texture()
			: this(false, false, false, false, new LRColor(), false) { }

		public TDB_Texture(bool p_flipvertical, bool p_isbitmap, bool p_istga, bool p_haschromakey, LRColor p_chromakey, bool p_bool2d)
		{
			FlipVertical = p_flipvertical;
			IsBitmap = p_isbitmap;
			IsTga = p_istga;
			HasChromaKey = p_haschromakey;
			ChromaKey = p_chromakey;
			Bool2D = p_bool2d;
		}

		public static TDB_Texture Read(LRBinaryReader p_reader)
		{
			TDB_Texture val = new TDB_Texture();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_FLIP_VERTICAL:
					{
						val.FlipVertical = true;
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.HasUnknown29 = true;
						val.Unknown29 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_BMP:
					{
						val.IsBitmap = true;
						break;
					}
					case PROPERTY_TGA:
					{
						val.IsTga = true;
						break;
					}
					case PROPERTY_CHROMA_KEY:
					{
						val.HasChromaKey = true;
						val.ChromaKey = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					case PROPERTY_2D:
					{
						val.Bool2D = true;
						break;
					}
					case PROPERTY_UNKNOWN_2E:
					{
						val.Unknown2E = true;
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
			if (p_value.FlipVertical)
			{
				p_writer.WriteByte(PROPERTY_FLIP_VERTICAL);
			}
			if (p_value.HasUnknown29)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_29);
				p_writer.WriteIntWithHeader(p_value.Unknown29);
			}
			if (p_value.IsBitmap)
			{
				p_writer.WriteByte(PROPERTY_BMP);
			}
			if (p_value.IsTga)
			{
				p_writer.WriteByte(PROPERTY_TGA);
			}
			if (p_value.HasChromaKey)
			{
				p_writer.WriteByte(PROPERTY_CHROMA_KEY);
				LRColor.WriteNoAlpha(p_writer, p_value.ChromaKey);
			}
			if (p_value.Bool2D)
			{
				p_writer.WriteByte(PROPERTY_2D);
			}
			if (p_value.Unknown2E)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2E);
			}
		}
	}
}
