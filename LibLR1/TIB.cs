using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class TIB {
        private const byte
            ID_UNKNOWN_27 = 0x27;

        private TIB_Unknown27[] m_Unknown27;

        public TIB_Unknown27[] Unknown27 { get { return m_Unknown27; } set { m_Unknown27 = value; } }

        public TIB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_UNKNOWN_27:
                        m_Unknown27 = BinaryFileHelper.ReadStructArrayBlock<TIB_Unknown27>(
                            stream,
                            new BinaryFileHelper.ReadObject<TIB_Unknown27>(
                                TIB_Unknown27.FromStream
                            ),
                            ID_UNKNOWN_27
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public TIB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }
    }

    public class TIB_Unknown27 {
        private const byte
            PROPERTY_28 = 0x28,
            PROPERTY_29 = 0x29,
            PROPERTY_2A = 0x2A,
            PROPERTY_2B = 0x2B,
            PROPERTY_2D = 0x2D;

        public int Int28;
        public int Int29;
        public int Int2A;
        public int Int2D;

        public static TIB_Unknown27 FromStream(Stream stream) {
            TIB_Unknown27 val = new TIB_Unknown27();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_28:
                        if (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_INT32))
                            stream.Position++;  // 0x2B bodge
                        val.Int28 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_29:
                        if (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_INT32))
                            stream.Position++;  // 0x2B bodge
                        val.Int29 = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_2A:
                        if (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_INT32))
                            stream.Position++;  // 0x2B bodge
                        val.Int2A = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_2D:
                        if (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_INT32))
                            stream.Position++;  // 0x2B bodge
                        val.Int2D = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
    }
}