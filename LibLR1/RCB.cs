using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Track listing format.
    /// </summary>
    public class RCB {
        private const byte
            ID_TRACK = 0x27;

        private Dictionary<string, RCB_Track> m_Tracks;

        public Dictionary<string, RCB_Track> Tracks { get { return m_Tracks; } set { m_Tracks = value; } }

        public RCB(Stream stream) {
            m_Tracks = new Dictionary<string, RCB_Track>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_TRACK:
                        m_Tracks = BinaryFileHelper.ReadDictionaryBlock<RCB_Track>(
                            stream,
                            new BinaryFileHelper.ReadObject<RCB_Track>(
                                RCB_Track.FromStream
                            ),
                            ID_TRACK
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public RCB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_TRACK);
            BinaryFileHelper.WriteDictionaryBlock<RCB_Track>(
                stream,
                new BinaryFileHelper.WriteObject<RCB_Track>(
                    RCB_Track.ToStream
                ),
                m_Tracks,
                ID_TRACK
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class RCB_Track {
        private const byte
            PROPERTY_POSITION_IN_CIRCUIT = 0x28,
            PROPERTY_FOLDER = 0x29,
            PROPERTY_CIRCUIT = 0x2A,
            PROPERTY_TRACK_ID = 0x2B,
            PROPERTY_MIRROR_FLAG = 0x2C,
            PROPERTY_THEME_STRING = 0x2D,
            PROPERTY_MASCOT = 0x2E;

        public int PositionInCircuit;
        public string Folder;
        public string Circuit;
        public int NameIndex;
        public bool Mirror;
        public string ThemeStr;
        public string Mascot;

        public RCB_Track()
            : this(0, "", "", 0, false, "", "") { }

        public RCB_Track(int positionincircuit, string folder, string circuit, int nameindex, bool mirror, string themestr, string mascot) {
            PositionInCircuit = positionincircuit;
            Folder = folder;
            Circuit = circuit;
            NameIndex = nameindex;
            Mirror = mirror;
            ThemeStr = themestr;
            Mascot = mascot;
        }

        public static RCB_Track FromStream(Stream stream) {
            RCB_Track val = new RCB_Track();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION_IN_CIRCUIT:
                        val.PositionInCircuit = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_FOLDER:
                        val.Folder = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_CIRCUIT:
                        val.Circuit = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_TRACK_ID:
                        val.NameIndex = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_MIRROR_FLAG:
                        val.Mirror = true;
                        break;
                    case PROPERTY_THEME_STRING:
                        val.ThemeStr = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_MASCOT:
                        val.Mascot = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, RCB_Track value) {
            stream.WriteByte(PROPERTY_POSITION_IN_CIRCUIT);
            BinaryFileHelper.WriteIntWithHeader(stream, value.PositionInCircuit);
            stream.WriteByte(PROPERTY_FOLDER);
            BinaryFileHelper.WriteStringWithHeader(stream, value.Folder);
            if (value.Circuit != "") {
                stream.WriteByte(PROPERTY_CIRCUIT);
                BinaryFileHelper.WriteStringWithHeader(stream, value.Circuit);
            }
            stream.WriteByte(PROPERTY_TRACK_ID);
            BinaryFileHelper.WriteIntWithHeader(stream, value.NameIndex);
            if (value.Mirror)
                stream.WriteByte(PROPERTY_MIRROR_FLAG);
            if (value.ThemeStr != "") {
                stream.WriteByte(PROPERTY_THEME_STRING);
                BinaryFileHelper.WriteStringWithHeader(stream, value.ThemeStr);
            }
            if (value.Mascot != "") {
                stream.WriteByte(PROPERTY_MASCOT);
                BinaryFileHelper.WriteStringWithHeader(stream, value.Mascot);
            }
        }
    }
}