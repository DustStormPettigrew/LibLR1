using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class TDB {
        private const byte
            ID_TEXTURES = 0x27,
            PROPERTY_28 = 0x28,
            PROPERTY_BMP_TGA = 0x2A,
            PROPERTY_COLOR_2C = 0x2C,
            PROPERTY_2D = 0x2D;

        private Dictionary<string, TDB_Texture> m_Textures;

        public Dictionary<string, TDB_Texture> Textures { get { return m_Textures; } set { m_Textures = value; } }

        public TDB(Stream stream) {
            m_Textures = new Dictionary<string, TDB_Texture>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_TEXTURES:
                        m_Textures = BinaryFileHelper.ReadDictionaryBlock<TDB_Texture>(
                            stream,
                            new BinaryFileHelper.ReadObject<TDB_Texture>(
                                TDB_Texture.FromStream
                            ),
                            ID_TEXTURES
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public TDB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_TEXTURES);
            BinaryFileHelper.WriteDictionaryBlock<TDB_Texture>(
                stream,
                new BinaryFileHelper.WriteObject<TDB_Texture>(
                    TDB_Texture.ToStream
                ),
                m_Textures,
                ID_TEXTURES
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class TDB_Texture {
        private const byte
            PROPERTY_28 = 0x28,
            PROPERTY_BMP_TGA = 0x2A,
            PROPERTY_2B = 0x2B,
            PROPERTY_COLOR_2C = 0x2C,
            PROPERTY_2D = 0x2D;

        public bool Bool28;
        public bool IsBitmap;
        public bool Bool2B;
        public bool HasColor2C;
        public LRColor Color2C;
        public bool Bool2D;

        public TDB_Texture()
            : this(false, false, false, false, new LRColor(), false) { }

        public TDB_Texture(bool bool28, bool isbitmap, bool bool2b, bool hascolor2c, LRColor color2c, bool bool2d) {
            Bool28 = bool28;
            IsBitmap = isbitmap;
            Bool2B = bool2b;
            HasColor2C = hascolor2c;
            Color2C = color2c;
            Bool2D = bool2d;
        }

        public static TDB_Texture FromStream(Stream stream) {
            TDB_Texture val = new TDB_Texture();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_28:
                        val.Bool28 = true;
                        break;
                    case PROPERTY_BMP_TGA:
                        val.IsBitmap = true;
                        break;
                    case PROPERTY_2B:
                        val.Bool2B = true;
                        break;
                    case PROPERTY_COLOR_2C:
                        val.HasColor2C = true;
                        val.Color2C = LRColor.FromStreamNoAlpha(stream);
                        break;
                    case PROPERTY_2D:
                        val.Bool2D = true;
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, TDB_Texture value) {
            if (value.Bool28)
                stream.WriteByte(PROPERTY_28);
            if (value.IsBitmap)
                stream.WriteByte(PROPERTY_BMP_TGA);
            if (value.Bool2B)
                stream.WriteByte(PROPERTY_2B);
            if (value.HasColor2C) {
                stream.WriteByte(PROPERTY_COLOR_2C);
                LRColor.ToStream(stream, value.Color2C);
            }
            if (value.Bool2D)
                stream.WriteByte(PROPERTY_2D);
        }
    }
}