using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {

    /// <summary>
    /// "AI" path format
    /// </summary>
    public class RRB {
        private const byte
            ID_NODES = 0x27,
            ID_UNKNOWN_28 = 0x28,
            ID_UNKNOWN_29 = 0x29,
            ID_UNKNOWN_2A = 0x2A,
            ID_UNKNOWN_2B = 0x2B,
            ID_UNKNOWN_2C = 0x2C,
            ID_UNKNOWN_2D = 0x2D;

        private RRB_Node[] m_Nodes;
        private LRQuaternion m_Unknown28;
        private LRVector3 m_Unknown29;   // probably initial position, need to check this.
        private LRVector3 m_Unknown2A;
        private LRQuaternion m_Unknown2B;
        private int m_Unknown2C;
        private int m_Unknown2D;

        public RRB_Node[] Nodes { get { return m_Nodes; } set { m_Nodes = value; } }
        public LRQuaternion Unknown28 { get { return m_Unknown28; } set { m_Unknown28 = value; } }
        public LRVector3 Unknown29 { get { return m_Unknown29; } set { m_Unknown29 = value; } }
        public LRVector3 Unknown2A { get { return m_Unknown2A; } set { m_Unknown2A = value; } }
        public LRQuaternion Unknown2B { get { return m_Unknown2B; } set { m_Unknown2B = value; } }
        public int Unknown2C { get { return m_Unknown2C; } set { m_Unknown2C = value; } }
        public int Unknown2D { get { return m_Unknown2D; } set { m_Unknown2D = value; } }

        public RRB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_NODES:
                        m_Nodes = BinaryFileHelper.ReadArrayBlock<RRB_Node>(
                            stream,
                            new BinaryFileHelper.ReadObject<RRB_Node>(
                                RRB_Node.FromStream
                            )
                        );
                        break;
                    case ID_UNKNOWN_28:
                        m_Unknown28 = LRQuaternion.FromStream(stream);
                        break;
                    case ID_UNKNOWN_29:
                        m_Unknown29 = LRVector3.FromStream(stream);
                        break;
                    case ID_UNKNOWN_2A:
                        m_Unknown2A = LRVector3.FromStream(stream);
                        break;
                    case ID_UNKNOWN_2B:
                        m_Unknown2B = LRQuaternion.FromStream(stream);
                        break;
                    case ID_UNKNOWN_2C:
                        m_Unknown2C = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case ID_UNKNOWN_2D:
                        m_Unknown2D = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public RRB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_UNKNOWN_28);
            LRQuaternion.ToStream(stream, m_Unknown28);
            stream.WriteByte(ID_UNKNOWN_29);
            LRVector3.ToStream(stream, m_Unknown29);
            stream.WriteByte(ID_UNKNOWN_2A);
            LRVector3.ToStream(stream, m_Unknown2A);
            stream.WriteByte(ID_UNKNOWN_2B);
            LRQuaternion.ToStream(stream, m_Unknown2B);
            stream.WriteByte(ID_UNKNOWN_2C);
            BinaryFileHelper.WriteIntWithHeader(stream, m_Unknown2C);
            stream.WriteByte(ID_UNKNOWN_2D);
            BinaryFileHelper.WriteIntWithHeader(stream, m_Unknown2D);
            stream.WriteByte(ID_NODES);
            BinaryFileHelper.WriteArrayBlock<RRB_Node>(
                stream,
                new BinaryFileHelper.WriteObject<RRB_Node>(
                    RRB_Node.ToStream
                ),
                m_Nodes
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class RRB_Node {
        public Fract16Bit DeltaX;
        public Fract16Bit DeltaY;
        public Fract8Bit DeltaZ;
        public Fract8Bit RotX;
        public Fract8Bit RotY;
        public Fract8Bit RotZ;
        public Fract8Bit RotW;
        public Fract8Bit Fract1;
        public Fract8Bit Fract2;
        public byte UnknownTiming;

        public static RRB_Node FromStream(Stream stream) {
            RRB_Node val = new RRB_Node();
            val.DeltaX = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.DeltaY = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.DeltaZ = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.RotX = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.RotY = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.RotZ = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.RotW = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.Fract1 = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.Fract2 = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.UnknownTiming = BinaryFileHelper.ReadByteWithHeader(stream);
            return val;
        }

        public void ToStream(Stream stream) {
            BinaryFileHelper.WriteFract16BitWithHeader(stream, this.DeltaX);
            BinaryFileHelper.WriteFract16BitWithHeader(stream, this.DeltaY);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.DeltaZ);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.RotX);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.RotY);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.RotZ);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.RotW);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.Fract1);
            BinaryFileHelper.WriteFract8BitWithHeader(stream, this.Fract2);
            BinaryFileHelper.WriteByteWithHeader(stream, this.UnknownTiming);
        }

        public static void ToStream(Stream stream, RRB_Node entry) {
            entry.ToStream(stream);
        }
    }
}