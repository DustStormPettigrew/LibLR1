using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Font Database format. Defines named font character sets.
	/// </summary>
	public class FDB
	{
		private const byte
			ID_FONTS = 0x27;

		private Dictionary<string, FDB_Font> m_fonts;

		public Dictionary<string, FDB_Font> Fonts
		{
			get { return m_fonts; }
			set { m_fonts = value; }
		}

		public FDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public FDB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_FONTS:
					{
						m_fonts = p_reader.ReadDictionaryBlock<FDB_Font>(
							FDB_Font.Read,
							ID_FONTS
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
			p_writer.WriteByte(ID_FONTS);
			p_writer.WriteDictionaryBlock<FDB_Font>(
				FDB_Font.Write,
				m_fonts,
				ID_FONTS
			);
		}
	}

	public class FDB_Font
	{
		private const byte
			PROPERTY_FLAGS = 0x28,
			PROPERTY_CHARACTERS = 0x2B,
			PROPERTY_COLOR = 0x2A,
			PROPERTY_UNKNOWN_2C = 0x2C;

		public bool HasFlags;
		public bool HasColor;
		public int ColorR;
		public int ColorG;
		public int ColorB;
		public object[] Characters;
		public bool HasUnknown2C;
		public int Unknown2C;

		public static FDB_Font Read(LRBinaryReader p_reader)
		{
			FDB_Font val = new FDB_Font();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_FLAGS:
					{
						val.HasFlags = true;
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
					case PROPERTY_CHARACTERS:
					{
						p_reader.Expect(Token.LeftBracket);
						List<object> chars = new List<object>();
						while (!p_reader.Next(Token.RightBracket))
						{
							if (p_reader.Next(Token.String))
								chars.Add(p_reader.ReadStringWithHeader());
							else if (p_reader.Next(Token.Int32))
								chars.Add(p_reader.ReadIntWithHeader());
							else
								break;
						}
						p_reader.Expect(Token.RightBracket);
						val.Characters = chars.ToArray();
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.HasUnknown2C = true;
						val.Unknown2C = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, FDB_Font p_value)
		{
			if (p_value.HasFlags)
			{
				p_writer.WriteByte(PROPERTY_FLAGS);
			}

			if (p_value.HasColor)
			{
				p_writer.WriteByte(PROPERTY_COLOR);
				p_writer.WriteIntWithHeader(p_value.ColorR);
				p_writer.WriteIntWithHeader(p_value.ColorG);
				p_writer.WriteIntWithHeader(p_value.ColorB);
			}

			if (p_value.HasUnknown2C)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2C);
				p_writer.WriteIntWithHeader(p_value.Unknown2C);
			}

			if (p_value.Characters != null)
			{
				p_writer.WriteByte(PROPERTY_CHARACTERS);
				p_writer.WriteToken(Token.LeftBracket);
				for (int i = 0; i < p_value.Characters.Length; i++)
				{
					if (p_value.Characters[i] is string s)
						p_writer.WriteStringWithHeader(s);
					else if (p_value.Characters[i] is int n)
						p_writer.WriteIntWithHeader(n);
				}
				p_writer.WriteToken(Token.RightBracket);
			}
		}
	}
}
