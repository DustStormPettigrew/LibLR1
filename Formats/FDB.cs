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
			: this(BinaryFileHelper.Decompress(p_filepath, true))
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
			PROPERTY_SPACING = 0x2C;

		public bool HasFlags;
		public bool HasColor;
		public int ColorR;
		public int ColorG;
		public int ColorB;
		public object[] Characters;
		public bool HasSpacing;
		public int Spacing;

		private static object ReadCharacterItem(LRBinaryReader p_reader)
		{
			if (p_reader.Next(Token.LeftBracket))
			{
				p_reader.Expect(Token.LeftBracket);
				List<object> items = new List<object>();
				while (!p_reader.Next(Token.RightBracket))
				{
					items.Add(ReadCharacterItem(p_reader));
				}
				p_reader.Expect(Token.RightBracket);
				return items.ToArray();
			}
			if (p_reader.Next(Token.String))
			{
				return p_reader.ReadStringWithHeader();
			}
			if (p_reader.Next(Token.Int32))
			{
				return p_reader.ReadIntWithHeader();
			}
			throw new UnexpectedTypeException(
				p_reader.ReadToken(),
				p_reader.BaseStream.Position - 1
			);
		}

		private static void WriteCharacterItem(LRBinaryWriter p_writer, object p_value)
		{
			if (p_value is string s)
			{
				p_writer.WriteStringWithHeader(s);
				return;
			}
			if (p_value is int n)
			{
				p_writer.WriteIntWithHeader(n);
				return;
			}
			if (p_value is object[] arr)
			{
				p_writer.WriteToken(Token.LeftBracket);
				for (int i = 0; i < arr.Length; i++)
				{
					WriteCharacterItem(p_writer, arr[i]);
				}
				p_writer.WriteToken(Token.RightBracket);
				return;
			}
			if (p_value is string[] stringArr)
			{
				p_writer.WriteToken(Token.LeftBracket);
				for (int i = 0; i < stringArr.Length; i++)
				{
					p_writer.WriteStringWithHeader(stringArr[i]);
				}
				p_writer.WriteToken(Token.RightBracket);
				return;
			}
			if (p_value is int[] intArr)
			{
				p_writer.WriteToken(Token.LeftBracket);
				for (int i = 0; i < intArr.Length; i++)
				{
					p_writer.WriteIntWithHeader(intArr[i]);
				}
				p_writer.WriteToken(Token.RightBracket);
				return;
			}
			throw new System.ArgumentException(
				"Unsupported FDB character item type: " + (p_value?.GetType().FullName ?? "null")
			);
		}

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
							chars.Add(ReadCharacterItem(p_reader));
						}
						p_reader.Expect(Token.RightBracket);
						val.Characters = chars.ToArray();
						break;
					}
					case PROPERTY_SPACING:
					{
						val.HasSpacing = true;
						val.Spacing = p_reader.ReadIntWithHeader();
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

			if (p_value.HasSpacing)
			{
				p_writer.WriteByte(PROPERTY_SPACING);
				p_writer.WriteIntWithHeader(p_value.Spacing);
			}

			if (p_value.Characters != null)
			{
				p_writer.WriteByte(PROPERTY_CHARACTERS);
				p_writer.WriteToken(Token.LeftBracket);
				for (int i = 0; i < p_value.Characters.Length; i++)
				{
					WriteCharacterItem(p_writer, p_value.Characters[i]);
				}
				p_writer.WriteToken(Token.RightBracket);
			}
		}
	}
}
