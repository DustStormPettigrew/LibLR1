using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class GCB {
        private const byte
            ID_VERTICES = 0x28,
            ID_MODELS = 0x2C;

        private GCB_Vertex[] m_Vertices;
        private Dictionary<string, GCB_ModelSet> m_ModelSets;

        public GCB_Vertex[] Vertices { get { return m_Vertices; } set { m_Vertices = value; } }
        public Dictionary<string, GCB_ModelSet> Models { get { return m_ModelSets; } set { m_ModelSets = value; } }

        public GCB(Stream stream) {
            m_Vertices = new GCB_Vertex[0];
            m_ModelSets = new Dictionary<string, GCB_ModelSet>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_VERTICES:
                        m_Vertices = BinaryFileHelper.ReadArrayBlock<GCB_Vertex>(
                            stream,
                            new BinaryFileHelper.ReadObject<GCB_Vertex>(
                                GCB_Vertex.FromStream
                            )
                        );
                        break;
                    case ID_MODELS:
                        m_ModelSets = BinaryFileHelper.ReadDictionaryBlock<GCB_ModelSet>(
                            stream,
                            new BinaryFileHelper.ReadObject<GCB_ModelSet>(
                                GCB_ModelSet.FromStream
                            ),
                            ID_MODELS
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public GCB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }
    }

    public class GCB_Vertex {
        public Fract16Bit a, b, c, d, e;
        public Fract8Bit f, g, h;

        public static GCB_Vertex FromStream(Stream stream) {
            GCB_Vertex val = new GCB_Vertex();
            val.a = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.b = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.c = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.d = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.e = BinaryFileHelper.ReadFract16BitWithHeader(stream);
            val.f = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.g = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            val.h = BinaryFileHelper.ReadFract8BitWithHeader(stream);
            return val;
        }
    }

    public class GCB_ModelSet {
        private const byte
            PROPERTY_MODELS = 0x2B,
            PROPERTY_UNKNOWN_2D = 0x2D;

        public GCB_Model[] Models;
        public float Unknown2D;

        public static GCB_ModelSet FromStream(Stream stream) {
            GCB_ModelSet val = new GCB_ModelSet();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_MODELS:
                        val.Models = BinaryFileHelper.ReadStructArrayBlock<GCB_Model>(
                            stream,
                            new BinaryFileHelper.ReadObject<GCB_Model>(
                                GCB_Model.FromStream
                            ),
                            PROPERTY_MODELS
                        );
                        break;
                    case PROPERTY_UNKNOWN_2D:
                        val.Unknown2D = BinaryFileHelper.ReadFloatWithHeader(stream);
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
    }

    public class GCB_Model {
        private const byte
            PROPERTY_NAME = 0x27,
            PROPERTY_POLYS = 0x2A;

        public string Name;
        public GCB_Polygon[] Polys;

        public static GCB_Model FromStream(Stream stream) {
            GCB_Model val = new GCB_Model();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_NAME:
                        val.Name = BinaryFileHelper.ReadStringWithHeader(stream);
                        break;
                    case PROPERTY_POLYS:
                        val.Polys = BinaryFileHelper.ReadArrayBlock<GCB_Polygon>(
                            stream,
                            new BinaryFileHelper.ReadObject<GCB_Polygon>(
                                GCB_Polygon.FromStream
                            )
                        );
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }
    }

    public class GCB_Polygon {
        public int V0, V1, V2;

        public static GCB_Polygon FromStream(Stream stream) {
            GCB_Polygon val = new GCB_Polygon();
            val.V0 = BinaryFileHelper.ReadIntegralWithHeader(stream);
            val.V1 = BinaryFileHelper.ReadIntegralWithHeader(stream);
            val.V2 = BinaryFileHelper.ReadIntegralWithHeader(stream);
            return val;
        }
    }
}