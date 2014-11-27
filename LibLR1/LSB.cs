using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class LSB {
        private const byte
            ID_LOADSCREEN = 0x27;

        private LSB_LoadScreen m_LoadScreen;

        public LSB_LoadScreen LoadScreen { get { return m_LoadScreen; } set { m_LoadScreen = value; } }

        public LSB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_LOADSCREEN:
                        m_LoadScreen = BinaryFileHelper.ReadStruct<LSB_LoadScreen>(
                            stream,
                            new BinaryFileHelper.ReadObject<LSB_LoadScreen>(
                                LSB_LoadScreen.FromStream
                            )
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public LSB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_LOADSCREEN);
            BinaryFileHelper.WriteStruct<LSB_LoadScreen>(
                stream,
                new BinaryFileHelper.WriteObject<LSB_LoadScreen>(
                    LSB_LoadScreen.ToStream
                ),
                m_LoadScreen
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class LSB_LoadScreen {
        private const byte
            PROPERTY_IMAGE = 0x28,
            PROPERTY_ICONS = 0x29,
            PROPERTY_ORDINAL = 0x2A;

        public string ImageName;
        public LRVector2[] Icons;
        public int Ordinal;

        public static LSB_LoadScreen FromStream(Stream stream) {
            LSB_LoadScreen val = new LSB_LoadScreen();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_IMAGE:
                        val.ImageName = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_ICONS:
                        val.Icons = BinaryFileHelper.ReadVector2fArrayBlock(stream);
                        break;
                    case PROPERTY_ORDINAL:
                        val.Ordinal = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, LSB_LoadScreen value) {
            stream.WriteByte(PROPERTY_IMAGE);
            BinaryFileHelper.WriteStringWithHeader(stream, value.ImageName);
            stream.WriteByte(PROPERTY_ICONS);
            BinaryFileHelper.WriteVector2fArrayBlock(stream, value.Icons);
            stream.WriteByte(PROPERTY_ORDINAL);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Ordinal);
        }
    }
}