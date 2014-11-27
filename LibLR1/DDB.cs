using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Drivers Database.
    /// </summary>
    public class DDB {
        private const byte
            ID_DRIVERS = 0x27;

        private Dictionary<string, DDB_Driver> m_Drivers;

        public Dictionary<string, DDB_Driver> Drivers {
            get { return m_Drivers; }
            set { m_Drivers = value; }
        }

        public DDB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_DRIVERS:
                        m_Drivers = BinaryFileHelper.ReadDictionaryBlock<DDB_Driver>(
                            stream,
                            new BinaryFileHelper.ReadObject<DDB_Driver>(
                                DDB_Driver.FromStream
                            ),
                            ID_DRIVERS
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public DDB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_DRIVERS);
            BinaryFileHelper.WriteDictionaryBlock<DDB_Driver>(
                stream,
                new BinaryFileHelper.WriteObject<DDB_Driver>(
                    DDB_Driver.ToStream
                ),
                m_Drivers,
                ID_DRIVERS
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class DDB_Driver {
        private const byte
            PROPERTY_UNKNOWN_28 = 0x28,
            PROPERTY_UNKNOWN_29 = 0x29,
            PROPERTY_UNKNOWN_2A = 0x2A,
            PROPERTY_UNKNOWN_2B = 0x2B,
            PROPERTY_UNKNOWN_2C = 0x2C,
            PROPERTY_UNKNOWN_2D = 0x2D,
            PROPERTY_UNKNOWN_2E = 0x2E,
            PROPERTY_UNKNOWN_2F = 0x2F,
            PROPERTY_UNKNOWN_30 = 0x30,
            PROPERTY_UNKNOWN_31 = 0x31,
            PROPERTY_UNKNOWN_33 = 0x33,
            PROPERTY_UNKNOWN_34 = 0x34,
            PROPERTY_UNKNOWN_35 = 0x35,
            PROPERTY_UNKNOWN_36 = 0x36,
            PROPERTY_UNKNOWN_37 = 0x37,
            PROPERTY_UNKNOWN_38 = 0x38,
            PROPERTY_UNKNOWN_3A = 0x3A;

        public string Unknown28;
        public string Unknown29;
        public string Unknown2A;
        public string Unknown2B;
        public int Unknown2C;
        public int Unknown2D;
        public int Unknown2E;
        public int Unknown2F;
        public int Unknown30;
        public int Unknown31;
        public int Unknown33;
        public int Unknown34;
        public int Unknown35;
        public int Unknown36;
        public int Unknown37;
        public int Unknown38;
        public bool HasUnknown3A = false;
        public int Unknown3A_0;
        public int Unknown3A_1;

        public static DDB_Driver FromStream(Stream stream) {
            DDB_Driver val = new DDB_Driver();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_UNKNOWN_28:
                        val.Unknown28 = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_29:
                        val.Unknown29 = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2A:
                        val.Unknown2A = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2B:
                        val.Unknown2B = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2C:
                        val.Unknown2C = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2D:
                        val.Unknown2D = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2E:
                        val.Unknown2E = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_2F:
                        val.Unknown2F = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_30:
                        val.Unknown30 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_31:
                        val.Unknown31 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_33:
                        val.Unknown33 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_34:
                        val.Unknown34 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_35:
                        val.Unknown35 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_36:
                        val.Unknown36 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_37:
                        val.Unknown37 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_38:
                        val.Unknown38 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_UNKNOWN_3A:
                        val.HasUnknown3A = true;
                        val.Unknown3A_0 = BinaryFileHelper.ReadIntWithHeader(stream);
                        val.Unknown3A_1 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, DDB_Driver value) {
            stream.WriteByte(PROPERTY_UNKNOWN_28);
            BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown28);

            stream.WriteByte(PROPERTY_UNKNOWN_29);
            BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown29);

            stream.WriteByte(PROPERTY_UNKNOWN_2A);
            BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown2A);

            stream.WriteByte(PROPERTY_UNKNOWN_2B);
            BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown2B);

            stream.WriteByte(PROPERTY_UNKNOWN_2C);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown2C);

            stream.WriteByte(PROPERTY_UNKNOWN_2D);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown2D);

            stream.WriteByte(PROPERTY_UNKNOWN_2E);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown2E);

            stream.WriteByte(PROPERTY_UNKNOWN_2F);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown2F);

            stream.WriteByte(PROPERTY_UNKNOWN_30);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown30);

            stream.WriteByte(PROPERTY_UNKNOWN_31);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown31);

            stream.WriteByte(PROPERTY_UNKNOWN_33);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown33);

            stream.WriteByte(PROPERTY_UNKNOWN_34);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown34);

            stream.WriteByte(PROPERTY_UNKNOWN_35);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown35);

            stream.WriteByte(PROPERTY_UNKNOWN_36);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown36);

            stream.WriteByte(PROPERTY_UNKNOWN_37);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown37);

            stream.WriteByte(PROPERTY_UNKNOWN_38);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown38);

            if (value.HasUnknown3A) {
                stream.WriteByte(PROPERTY_UNKNOWN_3A);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown3A_0);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown3A_1);
            }
        }
    }
}