using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class PWB {
        public const byte
            COLOR_RED = 0x2A,
            COLOR_YELLOW = 0x2B,
            COLOR_BLUE = 0x2C,
            COLOR_GREEN = 0x2D;

        private const byte
            ID_COLOR_BRICKS = 0x27,
            ID_WHITE_BRICKS = 0x2F;

        private List<PWB_ColorBrick> m_ColorBricks;
        private List<PWB_WhiteBrick> m_WhiteBricks;

        public List<PWB_ColorBrick> ColorBricks {
            get { return m_ColorBricks; }
            set { m_ColorBricks = value; }
        }

        public List<PWB_WhiteBrick> WhiteBricks {
            get { return m_WhiteBricks; }
            set { m_WhiteBricks = value; }
        }

        public PWB() {
            m_ColorBricks = new List<PWB_ColorBrick>();
            m_WhiteBricks = new List<PWB_WhiteBrick>();
        }

        public PWB(Stream stream) {
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_COLOR_BRICKS: {
                            m_ColorBricks = BinaryFileHelper.ReadStructListBlock<PWB_ColorBrick>(
                                stream,
                                new BinaryFileHelper.ReadObject<PWB_ColorBrick>(
                                    PWB_ColorBrick.FromStream
                                ),
                                ID_COLOR_BRICKS
                            );
                        } break;
                    case ID_WHITE_BRICKS: {
                            m_WhiteBricks = BinaryFileHelper.ReadStructListBlock<PWB_WhiteBrick>(
                                stream,
                                new BinaryFileHelper.ReadObject<PWB_WhiteBrick>(
                                    PWB_WhiteBrick.FromStream
                                ),
                                ID_WHITE_BRICKS
                            );
                        } break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public PWB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_COLOR_BRICKS);
            BinaryFileHelper.WriteStructListBlock<PWB_ColorBrick>(
                stream,
                new BinaryFileHelper.WriteObject<PWB_ColorBrick>(
                    PWB_ColorBrick.ToStream
                ),
                m_ColorBricks,
                ID_COLOR_BRICKS
            );
            stream.WriteByte(ID_WHITE_BRICKS);
            BinaryFileHelper.WriteStructListBlock<PWB_WhiteBrick>(
                stream,
                new BinaryFileHelper.WriteObject<PWB_WhiteBrick>(
                    PWB_WhiteBrick.ToStream
                ),
                m_WhiteBricks,
                ID_WHITE_BRICKS
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class PWB_ColorBrick {
        private const byte
            PROPERTY_POSITION = 0x28,
            PROPERTY_COLOR_RED = 0x2A,
            PROPERTY_COLOR_YELLOW = 0x2B,
            PROPERTY_COLOR_BLUE = 0x2C,
            PROPERTY_COLOR_GREEN = 0x2D;

        public LRVector3 Position;
        public byte Color;

        public PWB_ColorBrick()
            : this(new LRVector3(), PROPERTY_COLOR_RED) { }

        public PWB_ColorBrick(LRVector3 position, byte color) {
            Position = position;
            Color = color;
        }

        public static PWB_ColorBrick FromStream(Stream stream) {
            PWB_ColorBrick val = new PWB_ColorBrick();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = LRVector3.FromStream(stream);
                        } break;
                    case PROPERTY_COLOR_RED:
                    case PROPERTY_COLOR_YELLOW:
                    case PROPERTY_COLOR_BLUE:
                    case PROPERTY_COLOR_GREEN: {
                            val.Color = property_id;
                        } break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, PWB_ColorBrick value) {
            stream.WriteByte(PROPERTY_POSITION);
            LRVector3.ToStream(stream, value.Position);
            stream.WriteByte(value.Color);
        }
    }

    public class PWB_WhiteBrick {
        private const byte
            PROPERTY_POSITION = 0x28;

        public LRVector3 Position;

        public PWB_WhiteBrick()
            : this(new LRVector3()) { }

        public PWB_WhiteBrick(LRVector3 position) {
            Position = position;
        }

        public static PWB_WhiteBrick FromStream(Stream stream) {
            PWB_WhiteBrick val = new PWB_WhiteBrick();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = LRVector3.FromStream(stream);
                        } break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, PWB_WhiteBrick value) {
            stream.WriteByte(PROPERTY_POSITION);
            LRVector3.ToStream(stream, value.Position);
        }
    }
}