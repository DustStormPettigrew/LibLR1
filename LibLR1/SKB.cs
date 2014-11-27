using System;
using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Private while I fix it.
    /// </summary>
    class SKB {
        private const byte
            ID_GRADIENT = 0x27,
            ID_GRADIENT_DICT_ARRAY = 0x2C,
            ID_PREFERREDSET = 0x2D,
            ID_UNKNOWN_FLOAT = 0x2E,
            PROPERTY_UNKNOWN_INT = 0x28,
            PROPERTY_COLOR_1 = 0x29,
            PROPERTY_COLOR_2 = 0x2A,
            PROPERTY_COLOR_3 = 0x2B;

        private Dictionary<string, SKB_Gradient>[] m_Gradients;

        public Dictionary<string, SKB_Gradient>[] Gradients { get { return m_Gradients; } set { m_Gradients = value; } }

        public SKB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_GRADIENT_DICT_ARRAY:
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
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public SKB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }
    }

    public class SKB_Gradient {
        private const byte
            PROPERTY_UNKNOWN_INT = 0x28,
            PROPERTY_COLOR_1 = 0x29,
            PROPERTY_COLOR_2 = 0x2A,
            PROPERTY_COLOR_3 = 0x2B;

        public int? UnknownInt;
        public LRColor Color1;
        public LRColor Color2;
        public LRColor Color3;

        public static SKB_Gradient FromStream(Stream stream) {
            SKB_Gradient val = new SKB_Gradient();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_UNKNOWN_INT:
                        val.UnknownInt = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_COLOR_1:
                        val.Color1 = LRColor.FromStreamNoAlpha(stream);
                        break;
                    case PROPERTY_COLOR_2:
                        val.Color2 = LRColor.FromStreamNoAlpha(stream);
                        break;
                    case PROPERTY_COLOR_3:
                        val.Color3 = LRColor.FromStreamNoAlpha(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, SKB_Gradient value) {
            if (value.UnknownInt.HasValue) {
                stream.WriteByte(PROPERTY_UNKNOWN_INT);
                BinaryFileHelper.WriteIntWithHeader(stream, value.UnknownInt.Value);
            }
            stream.WriteByte(PROPERTY_COLOR_1);
            LRColor.ToStreamNoAlpha(stream, value.Color1);
            stream.WriteByte(PROPERTY_COLOR_2);
            LRColor.ToStreamNoAlpha(stream, value.Color2);
            stream.WriteByte(PROPERTY_COLOR_3);
            LRColor.ToStreamNoAlpha(stream, value.Color3);
        }
    }
}