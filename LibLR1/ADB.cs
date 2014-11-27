using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

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

		public ADB(Stream stream)
		{
			m_Data       = new ADB_Data();
			m_Pointers   = new ADB_Pointer[0];
			m_Animations = new Dictionary<string, ADB_Meta>();
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_ANIM_DATA:
					{
						m_Data = BinaryFileHelper.ReadStruct<ADB_Data>(
							stream,
							new BinaryFileHelper.ReadObject<ADB_Data>(
								ADB_Data.FromStream
							)
						);
						break;
					}
					case ID_ANIM_POINTERS:
					{
						m_Pointers = BinaryFileHelper.ReadArrayBlock<ADB_Pointer>(
							stream,
							new BinaryFileHelper.ReadObject<ADB_Pointer>(
								ADB_Pointer.FromStream
							)
						);
						break;
					}
					case ID_ANIM_META:
					{
						m_Animations = BinaryFileHelper.ReadDictionaryBlock<ADB_Meta>(
							stream,
							new BinaryFileHelper.ReadObject<ADB_Meta>(
								ADB_Meta.FromStream
							),
							ID_ANIM_META
						);
						break;
					}
					default:
					{
						throw new UnexpectedBlockException(block_id, stream.Position - 1);
					}
				}
			}
		}
		
		public ADB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
		}
		
		public void Save(Stream stream)
		{
			stream.WriteByte(ID_ANIM_DATA);
			BinaryFileHelper.WriteStruct<ADB_Data>(
				stream,
				new BinaryFileHelper.WriteObject<ADB_Data>(
					ADB_Data.ToStream
				),
				m_Data
			);
			
			stream.WriteByte(ID_ANIM_POINTERS);
			BinaryFileHelper.WriteArrayBlock<ADB_Pointer>(
				stream,
				new BinaryFileHelper.WriteObject<ADB_Pointer>(
					ADB_Pointer.ToStream
				),
				m_Pointers
			);
			
			stream.WriteByte(ID_ANIM_META);
			BinaryFileHelper.WriteDictionaryBlock<ADB_Meta>(
				stream,
				new BinaryFileHelper.WriteObject<ADB_Meta>(
					ADB_Meta.ToStream
				),
				m_Animations,
				ID_ANIM_META
			);
		}
		
		public void Save(string path)
		{
			using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
			{
				Save(fsOut);
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
		
		public static ADB_Data FromStream(Stream stream)
		{
			ADB_Data val = new ADB_Data();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_DATA_XYZ_OFFSETS:
					{
						val.PositionOffsets = BinaryFileHelper.ReadVector3fArrayBlock(stream);
						break;
					}
					case PROPERTY_DATA_TRANSFORMS:
					{
						val.Transforms = BinaryFileHelper.ReadQuaternionArrayBlock(stream);
						break;
					}
					case PROPERTY_DATA_TIME_OFFSETS:
					{
						val.TimeOffsets = BinaryFileHelper.ReadIntArrayBlock(stream);
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(property_id, stream.Position - 1);
					}
				}
			}
			return val;
		}
		
		public static void ToStream(Stream stream, ADB_Data value)
		{
			stream.WriteByte(PROPERTY_DATA_XYZ_OFFSETS);
			BinaryFileHelper.WriteVector3fArrayBlock(stream, value.PositionOffsets);
			
			stream.WriteByte(PROPERTY_DATA_TRANSFORMS);
			BinaryFileHelper.WriteQuaternionArrayBlock(stream, value.Transforms);
			
			stream.WriteByte(PROPERTY_DATA_TIME_OFFSETS);
			BinaryFileHelper.WriteIntArrayBlock(stream, value.TimeOffsets);
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
		
		public static ADB_Pointer FromStream(Stream stream)
		{
			ADB_Pointer val = new ADB_Pointer();
			val.TransformOffset     = BinaryFileHelper.ReadIntWithHeader(stream);
			val.TransformTimeOffset = BinaryFileHelper.ReadIntWithHeader(stream);
			val.TransformLength     = BinaryFileHelper.ReadIntWithHeader(stream);
			val.PositionOffset      = BinaryFileHelper.ReadIntWithHeader(stream);
			val.PositionTimeOffset  = BinaryFileHelper.ReadIntWithHeader(stream);
			val.PositionLength      = BinaryFileHelper.ReadIntWithHeader(stream);
			return val;
		}
		
		public static void ToStream(Stream stream, ADB_Pointer value)
		{
			BinaryFileHelper.WriteIntWithHeader(stream, value.TransformOffset);
			BinaryFileHelper.WriteIntWithHeader(stream, value.TransformTimeOffset);
			BinaryFileHelper.WriteIntWithHeader(stream, value.TransformLength);
			BinaryFileHelper.WriteIntWithHeader(stream, value.PositionOffset);
			BinaryFileHelper.WriteIntWithHeader(stream, value.PositionTimeOffset);
			BinaryFileHelper.WriteIntWithHeader(stream, value.PositionLength);
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
		
		public static ADB_Meta FromStream(Stream stream)
		{
			ADB_Meta val = new ADB_Meta();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_META_LENGTH:
					{
						val.Length = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_META_LENGTH_1:
					{
						val.Length1 = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_META_SPEED:
					{
						val.Speed = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_META_XYZ_OFFSET:
					{
						val.InitialPosition = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_META_TRANSFORM:
					{
						val.InitialQuaternion = LRQuaternion.FromStream(stream);
						break;
					}
					case PROPERTY_META_POINTERS_OFFSET:
					{
						val.PointerTableOffset = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
				}
			}
			return val;
		}
		
		public static void ToStream(Stream stream, ADB_Meta value)
		{
			stream.WriteByte(PROPERTY_META_LENGTH);
			BinaryFileHelper.WriteIntWithHeader(stream, value.Length);
			
			stream.WriteByte(PROPERTY_META_LENGTH_1);
			BinaryFileHelper.WriteIntWithHeader(stream, value.Length1);
			
			stream.WriteByte(PROPERTY_META_SPEED);
			BinaryFileHelper.WriteIntWithHeader(stream, value.Speed);
			
			stream.WriteByte(PROPERTY_META_XYZ_OFFSET);
			LRVector3.ToStream(stream, value.InitialPosition);
			
			stream.WriteByte(PROPERTY_META_TRANSFORM);
			LRQuaternion.ToStream(stream, value.InitialQuaternion);
			
			stream.WriteByte(PROPERTY_META_POINTERS_OFFSET);
			BinaryFileHelper.WriteIntWithHeader(stream, value.PointerTableOffset);
		}
	}
}