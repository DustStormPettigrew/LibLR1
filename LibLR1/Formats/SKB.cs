using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.Collections.Generic;

namespace LibLR1
{
	/// <summary>
	/// Skin/color gradient database format.
	/// </summary>
	public class SKB
	{
		private const byte
			ID_GRADIENT = 0x27,
			ID_GRADIENT_DICT_ARRAY = 0x2C,
			ID_PREFERREDSET = 0x2D,
			ID_UNKNOWN_FLOAT = 0x2E,
			PROPERTY_UNKNOWN_INT = 0x28,
			PROPERTY_COLOR_1 = 0x29,
			PROPERTY_COLOR_2 = 0x2A,
			PROPERTY_COLOR_3 = 0x2B;

		private Dictionary<string, SKB_Gradient>[] m_gradients;
		private string m_preferredSet;
		private float? m_unknownFloat;

		public Dictionary<string, SKB_Gradient>[] Gradients { get { return m_gradients; } set { m_gradients = value; } }
		public string PreferredSet { get { return m_preferredSet; } set { m_preferredSet = value; } }
		public float? UnknownFloat { get { return m_unknownFloat; } set { m_unknownFloat = value; } }

		public SKB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public SKB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_GRADIENT_DICT_ARRAY:
					{
						p_reader.Expect(Token.LeftBracket);
						int arrayLen = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_gradients = new Dictionary<string, SKB_Gradient>[arrayLen];
						for (int i = 0; i < arrayLen; i++)
						{
							p_reader.Expect((Token)ID_GRADIENT);
							p_reader.Expect(Token.LeftBracket);
							int dictLen = p_reader.ReadIntWithHeader();
							p_reader.Expect(Token.RightBracket);
							Dictionary<string, SKB_Gradient> dict = new Dictionary<string, SKB_Gradient>();
							for (int j = 0; j < dictLen; j++)
							{
								string key = p_reader.ReadStringWithHeader();
								p_reader.Expect(Token.LeftCurly);
								p_reader.Expect((Token)ID_GRADIENT);
								SKB_Gradient gradient = p_reader.ReadStruct<SKB_Gradient>(SKB_Gradient.FromStream);
								p_reader.Expect(Token.RightCurly);
								dict.Add(key, gradient);
							}
							m_gradients[i] = dict;
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_PREFERREDSET:
					{
						m_preferredSet = p_reader.ReadStringWithHeader();
						break;
					}
					case ID_UNKNOWN_FLOAT:
					{
						m_unknownFloat = p_reader.ReadFloatWithHeader();
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
	}

	public class SKB_Gradient
	{
		private const byte
			PROPERTY_UNKNOWN_INT = 0x28,
			PROPERTY_COLOR_1 = 0x29,
			PROPERTY_COLOR_2 = 0x2A,
			PROPERTY_COLOR_3 = 0x2B;

		public int? UnknownInt;
		public LRColor Color1;
		public LRColor Color2;
		public LRColor Color3;

		public static SKB_Gradient FromStream(LRBinaryReader p_reader)
		{
			SKB_Gradient val = new SKB_Gradient();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_INT:
					{
						val.UnknownInt = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_COLOR_1:
					{
						val.Color1 = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					case PROPERTY_COLOR_2:
					{
						val.Color2 = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					case PROPERTY_COLOR_3:
					{
						val.Color3 = LRColor.ReadNoAlpha(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, SKB_Gradient p_value)
		{
			if (p_value.UnknownInt.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_INT);
				p_writer.WriteIntWithHeader(p_value.UnknownInt.Value);
			}
			p_writer.WriteByte(PROPERTY_COLOR_1);
			LRColor.WriteNoAlpha(p_writer, p_value.Color1);
			p_writer.WriteByte(PROPERTY_COLOR_2);
			LRColor.WriteNoAlpha(p_writer, p_value.Color2);
			p_writer.WriteByte(PROPERTY_COLOR_3);
			LRColor.WriteNoAlpha(p_writer, p_value.Color3);
		}
	}
}