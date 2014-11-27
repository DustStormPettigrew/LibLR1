using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    /// <summary>
    /// Skeleton format.
    /// </summary>
    public class SDB {
        private const byte
            ID_BONE = 0x27;

        private Dictionary<string, SDB_Bone> m_Bones;

        public Dictionary<string, SDB_Bone> Bones { get { return m_Bones; } set { m_Bones = value; } }

        public SDB(Stream stream) {
            m_Bones = new Dictionary<string, SDB_Bone>();
            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_BONE:
                        m_Bones = BinaryFileHelper.ReadDictionaryBlock<SDB_Bone>(
                            stream,
                            new BinaryFileHelper.ReadObject<SDB_Bone>(
                                SDB_Bone.FromStream
                            ),
                            ID_BONE
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public SDB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_BONE);
            BinaryFileHelper.WriteDictionaryBlock<SDB_Bone>(
                stream,
                new BinaryFileHelper.WriteObject<SDB_Bone>(
                    SDB_Bone.ToStream
                ),
                m_Bones,
                ID_BONE
            );
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }
    }

    public class SDB_Bone {
        private const byte
            PROPERTY_POSITION = 0x28,
            PROPERTY_MATRIX = 0x29,
            PROPERTY_PARENT = 0x2A;

        public LRVector3 Position;
        public LRQuaternion Transform;
        public bool HasParent;
        public string ParentBone;

        public SDB_Bone()
            : this(new LRVector3(), new LRQuaternion(), false, "") { }

        public SDB_Bone(LRVector3 position, LRQuaternion transform, bool hasparent, string parentbone) {
            Position = position;
            Transform = transform;
            HasParent = hasparent;
            ParentBone = parentbone;
        }

        public static SDB_Bone FromStream(Stream stream) {
            SDB_Bone val = new SDB_Bone();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION:
                        val.Position = LRVector3.FromStream(stream);
                        break;
                    case PROPERTY_MATRIX:
                        val.Transform = LRQuaternion.FromStream(stream);
                        break;
                    case PROPERTY_PARENT:
                        val.ParentBone = BinaryFileHelper.ReadStringWithHeader(stream);
                        val.HasParent = true;
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, SDB_Bone value) {
            stream.WriteByte(PROPERTY_POSITION);
            LRVector3.ToStream(stream, value.Position);
            stream.WriteByte(PROPERTY_MATRIX);
            LRQuaternion.ToStream(stream, value.Transform);
            if (value.HasParent) {
                stream.WriteByte(PROPERTY_PARENT);
                BinaryFileHelper.WriteStringWithHeader(stream, value.ParentBone);
            }
        }
    }
}