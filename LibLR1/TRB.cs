using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class TRB {
        private const byte
            ID_UNKNOWN_27 = 0x27;

        private TRB_Unknown27[] m_Unknown27;

        public TRB_Unknown27[] Unknown27 { get { return m_Unknown27; } set { m_Unknown27 = value; } }

        public TRB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_UNKNOWN_27:
                        m_Unknown27 = BinaryFileHelper.ReadStructArrayBlock<TRB_Unknown27>(
                            stream,
                            new BinaryFileHelper.ReadObject<TRB_Unknown27>(
                                TRB_Unknown27.FromStream
                            ),
                            ID_UNKNOWN_27
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public TRB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }
    }

    public class TRB_Unknown27 {
        private const byte
            PROPERTY_29 = 0x29,
            PROPERTY_2A = 0x2A,
            PROPERTY_2B = 0x2B,
            PROPERTY_2D = 0x2D,
            PROPERTY_2E = 0x2E,
            PROPERTY_2F = 0x2F;

        public LRVector3 Vec29;
        public float Float2A;
        public int Int2B;
        public string String2D;
        public int Int2E;
        public bool Bool2F;

        public static TRB_Unknown27 FromStream(Stream stream) {
            TRB_Unknown27 val = new TRB_Unknown27();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_29:
                        val.Vec29 = LRVector3.FromStream(stream);
                        break;
                    case PROPERTY_2A:
                        val.Float2A = BinaryFileHelper.ReadFloatWithHeader(stream);
                        break;
                    case PROPERTY_2B:
                        val.Int2B = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_2D:
                        val.String2D = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_2E:
                        val.Int2E = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_2F:
                        val.Bool2F = true;
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
    }
}
