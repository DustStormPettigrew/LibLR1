using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Trigger box format. Defines trigger/teleport positions in a race track.
	/// </summary>
	public class TGB
	{
		private const byte
			ID_TRIGGERS = 0x27;

		private TGB_Trigger[] m_triggers;

		public TGB_Trigger[] Triggers
		{
			get { return m_triggers; }
			set { m_triggers = value; }
		}

		public TGB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public TGB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TRIGGERS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						m_triggers = new TGB_Trigger[count];
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						for (int i = 0; i < count; i++)
						{
							p_reader.Expect((Token)ID_TRIGGERS);
							m_triggers[i] = p_reader.ReadStruct<TGB_Trigger>(TGB_Trigger.Read);
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
			p_writer.WriteByte(ID_TRIGGERS);
			p_writer.WriteToken(Token.LeftBracket);
			p_writer.WriteIntWithHeader(m_triggers.Length);
			p_writer.WriteToken(Token.RightBracket);
			p_writer.WriteToken(Token.LeftCurly);
			for (int i = 0; i < m_triggers.Length; i++)
			{
				p_writer.WriteByte(ID_TRIGGERS);
				p_writer.WriteStruct<TGB_Trigger>(TGB_Trigger.Write, m_triggers[i]);
			}
			p_writer.WriteToken(Token.RightCurly);
		}
	}

	public class TGB_Trigger
	{
		private const byte
			PROPERTY_POSITION = 0x28,
			PROPERTY_CHECKPOINT_ID = 0x29;

		public LRVector3 Position;
		public bool HasCheckpointId;
		public int CheckpointId;

		public static TGB_Trigger Read(LRBinaryReader p_reader)
		{
			TGB_Trigger val = new TGB_Trigger();
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
					case PROPERTY_CHECKPOINT_ID:
					{
						val.HasCheckpointId = true;
						val.CheckpointId = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, TGB_Trigger p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			LRVector3.Write(p_writer, p_value.Position);

			if (p_value.HasCheckpointId)
			{
				p_writer.WriteByte(PROPERTY_CHECKPOINT_ID);
				p_writer.WriteIntWithHeader(p_value.CheckpointId);
			}
		}
	}
}
