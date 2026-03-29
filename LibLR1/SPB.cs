using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Start Positions format. Defines racer starting positions for a race track.
	/// </summary>
	public class SPB
	{
		private const byte
			ID_START_POSITIONS = 0x27;

		private Dictionary<int, SPB_StartPosition> m_startPositions;

		public Dictionary<int, SPB_StartPosition> StartPositions
		{
			get { return m_startPositions; }
			set { m_startPositions = value; }
		}

		public SPB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public SPB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_START_POSITIONS:
					{
						m_startPositions = new Dictionary<int, SPB_StartPosition>();
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						for (int i = 0; i < count; i++)
						{
							p_reader.Expect((Token)ID_START_POSITIONS);
							int index = p_reader.ReadIntWithHeader();
							SPB_StartPosition pos = p_reader.ReadStruct<SPB_StartPosition>(
								SPB_StartPosition.Read
							);
							m_startPositions[index] = pos;
						}
						p_reader.Expect(Token.RightCurly);
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
			p_writer.WriteByte(ID_START_POSITIONS);
			p_writer.WriteToken(Token.LeftBracket);
			p_writer.WriteIntWithHeader(m_startPositions.Count);
			p_writer.WriteToken(Token.RightBracket);
			p_writer.WriteToken(Token.LeftCurly);
			foreach (var kv in m_startPositions)
			{
				p_writer.WriteByte(ID_START_POSITIONS);
				p_writer.WriteIntWithHeader(kv.Key);
				p_writer.WriteStruct<SPB_StartPosition>(
					SPB_StartPosition.Write,
					kv.Value
				);
			}
			p_writer.WriteToken(Token.RightCurly);
		}
	}

	public class SPB_StartPosition
	{
		private const byte
			PROPERTY_POSITION = 0x28,
			PROPERTY_ORIENTATION = 0x29;

		public LRVector3 Position;
		public float[] Orientation;

		public static SPB_StartPosition Read(LRBinaryReader p_reader)
		{
			SPB_StartPosition val = new SPB_StartPosition();
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
					case PROPERTY_ORIENTATION:
					{
						val.Orientation = new float[6];
						for (int i = 0; i < 6; i++)
						{
							val.Orientation[i] = p_reader.ReadFloatWithHeader();
						}
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

		public static void Write(LRBinaryWriter p_writer, SPB_StartPosition p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			LRVector3.Write(p_writer, p_value.Position);

			p_writer.WriteByte(PROPERTY_ORIENTATION);
			for (int i = 0; i < 6; i++)
			{
				p_writer.WriteFloatWithHeader(p_value.Orientation[i]);
			}
		}
	}
}
