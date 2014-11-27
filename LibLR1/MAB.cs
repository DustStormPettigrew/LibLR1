using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class MAB {
        private const byte
            ID_MATERIALFRAMES = 0x27,
            ID_ANIMATIONS = 0x28;

        private MAB_MaterialFrame[] m_MaterialFrames;
        private MAB_Animation[] m_Animations;

        public MAB_MaterialFrame[] MaterialFrames { get { return m_MaterialFrames; } set { m_MaterialFrames = value; } }
        public MAB_Animation[] Animations { get { return m_Animations; } set { m_Animations = value; } }

        public MAB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_MATERIALFRAMES:
                        m_MaterialFrames = BinaryFileHelper.ReadArrayBlock<MAB_MaterialFrame>(
                            stream,
                            new BinaryFileHelper.ReadObject<MAB_MaterialFrame>(
                                MAB_MaterialFrame.FromStream
                            )
                        );
                        break;
                    case ID_ANIMATIONS:
                        m_Animations = BinaryFileHelper.ReadStructArrayBlock<MAB_Animation>(
                            stream,
                            new BinaryFileHelper.ReadObject<MAB_Animation>(
                                MAB_Animation.FromStream
                            ),
                            ID_ANIMATIONS
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public MAB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_MATERIALFRAMES);
            BinaryFileHelper.WriteArrayBlock<MAB_MaterialFrame>(
                stream,
                new BinaryFileHelper.WriteObject<MAB_MaterialFrame>(
                    MAB_MaterialFrame.ToStream
                ),
                m_MaterialFrames
            );
            stream.WriteByte(ID_ANIMATIONS);
            BinaryFileHelper.WriteStructArrayBlock<MAB_Animation>(
                stream,
                new BinaryFileHelper.WriteObject<MAB_Animation>(
                    MAB_Animation.ToStream
                ),
                m_Animations,
                ID_ANIMATIONS
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class MAB_MaterialFrame {
        public string Material;
        public int Frame;

        public static MAB_MaterialFrame FromStream(Stream stream) {
            MAB_MaterialFrame val = new MAB_MaterialFrame();
            val.Material = BinaryFileHelper.ReadStringWithHeader(stream);
            val.Frame = BinaryFileHelper.ReadIntWithHeader(stream);
            return val;
        }

        public static void ToStream(Stream stream, MAB_MaterialFrame value) {
            BinaryFileHelper.WriteStringWithHeader(stream, value.Material);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Frame);
        }
    }

    public class MAB_Animation {
        private const byte
            PROPERTY_MATERIALFRAMES = 0x27,
            PROPERTY_NUM_FRAMES = 0x29,
            PROPERTY_SPEED = 0x2A;

        public int AnimationOffset;
        public int AnimationLength;
        public int Frames;
        public int Speed;

        public static MAB_Animation FromStream(Stream stream) {
            MAB_Animation val = new MAB_Animation();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_MATERIALFRAMES:
                        val.AnimationOffset = BinaryFileHelper.ReadIntWithHeader(stream);
                        val.AnimationLength = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_NUM_FRAMES:
                        val.Frames = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case PROPERTY_SPEED:
                        val.Speed = BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MAB_Animation value) {
            stream.WriteByte(PROPERTY_MATERIALFRAMES);
            BinaryFileHelper.WriteIntWithHeader(stream, value.AnimationOffset);
            BinaryFileHelper.WriteIntWithHeader(stream, value.AnimationLength);
            stream.WriteByte(PROPERTY_NUM_FRAMES);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Frames);
            stream.WriteByte(PROPERTY_SPEED);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Speed);
        }
    }
}