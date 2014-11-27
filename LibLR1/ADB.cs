using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;
using LibLR1.IO;
using System;

namespace LibLR1
{
	/// <summary>
	/// Animation format.
	/// </summary>
	public class ADB
	{
		private const byte
			ID_ANIM_DATA     = 0x27,
			ID_ANIM_POINTERS = 0x2B,
			ID_ANIM_META     = 0x2C;
		
		private ADB_Data                     m_Data;
		private ADB_Pointer[]                m_Pointers;
		private Dictionary<string, ADB_Meta> m_Animations;

		public ADB_Data Data
		{
			get { return m_Data; }
			set { m_Data = value; }
		}
		public ADB_Pointer[] Pointers
		{
			get { return m_Pointers; }
			set { m_Pointers = value; }
		}
		public Dictionary<string, ADB_Meta> Animations
		{
			get { return m_Animations; }
			set { m_Animations = value; }
		}

		public ADB(string p_path, bool p_decompress = true)
			: this(new LRBinaryReader(p_decompress ? BinaryFileHelper.Decompress(p_path) : (Stream)File.OpenRead(p_path)))
		{
		}

		public ADB(LRBinaryReader p_reader)
		{
			m_Data       = new ADB_Data();
			m_Pointers   = new ADB_Pointer[0];
			m_Animations = new Dictionary<string, ADB_Meta>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte block_id = p_reader.ReadByte();//BinaryFileHelper.ReadByte(p_reader);
				switch (block_id)
				{
					case ID_ANIM_DATA:
					{
						m_Data = p_reader.ReadStruct<ADB_Data>(
							new LRBinaryReader.ReadObject<ADB_Data>(
								ADB_Data.Read
							)
						);
						break;
					}
					case ID_ANIM_POINTERS:
					{
						m_Pointers = p_reader.ReadArrayBlock<ADB_Pointer>(
							new LRBinaryReader.ReadObject<ADB_Pointer>(
								ADB_Pointer.Read
							)
						);
						break;
					}
					case ID_ANIM_META:
					{
						m_Animations = p_reader.ReadDictionaryBlock<ADB_Meta>(
							new LRBinaryReader.ReadObject<ADB_Meta>(
								ADB_Meta.Read
							),
							ID_ANIM_META
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(block_id, p_reader.BaseStream.Position - 1);
					}
				}
			}
		}

		public void Save(LRBinaryWriter p_writer)
		{
			p_writer.WriteByte(ID_ANIM_DATA);
			p_writer.WriteStruct<ADB_Data>(
				new LRBinaryWriter.WriteObject<ADB_Data>(
					ADB_Data.Write
				),
				m_Data
			);

			p_writer.WriteByte(ID_ANIM_POINTERS);
			p_writer.WriteArrayBlock<ADB_Pointer>(
				new LRBinaryWriter.WriteObject<ADB_Pointer>(
					ADB_Pointer.Write
				),
				m_Pointers
			);

			p_writer.WriteByte(ID_ANIM_META);
			p_writer.WriteDictionaryBlock<ADB_Meta>(
				new LRBinaryWriter.WriteObject<ADB_Meta>(
					ADB_Meta.Write
				),
				m_Animations,
				ID_ANIM_META
			);
		}
		
		public void Save(string p_filepath)
		{
			using (LRBinaryWriter writer = new LRBinaryWriter(File.OpenWrite(p_filepath)))
			{
				Save(writer);
			}
		}
	}
	
	public class ADB_Data
	{
		private const byte
			PROPERTY_DATA_XYZ_OFFSETS  = 0x28,
			PROPERTY_DATA_TRANSFORMS   = 0x29,
			PROPERTY_DATA_TIME_OFFSETS = 0x2A;
		
		public LRVector3[]    PositionOffsets;
		public LRQuaternion[] Transforms;
		public int[]          TimeOffsets;
		
		public ADB_Data()
			: this(new LRVector3[0], new LRQuaternion[0], new int[0])
		{
		}
		
		public ADB_Data(LRVector3[] positionoffsets, LRQuaternion[] transforms, int[] timeoffsets)
		{
			PositionOffsets = positionoffsets;
			Transforms      = transforms;
			TimeOffsets     = timeoffsets;
		}
		
