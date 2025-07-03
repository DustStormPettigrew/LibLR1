using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Skeleton format.
	/// </summary>
	public class SDB
	{
		private const byte
			ID_BONE = 0x27;

		private Dictionary<string, SDB_Bone> m_bones;

		public Dictionary<string, SDB_Bone> Bones
		{
			get { return m_bones; }
			set { m_bones = value; }
		}

		public SDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public SDB(LRBinaryReader p_reader)
		{
			m_bones = new Dictionary<string, SDB_Bone>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_BONE:
					{
						m_bones = p_reader.ReadDictionaryBlock<SDB_Bone>(
							SDB_Bone.Read,
							ID_BONE
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(
							blockId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
		}
		
		public void Save(string p_filepath)
		{
			using (LRBinaryWriter writer = new LRBinaryWriter(File.OpenWrite(p_filepath)))
			{
				Save(writer);
			}
		}

		public void Save(LRBinaryWriter p_writer)
		{
			p_writer.WriteByte(ID_BONE);
			p_writer.WriteDictionaryBlock<SDB_Bone>(
				SDB_Bone.Write,
				m_bones,
				ID_BONE
			);
		}
	}

	public class SDB_Bone
	{
		private const byte
			PROPERTY_POSITION = 0x28,
			PROPERTY_MATRIX   = 0x29,
			PROPERTY_PARENT   = 0x2A;

		public LRVector3    Position;
		public LRQuaternion Transform;
		public bool         HasParent;
		public string       ParentBone;

		public SDB_Bone()
			: this(new LRVector3(), new LRQuaternion(), false, "") { }

		public SDB_Bone(LRVector3 p_position, LRQuaternion p_transform, bool p_hasparent, string p_parentbone)
		{
			Position   = p_position;
			Transform  = p_transform;
			HasParent  = p_hasparent;
			ParentBone = p_parentbone;
		}

		public static SDB_Bone Read(LRBinaryReader p_reader)
		{
			SDB_Bone val = new SDB_Bone();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_MATRIX:
					{
						val.Transform = LRQuaternion.Read(p_reader);
						break;
					}
					case PROPERTY_PARENT:
					{
						val.ParentBone = p_reader.ReadStringWithHeader();
						val.HasParent = true;
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(
							propertyId,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, SDB_Bone p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			LRVector3.Write(p_writer, p_value.Position);
			p_writer.WriteByte(PROPERTY_MATRIX);
			LRQuaternion.Write(p_writer, p_value.Transform);
			if (p_value.HasParent)
			{
				p_writer.WriteByte(PROPERTY_PARENT);
				p_writer.WriteStringWithHeader(p_value.ParentBone);
			}
		}
	}
}