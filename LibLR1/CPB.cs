using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Checkpoint layout format
    /// </summary>
    public class CPB {
        private const byte
            ID_CHECKPOINT = 0x27,
            PROPERTY_DIRECTION = 0x28,
            PROPERTY_TIMING = 0x29,
            PROPERTY_LOCATION = 0x2A;

        private CPB_Checkpoint[] m_Checkpoints;

        public CPB_Checkpoint[] Checkpoints {
            get { return m_Checkpoints; }
            set { m_Checkpoints = value; }
        }

        public CPB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_CHECKPOINT:
                        m_Checkpoints = BinaryFileHelper.ReadStructArrayBlock<CPB_Checkpoint>(
                            stream,
                            new BinaryFileHelper.ReadObject<CPB_Checkpoint>(
                                CPB_Checkpoint.FromStream
                            ),
                            ID_CHECKPOINT
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public CPB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }
    }

    public class CPB_Checkpoint {
        private const byte
            PROPERTY_DIRECTION = 0x28,
            PROPERTY_TIMING = 0x29,
            PROPERTY_LOCATION = 0x2A;

        public CPB_Checkpoint_Normal Direction;
        public CPB_Checkpoint_Timing Timing;
        public LRVector3 Location;

        public CPB_Checkpoint()
            : this(new CPB_Checkpoint_Normal(), new CPB_Checkpoint_Timing(), new LRVector3()) { }

        public CPB_Checkpoint(CPB_Checkpoint_Normal direction, CPB_Checkpoint_Timing timing, LRVector3 location) {
            Direction = direction;
            Timing = timing;
            Location = location;
        }

        public static CPB_Checkpoint FromStream(Stream stream) {
            CPB_Checkpoint val = new CPB_Checkpoint();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_DIRECTION:
                        val.Direction = CPB_Checkpoint_Normal.FromStream(stream);
                        break;
                    case PROPERTY_TIMING:
                        val.Timing = CPB_Checkpoint_Timing.FromStream(stream);
                        break;
                    case PROPERTY_LOCATION:
                        val.Location = LRVector3.FromStream(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
    }

    public class CPB_Checkpoint_Normal {
        public LRVector3 Normal;
        public float Unknown;

        public static CPB_Checkpoint_Normal FromStream(Stream stream) {
            CPB_Checkpoint_Normal val = new CPB_Checkpoint_Normal();
            val.Normal = LRVector3.FromStream(stream);
            val.Unknown = BinaryFileHelper.ReadFloatWithHeader(stream);
            return val;
        }
    }

    public class CPB_Checkpoint_Timing {
        public int
            Unknown1,
            Unknown2,
            Unknown3,
            Unknown4;

        public static CPB_Checkpoint_Timing FromStream(Stream stream) {
            CPB_Checkpoint_Timing val = new CPB_Checkpoint_Timing();
            val.Unknown1 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.Unknown2 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.Unknown3 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.Unknown4 = BinaryFileHelper.ReadIntWithHeader(stream);
            return val;
        }
    }
}