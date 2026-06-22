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

		public CPB_Checkpoint_Direction Direction;
		public CPB_Checkpoint_Timing Timing;
		public LRVector3 Location;

		public CPB_Checkpoint()
			: this(new CPB_Checkpoint_Direction(), new CPB_Checkpoint_Timing(), new LRVector3()) { }

		public CPB_Checkpoint(CPB_Checkpoint_Direction direction, CPB_Checkpoint_Timing timing, LRVector3 location)
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
						val.Direction = CPB_Checkpoint_Direction.Read(p_reader);
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

	public class CPB_Checkpoint_Direction
	{
		public LRVector3 Normal;
		public float VarD;

		public static CPB_Checkpoint_Direction Read(LRBinaryReader p_reader)
		{
			CPB_Checkpoint_Direction val = new CPB_Checkpoint_Direction();
			val.Normal = LRVector3.Read(p_reader);
			val.VarD = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class CPB_Checkpoint_Timing
	{
		public int
			VarA,
			VarB,
			VarC,
			VarD;

		public static CPB_Checkpoint_Timing Read(LRBinaryReader p_reader)
		{
			CPB_Checkpoint_Timing val = new CPB_Checkpoint_Timing();
			val.VarA = p_reader.ReadIntWithHeader();
			val.VarB = p_reader.ReadIntWithHeader();
			val.VarC = p_reader.ReadIntWithHeader();
			val.VarD = p_reader.ReadIntWithHeader();
			return val;
		}
	}
}
