using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class MDB {
        public const byte
            ID_MATERIALS = 0x27;

        private Dictionary<string, MDB_Material> m_Materials;

        public Dictionary<string, MDB_Material> Materials {
            get { return m_Materials; }
            set { m_Materials = value; }
        }

        public MDB(Stream stream) {
            m_Materials = new Dictionary<string, MDB_Material>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_MATERIALS:
                        m_Materials = BinaryFileHelper.ReadDictionaryBlock<MDB_Material>(
                            stream,
                            new BinaryFileHelper.ReadObject<MDB_Material>(
                                MDB_Material.FromStream
                            ),
                            ID_MATERIALS
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public MDB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_MATERIALS);
            BinaryFileHelper.WriteDictionaryBlock<MDB_Material>(
                stream,
                new BinaryFileHelper.WriteObject<MDB_Material>(
                    MDB_Material.ToStream
                ),
                m_Materials,
                ID_MATERIALS
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class MDB_Material {
        public const byte
            PROPERTY_AMBIENT_COLOR = 0x28,
            PROPERTY_DIFFUSE_COLOR = 0x29,
            PROPERTY_2A = 0x2A,
            PROPERTY_2B = 0x2B,
            PROPERTY_TEXTURE_NAME = 0x2C,
            PROPERTY_2D = 0x2D,
            PROPERTY_2E = 0x2E,
            PROPERTY_38 = 0x38,
            PROPERTY_3A = 0x3A,
            PROPERTY_3F = 0x3F,
            PROPERTY_41 = 0x41,
            PROPERTY_45 = 0x45,
            PROPERTY_ALPHA = 0x46,
            PROPERTY_4A = 0x4A;

        public LRColor AmbientColor;
        public LRColor DiffuseColor;
        public bool Bool2A;
        public bool Bool2B;
        public string TextureName;
        public bool Bool2D;
        public bool Bool2E;
        public bool Bool38;
        public bool Bool3A;
        public bool Bool3F;
        public bool Bool41;
        public bool Bool45;
        public int? Alpha;
        public bool Bool4A;

        public static MDB_Material FromStream(Stream stream) {
            MDB_Material val = new MDB_Material();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_AMBIENT_COLOR:
                        val.AmbientColor = LRColor.FromStream(stream);
                        break;
                    case PROPERTY_DIFFUSE_COLOR:
                        val.DiffuseColor = LRColor.FromStream(stream);
                        break;
                    case PROPERTY_2A:
                        val.Bool2A = true;
                        break;
                    case PROPERTY_2B:
                        val.Bool2B = true;
                        break;
                    case PROPERTY_TEXTURE_NAME:
                        val.TextureName = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_2D:
                        val.Bool2D = true;
                        break;
                    case PROPERTY_2E:
                        val.Bool2E = true;
                        break;
                    case PROPERTY_38:
                        val.Bool38 = true;
                        break;
                    case PROPERTY_3A:
                        val.Bool3A = true;
                        break;
                    case PROPERTY_3F:
                        val.Bool3F = true;
                        break;
                    case PROPERTY_41:
                        val.Bool41 = true;
                        break;
                    case PROPERTY_45:
                        val.Bool45 = true;
                        break;
                    case PROPERTY_ALPHA:
                        val.Alpha = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_4A:
                        val.Bool4A = true;
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
        public static void ToStream(Stream stream, MDB_Material value) {
            if (value.AmbientColor != null) {
                stream.WriteByte(PROPERTY_AMBIENT_COLOR);
                LRColor.ToStream(stream, value.AmbientColor);
            }
            if (value.DiffuseColor != null) {
                stream.WriteByte(PROPERTY_DIFFUSE_COLOR);
                LRColor.ToStream(stream, value.DiffuseColor);
            }
            if (value.Bool2A)
                stream.WriteByte(PROPERTY_2A);
            if (value.Bool2B)
                stream.WriteByte(PROPERTY_2B);
            if (value.TextureName != null) {
                stream.WriteByte(PROPERTY_TEXTURE_NAME);
                BinaryFileHelper.WriteStringWithHeader(stream, value.TextureName);
            }
            if (value.Bool2D)
                stream.WriteByte(PROPERTY_2D);
            if (value.Bool2E)
                stream.WriteByte(PROPERTY_2E);
            if (value.Bool38)
                stream.WriteByte(PROPERTY_38);
            if (value.Bool3A)
                stream.WriteByte(PROPERTY_3A);
            if (value.Bool3F)
                stream.WriteByte(PROPERTY_3F);
            if (value.Bool41)
                stream.WriteByte(PROPERTY_41);
            if (value.Bool45)
                stream.WriteByte(PROPERTY_45);
            if (value.Alpha.HasValue) {
                stream.WriteByte(PROPERTY_ALPHA);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Alpha.Value);
            }
            if (value.Bool4A)
                stream.WriteByte(PROPERTY_4A);
        }
    }
}