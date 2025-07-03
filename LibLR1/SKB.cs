using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.Collections.Generic;

namespace LibLR1
{
	/// <summary>
	/// Private while I fix it.
	/// </summary>
	class SKB
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

		public Dictionary<string, SKB_Gradient>[] Gradients { get { return m_gradients; } set { m_gradients = value; } }

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
						throw new NotImplementedException("Will needs to work out how to do recursive array/dict loading :D");
						/*_Gradients = BinaryFileHelper.ReadArrayBlock<Dictionary<string,SKB_Gradient>>(
							stream,
							new BinaryFileHelper.ReadObject<Dictionary<string,SKB_Gradient>>(
								BinaryFileHelper.ReadDictionaryBlock<SKB_Gradient>(
									stream, SKB_Gradient.FromStream, ID_GRADIENT
								)
							)
						);*/
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