		public static ADB_Data Read(LRBinaryReader p_reader)
		{
			ADB_Data val = new ADB_Data();
			while (p_reader.Next(BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = p_reader.ReadByte();
				switch (property_id)
				{
					case PROPERTY_DATA_XYZ_OFFSETS:
					{
						val.PositionOffsets = p_reader.ReadVector3fArrayBlock();
						break;
					}
					case PROPERTY_DATA_TRANSFORMS:
					{
						val.Transforms = p_reader.ReadQuaternionArrayBlock();
						break;
					}
					case PROPERTY_DATA_TIME_OFFSETS:
					{
						val.TimeOffsets = p_reader.ReadIntArrayBlock();
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(property_id, p_reader.BaseStream.Position - 1);
					}
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, ADB_Data p_value)
		{
			p_writer.WriteByte(PROPERTY_DATA_XYZ_OFFSETS);
			p_writer.WriteVector3fArrayBlock(p_value.PositionOffsets);
			
			p_writer.WriteByte(PROPERTY_DATA_TRANSFORMS);
			p_writer.WriteQuaternionArrayBlock(p_value.Transforms);
			
			p_writer.WriteByte(PROPERTY_DATA_TIME_OFFSETS);
			p_writer.WriteIntArrayBlock(p_value.TimeOffsets);
		}
	}
	
	public class ADB_Pointer
	{
		public int TransformOffset;
		public int TransformTimeOffset;
		public int TransformLength;
		public int PositionOffset;
		public int PositionTimeOffset;
		public int PositionLength;
		
		public ADB_Pointer()
			: this(0, 0, 0, 0, 0, 0)
		{
		}
		
		public ADB_Pointer(int transformoffset, int transformtimeoffset, int transformlength, int positionoffset, int positiontimeoffset, int positionlength)
		{
			TransformOffset     = transformoffset;
			TransformTimeOffset = transformtimeoffset;
			TransformLength     = transformlength;
			PositionOffset      = positionoffset;
			PositionTimeOffset  = positiontimeoffset;
			PositionLength      = positionlength;
		}
		
		public static ADB_Pointer Read(LRBinaryReader p_reader)
		{
			ADB_Pointer val = new ADB_Pointer();
			val.TransformOffset     = p_reader.ReadIntWithHeader();
			val.TransformTimeOffset = p_reader.ReadIntWithHeader();
			val.TransformLength     = p_reader.ReadIntWithHeader();
			val.PositionOffset      = p_reader.ReadIntWithHeader();
			val.PositionTimeOffset  = p_reader.ReadIntWithHeader();
			val.PositionLength      = p_reader.ReadIntWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, ADB_Pointer p_value)
		{
			p_writer.WriteIntWithHeader(p_value.TransformOffset);
			p_writer.WriteIntWithHeader(p_value.TransformTimeOffset);
			p_writer.WriteIntWithHeader(p_value.TransformLength);
			p_writer.WriteIntWithHeader(p_value.PositionOffset);
			p_writer.WriteIntWithHeader(p_value.PositionTimeOffset);
			p_writer.WriteIntWithHeader(p_value.PositionLength);
		}
	}
	
	public class ADB_Meta
	{
		private const byte
			PROPERTY_META_POINTERS_OFFSET = 0x2B,
			PROPERTY_META_LENGTH          = 0x2D,
			PROPERTY_META_LENGTH_1        = 0x2E,
			PROPERTY_META_SPEED           = 0x2F,
			PROPERTY_META_XYZ_OFFSET      = 0x30,
			PROPERTY_META_TRANSFORM       = 0x31;
		
		public int          PointerTableOffset;
		public int          Length;
		public int          Length1;
		public int          Speed;
		public LRVector3    InitialPosition;
		public LRQuaternion InitialQuaternion;
		
		public ADB_Meta()
			: this(0, 0, 0, 0, new LRVector3(), new LRQuaternion())
		{
		}
		
		public ADB_Meta(int pointertableoffset, int length, int length1, int speed, LRVector3 initialposition, LRQuaternion initialtransform)
		{
			PointerTableOffset = pointertableoffset;
			Length             = length;
			Length1            = length1;
			Speed              = speed;
			InitialPosition    = initialposition;
			InitialQuaternion  = initialtransform;
		}
		
		public static ADB_Meta Read(LRBinaryReader p_reader)
		{
			ADB_Meta val = new ADB_Meta();
			while (p_reader.Next(Token.RIGHT_CURLY) == false)
			{
				byte property_id = p_reader.ReadByte();
				switch (property_id)
				{
					case PROPERTY_META_LENGTH:
					{
						val.Length = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_META_LENGTH_1:
					{
						val.Length1 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_META_SPEED:
					{
						val.Speed = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_META_XYZ_OFFSET:
					{
						val.InitialPosition = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_META_TRANSFORM:
					{
						val.InitialQuaternion = LRQuaternion.Read(p_reader);
						break;
					}
					case PROPERTY_META_POINTERS_OFFSET:
					{
						val.PointerTableOffset = p_reader.ReadIntWithHeader();
						break;
					}
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, ADB_Meta p_value)
		{
			p_writer.WriteByte(PROPERTY_META_LENGTH);
			p_writer.WriteIntWithHeader(p_value.Length);

			p_writer.WriteByte(PROPERTY_META_LENGTH_1);
			p_writer.WriteIntWithHeader(p_value.Length1);

			p_writer.WriteByte(PROPERTY_META_SPEED);
			p_writer.WriteIntWithHeader(p_value.Speed);

			p_writer.WriteByte(PROPERTY_META_XYZ_OFFSET);
			LRVector3.Write(p_writer, p_value.InitialPosition);

			p_writer.WriteByte(PROPERTY_META_TRANSFORM);
			LRQuaternion.Write(p_writer, p_value.InitialQuaternion);

			p_writer.WriteByte(PROPERTY_META_POINTERS_OFFSET);
			p_writer.WriteIntWithHeader(p_value.PointerTableOffset);
		}
	}
}