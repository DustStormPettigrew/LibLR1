using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	/// <summary>
	/// Checkpoint layout format
	/// </summary>
	public class CPB
	{
		private const byte
			ID_CHECKPOINT = 0x27,
			PROPERTY_DIRECTION = 0x28,
			PROPERTY_TIMING = 0x29,
			PROPERTY_LOCATION = 0x2A;

		private CPB_Checkpoint[] m_checkpoints;

		public CPB_Checkpoint[] Checkpoints
		{
			get { return m_checkpoints; }
			set { m_checkpoints = value; }
		}
		
		public CPB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CPB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_CHECKPOINT:
					{
						m_checkpoints = p_reader.ReadStructArrayBlock<CPB_Checkpoint>(
							CPB_Checkpoint.Read,
							ID_CHECKPOINT
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
	}

	public class CPB_Checkpoint
	{
		private const byte
			PROPERTY_DIRECTION = 0x28,
			PROPERTY_TIMING = 0x29,
			PROPERTY_LOCATION = 0x2A;

		public CPB_Checkpoint_Normal Direction;
		public CPB_Checkpoint_Timing Timing;
		public LRVector3 Location;

		public CPB_Checkpoint()
			: this(new CPB_Checkpoint_Normal(), new CPB_Checkpoint_Timing(), new LRVector3()) { }

		public CPB_Checkpoint(CPB_Checkpoint_Normal direction, CPB_Checkpoint_Timing timing, LRVector3 location)
		{
			Direction = direction;
			Timing = timing;
			Location = location;
		}

		public static CPB_Checkpoint Read(LRBinaryReader p_reader)
		{
			CPB_Checkpoint val = new CPB_Checkpoint();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_DIRECTION:
					{
						val.Direction = CPB_Checkpoint_Normal.Read(p_reader);
						break;
					}
					case PROPERTY_TIMING:
					{
						val.Timing = CPB_Checkpoint_Timing.Read(p_reader);
						break;
					}
					case PROPERTY_LOCATION:
					{
						val.Location = LRVector3.Read(p_reader);
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
	}

	public class CPB_Checkpoint_Normal
	{
		public LRVector3 Normal;
		public float Unknown;

		public static CPB_Checkpoint_Normal Read(LRBinaryReader p_reader)
		{
			CPB_Checkpoint_Normal val = new CPB_Checkpoint_Normal();
			val.Normal = LRVector3.Read(p_reader);
			val.Unknown = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class CPB_Checkpoint_Timing
	{
		public int
			Unknown1,
			Unknown2,
			Unknown3,
			Unknown4;

		public static CPB_Checkpoint_Timing Read(LRBinaryReader p_reader)
		{
			CPB_Checkpoint_Timing val = new CPB_Checkpoint_Timing();
			val.Unknown1 = p_reader.ReadIntWithHeader();
			val.Unknown2 = p_reader.ReadIntWithHeader();
			val.Unknown3 = p_reader.ReadIntWithHeader();
			val.Unknown4 = p_reader.ReadIntWithHeader();
			return val;
		}
	}
}