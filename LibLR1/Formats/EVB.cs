using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Event format. Defines track events including camera positions, triggers, AI behaviors, and scripted sequences.
	/// </summary>
	public class EVB
	{
		private const byte
			ID_EVENTS = 0x2A,
			ID_ANIMATIONS = 0x28,
			ID_MODELS = 0x3D,
			ID_TRIGGERS = 0x4B,
			ID_CHECKPOINTS = 0x4D,
			ID_AI_PATHS = 0x51,
			ID_EFFECTS = 0x42,
			ID_ACTIONS = 0x52,
			ID_SOUND_ACTIONS = 0x53;

		private Dictionary<int, EVB_Event> m_events;
		private Dictionary<int, EVB_Animation> m_animations;
		private Dictionary<int, EVB_Trigger> m_triggers;
		private Dictionary<int, EVB_Checkpoint> m_checkpoints;
		private Dictionary<int, EVB_AIPath> m_aiPaths;
		private Dictionary<int, EVB_Model> m_models;
		private Dictionary<int, EVB_Effect> m_effects;
		private Dictionary<int, EVB_Action> m_actions;
		private Dictionary<int, EVB_SoundAction> m_soundActions;

		public Dictionary<int, EVB_Event> Events
		{
			get { return m_events; }
			set { m_events = value; }
		}

		public Dictionary<int, EVB_Animation> Animations
		{
			get { return m_animations; }
			set { m_animations = value; }
		}

		public Dictionary<int, EVB_Trigger> Triggers
		{
			get { return m_triggers; }
			set { m_triggers = value; }
		}

		public Dictionary<int, EVB_Checkpoint> Checkpoints
		{
			get { return m_checkpoints; }
			set { m_checkpoints = value; }
		}

		public Dictionary<int, EVB_AIPath> AIPaths
		{
			get { return m_aiPaths; }
			set { m_aiPaths = value; }
		}

		public Dictionary<int, EVB_Model> Models
		{
			get { return m_models; }
			set { m_models = value; }
		}

		public Dictionary<int, EVB_Effect> Effects
		{
			get { return m_effects; }
			set { m_effects = value; }
		}

		public Dictionary<int, EVB_Action> Actions
		{
			get { return m_actions; }
			set { m_actions = value; }
		}

		public Dictionary<int, EVB_SoundAction> SoundActions
		{
			get { return m_soundActions; }
			set { m_soundActions = value; }
		}

		public EVB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public EVB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_EVENTS:
					{
						m_events = ReadIntKeyedBlock<EVB_Event>(p_reader, EVB_Event.Read);
						break;
					}
					case ID_ANIMATIONS:
					{
						m_animations = ReadIntKeyedBlock<EVB_Animation>(p_reader, EVB_Animation.Read);
						break;
					}
					case ID_TRIGGERS:
					{
						m_triggers = ReadIntKeyedBlock<EVB_Trigger>(p_reader, EVB_Trigger.Read);
						break;
					}
					case ID_CHECKPOINTS:
					{
						m_checkpoints = ReadIntKeyedBlock<EVB_Checkpoint>(p_reader, EVB_Checkpoint.Read);
						break;
					}
					case ID_AI_PATHS:
					{
						m_aiPaths = ReadIntKeyedBlock<EVB_AIPath>(p_reader, EVB_AIPath.Read);
						break;
					}
					case ID_MODELS:
					{
						m_models = ReadIntKeyedBlock<EVB_Model>(p_reader, EVB_Model.Read);
						break;
					}
					case ID_EFFECTS:
					{
						m_effects = ReadIntKeyedBlock<EVB_Effect>(p_reader, EVB_Effect.Read);
						break;
					}
					case ID_ACTIONS:
					{
						m_actions = ReadIntKeyedBlock<EVB_Action>(p_reader, EVB_Action.Read);
						break;
					}
					case ID_SOUND_ACTIONS:
					{
						m_soundActions = ReadIntKeyedBlock<EVB_SoundAction>(p_reader, EVB_SoundAction.Read);
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

		private static Dictionary<int, T> ReadIntKeyedBlock<T>(
			LRBinaryReader p_reader,
			System.Func<LRBinaryReader, T> p_readFunc)
		{
			Dictionary<int, T> dict = new Dictionary<int, T>();
			p_reader.Expect(Token.LeftBracket);
			int count = p_reader.ReadIntWithHeader();
			p_reader.Expect(Token.RightBracket);
			p_reader.Expect(Token.LeftCurly);
			for (int i = 0; i < count; i++)
			{
				p_reader.Expect((Token)0x27);
				int key = p_reader.ReadIntWithHeader();
				// Some entries have an optional marker byte (e.g. 0x3C) before the struct
				if (!p_reader.Next(Token.LeftCurly))
				{
					p_reader.ReadByte();
				}
				T value = p_reader.ReadStruct<T>(p_readFunc);
				dict[key] = value;
			}
			p_reader.Expect(Token.RightCurly);
			return dict;
		}

		private static void WriteIntKeyedBlock<T>(
			LRBinaryWriter p_writer,
			byte p_blockId,
			Dictionary<int, T> p_dict,
			System.Action<LRBinaryWriter, T> p_writeFunc)
		{
			p_writer.WriteByte(p_blockId);
			p_writer.WriteToken(Token.LeftBracket);
			p_writer.WriteIntWithHeader(p_dict.Count);
			p_writer.WriteToken(Token.RightBracket);
			p_writer.WriteToken(Token.LeftCurly);
			foreach (KeyValuePair<int, T> kvp in p_dict)
			{
				p_writer.WriteByte(0x27);
				p_writer.WriteIntWithHeader(kvp.Key);
				p_writer.WriteToken(Token.LeftCurly);
				p_writeFunc(p_writer, kvp.Value);
				p_writer.WriteToken(Token.RightCurly);
			}
			p_writer.WriteToken(Token.RightCurly);
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
			if (m_events != null)
				WriteIntKeyedBlock(p_writer, ID_EVENTS, m_events, EVB_Event.Write);

			if (m_checkpoints != null)
				WriteIntKeyedBlock(p_writer, ID_CHECKPOINTS, m_checkpoints, EVB_Checkpoint.Write);

			if (m_aiPaths != null)
				WriteIntKeyedBlock(p_writer, ID_AI_PATHS, m_aiPaths, EVB_AIPath.Write);

			if (m_triggers != null)
				WriteIntKeyedBlock(p_writer, ID_TRIGGERS, m_triggers, EVB_Trigger.Write);

			if (m_animations != null)
				WriteIntKeyedBlock(p_writer, ID_ANIMATIONS, m_animations, EVB_Animation.Write);

			if (m_models != null)
				WriteIntKeyedBlock(p_writer, ID_MODELS, m_models, EVB_Model.Write);

			if (m_effects != null)
				WriteIntKeyedBlock(p_writer, ID_EFFECTS, m_effects, EVB_Effect.Write);

			if (m_actions != null)
				WriteIntKeyedBlock(p_writer, ID_ACTIONS, m_actions, EVB_Action.Write);

			if (m_soundActions != null)
				WriteIntKeyedBlock(p_writer, ID_SOUND_ACTIONS, m_soundActions, EVB_SoundAction.Write);
		}
	}

	public class EVB_Event
	{
		private const byte
			PROPERTY_ID = 0x2C,
			PROPERTY_TYPE = 0x2E,
			PROPERTY_SCALE_MIN = 0x2F,
			PROPERTY_SCALE_MAX = 0x30,
			PROPERTY_DISTANCE_MIN = 0x31,
			PROPERTY_DISTANCE_MAX = 0x32,
			PROPERTY_MODEL = 0x33,
			PROPERTY_POSITION = 0x3B,
			PROPERTY_UNKNOWN_2D = 0x2D,
			PROPERTY_UNKNOWN_3F = 0x3F,
			PROPERTY_UNKNOWN_40 = 0x40,
			PROPERTY_UNKNOWN_54 = 0x54;

		public int Id;
		public int Type;
		public float ScaleMin;
		public float ScaleMax;
		public float DistanceMin;
		public float DistanceMax;
		public string Model;
		public bool HasPosition;
		public LRVector3 Position;
		public bool HasUnknown2D;
		public bool HasUnknown3F;
		public bool HasUnknown40;
		public float Unknown40;
		public bool HasUnknown54;
		public int Unknown54;

		public static EVB_Event Read(LRBinaryReader p_reader)
		{
			EVB_Event val = new EVB_Event();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_ID:
						val.Id = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_TYPE:
						val.Type = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_SCALE_MIN:
						val.ScaleMin = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_SCALE_MAX:
						val.ScaleMax = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_DISTANCE_MIN:
						val.DistanceMin = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_DISTANCE_MAX:
						val.DistanceMax = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_POSITION:
						val.HasPosition = true;
						val.Position = LRVector3.Read(p_reader);
						break;
					case PROPERTY_UNKNOWN_2D:
						val.HasUnknown2D = true;
						break;
					case PROPERTY_UNKNOWN_3F:
						val.HasUnknown3F = true;
						break;
					case PROPERTY_UNKNOWN_40:
						val.HasUnknown40 = true;
						val.Unknown40 = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_UNKNOWN_54:
						val.HasUnknown54 = true;
						val.Unknown54 = p_reader.ReadIntWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Event p_value)
		{
			if (p_value.HasPosition)
			{
				p_writer.WriteByte(PROPERTY_POSITION);
				LRVector3.Write(p_writer, p_value.Position);
			}

			p_writer.WriteByte(PROPERTY_ID);
			p_writer.WriteIntWithHeader(p_value.Id);

			p_writer.WriteByte(PROPERTY_TYPE);
			p_writer.WriteIntWithHeader(p_value.Type);

			p_writer.WriteByte(PROPERTY_SCALE_MIN);
			p_writer.WriteFloatWithHeader(p_value.ScaleMin);

			p_writer.WriteByte(PROPERTY_SCALE_MAX);
			p_writer.WriteFloatWithHeader(p_value.ScaleMax);

			p_writer.WriteByte(PROPERTY_DISTANCE_MIN);
			p_writer.WriteFloatWithHeader(p_value.DistanceMin);

			p_writer.WriteByte(PROPERTY_DISTANCE_MAX);
			p_writer.WriteFloatWithHeader(p_value.DistanceMax);

			if (p_value.HasUnknown40)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_40);
				p_writer.WriteFloatWithHeader(p_value.Unknown40);
			}

			if (p_value.HasUnknown2D)
				p_writer.WriteByte(PROPERTY_UNKNOWN_2D);

			if (p_value.HasUnknown3F)
				p_writer.WriteByte(PROPERTY_UNKNOWN_3F);

			if (p_value.Model != null)
			{
				p_writer.WriteByte(PROPERTY_MODEL);
				p_writer.WriteStringWithHeader(p_value.Model);
			}

			if (p_value.HasUnknown54)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_54);
				p_writer.WriteIntWithHeader(p_value.Unknown54);
			}
		}
	}

	public class EVB_Checkpoint
	{
		private const byte
			PROPERTY_START = 0x4E,
			PROPERTY_END = 0x4F,
			PROPERTY_UNKNOWN_50 = 0x50,
			PROPERTY_UNKNOWN_3A = 0x3A;

		public int[] Start;
		public int[] End;
		public bool HasUnknown50;
		public bool HasUnknown3A;

		public static EVB_Checkpoint Read(LRBinaryReader p_reader)
		{
			EVB_Checkpoint val = new EVB_Checkpoint();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_START:
					{
						val.Start = new int[4];
						for (int i = 0; i < 4; i++)
							val.Start[i] = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_END:
					{
						val.End = new int[4];
						for (int i = 0; i < 4; i++)
							val.End[i] = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_50:
						val.HasUnknown50 = true;
						break;
					case PROPERTY_UNKNOWN_3A:
						val.HasUnknown3A = true;
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Checkpoint p_value)
		{
			if (p_value.HasUnknown50)
				p_writer.WriteByte(0x50);

			if (p_value.Start != null)
			{
				p_writer.WriteByte(0x4E);
				for (int i = 0; i < p_value.Start.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Start[i]);
			}

			if (p_value.End != null)
			{
				p_writer.WriteByte(0x4F);
				for (int i = 0; i < p_value.End.Length; i++)
					p_writer.WriteIntWithHeader(p_value.End[i]);
			}

			if (p_value.HasUnknown3A)
				p_writer.WriteByte(0x3A);
		}
	}

	public class EVB_AIPath
	{
		private const byte
			PROPERTY_UNKNOWN_36 = 0x36,
			PROPERTY_UNKNOWN_37 = 0x37;

		public bool HasUnknown36;
		public bool HasUnknown37;

		public static EVB_AIPath Read(LRBinaryReader p_reader)
		{
			EVB_AIPath val = new EVB_AIPath();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_UNKNOWN_36:
						val.HasUnknown36 = true;
						break;
					case PROPERTY_UNKNOWN_37:
						val.HasUnknown37 = true;
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_AIPath p_value)
		{
			if (p_value.HasUnknown36)
				p_writer.WriteByte(0x36);

			if (p_value.HasUnknown37)
				p_writer.WriteByte(0x37);
		}
	}

	public class EVB_Trigger
	{
		private const byte
			PROPERTY_TYPE = 0x4C,
			PROPERTY_DURATION = 0x49,
			PROPERTY_UNKNOWN_27 = 0x27,
			PROPERTY_TARGET = 0x34;

		public int Type;
		public int Duration;
		public bool HasUnknown27;
		public int Target;

		public static EVB_Trigger Read(LRBinaryReader p_reader)
		{
			EVB_Trigger val = new EVB_Trigger();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_TYPE:
						val.Type = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_DURATION:
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_UNKNOWN_27:
						val.HasUnknown27 = true;
						break;
					case PROPERTY_TARGET:
						val.Target = p_reader.ReadIntWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Trigger p_value)
		{
			p_writer.WriteByte(0x4C);
			p_writer.WriteIntWithHeader(p_value.Type);

			p_writer.WriteByte(0x49);
			p_writer.WriteIntWithHeader(p_value.Duration);

			if (p_value.HasUnknown27)
				p_writer.WriteByte(0x27);

			p_writer.WriteByte(0x34);
			p_writer.WriteIntWithHeader(p_value.Target);
		}
	}

	public class EVB_Animation
	{
		private const byte
			PROPERTY_MODEL = 0x33,
			PROPERTY_UNKNOWN_36 = 0x36,
			PROPERTY_TARGET = 0x34,
			PROPERTY_UNKNOWN_37 = 0x37,
			PROPERTY_UNKNOWN_35 = 0x35,
			PROPERTY_UNKNOWN_2D = 0x2D,
			PROPERTY_UNKNOWN_27 = 0x27,
			PROPERTY_UNKNOWN_41 = 0x41;

		public string Model;
		public int Unknown36;
		public int Target;
		public int Unknown37;
		public int Unknown35;
		public bool HasUnknown2D;
		public bool HasUnknown41;
		public bool HasUnknown27;
		public int Unknown27_36;
		public bool HasUnknown27_2;
		public int Unknown27_37;

		public static EVB_Animation Read(LRBinaryReader p_reader)
		{
			EVB_Animation val = new EVB_Animation();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_UNKNOWN_36:
						val.Unknown36 = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_TARGET:
						val.Target = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_UNKNOWN_37:
						val.Unknown37 = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_UNKNOWN_35:
						val.Unknown35 = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_UNKNOWN_2D:
						val.HasUnknown2D = true;
						break;
					case PROPERTY_UNKNOWN_41:
						val.HasUnknown41 = true;
						break;
					case PROPERTY_UNKNOWN_27:
					{
						if (!val.HasUnknown27)
						{
							val.HasUnknown27 = true;
							byte nextProp = p_reader.ReadByte();
							if (nextProp == PROPERTY_UNKNOWN_36)
								val.Unknown27_36 = p_reader.ReadIntWithHeader();
						}
						else
						{
							val.HasUnknown27_2 = true;
							byte nextProp = p_reader.ReadByte();
							if (nextProp == PROPERTY_UNKNOWN_37)
								val.Unknown27_37 = p_reader.ReadIntWithHeader();
						}
						break;
					}
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Animation p_value)
		{
			if (p_value.Model != null)
			{
				p_writer.WriteByte(0x33);
				p_writer.WriteStringWithHeader(p_value.Model);
			}

			p_writer.WriteByte(0x36);
			p_writer.WriteIntWithHeader(p_value.Unknown36);

			p_writer.WriteByte(0x34);
			p_writer.WriteIntWithHeader(p_value.Target);

			p_writer.WriteByte(0x37);
			p_writer.WriteIntWithHeader(p_value.Unknown37);

			p_writer.WriteByte(0x35);
			p_writer.WriteIntWithHeader(p_value.Unknown35);

			if (p_value.HasUnknown2D)
				p_writer.WriteByte(0x2D);

			if (p_value.HasUnknown41)
				p_writer.WriteByte(0x41);

			if (p_value.HasUnknown27)
			{
				p_writer.WriteByte(0x27);
				p_writer.WriteByte(0x36);
				p_writer.WriteIntWithHeader(p_value.Unknown27_36);
			}

			if (p_value.HasUnknown27_2)
			{
				p_writer.WriteByte(0x27);
				p_writer.WriteByte(0x37);
				p_writer.WriteIntWithHeader(p_value.Unknown27_37);
			}
		}
	}

	public class EVB_Action
	{
		private const byte
			PROPERTY_MODEL = 0x33,
			PROPERTY_COLLISION = 0x4A;

		public string Model;
		public string Collision;

		public static EVB_Action Read(LRBinaryReader p_reader)
		{
			EVB_Action val = new EVB_Action();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_COLLISION:
						val.Collision = p_reader.ReadStringWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Action p_value)
		{
			if (p_value.Model != null)
			{
				p_writer.WriteByte(0x33);
				p_writer.WriteStringWithHeader(p_value.Model);
			}

			if (p_value.Collision != null)
			{
				p_writer.WriteByte(0x4A);
				p_writer.WriteStringWithHeader(p_value.Collision);
			}
		}
	}

	public class EVB_Model
	{
		private const byte
			PROPERTY_MODEL = 0x33,
			PROPERTY_POSITION = 0x3B,
			PROPERTY_NAME = 0x3D,
			PROPERTY_TRANSFORM = 0x3E,
			PROPERTY_UNKNOWN_3F = 0x3F,
			PROPERTY_UNKNOWN_54 = 0x54;

		public string Name;
		public string Model;
		public bool HasPosition;
		public LRVector3 Position;
		public bool HasTransform;
		public float[] Transform;
		public bool HasUnknown3F;
		public bool HasUnknown54;
		public int Unknown54;

		public static EVB_Model Read(LRBinaryReader p_reader)
		{
			EVB_Model val = new EVB_Model();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_NAME:
						val.Name = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_POSITION:
						val.HasPosition = true;
						val.Position = LRVector3.Read(p_reader);
						break;
					case PROPERTY_TRANSFORM:
						val.HasTransform = true;
						val.Transform = new float[6];
						for (int i = 0; i < 6; i++)
							val.Transform[i] = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_UNKNOWN_3F:
						val.HasUnknown3F = true;
						break;
					case PROPERTY_UNKNOWN_54:
						val.HasUnknown54 = true;
						val.Unknown54 = p_reader.ReadIntWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Model p_value)
		{
			if (p_value.Name != null)
			{
				p_writer.WriteByte(PROPERTY_NAME);
				p_writer.WriteStringWithHeader(p_value.Name);
			}

			if (p_value.HasPosition)
			{
				p_writer.WriteByte(PROPERTY_POSITION);
				LRVector3.Write(p_writer, p_value.Position);
			}

			if (p_value.HasTransform)
			{
				p_writer.WriteByte(PROPERTY_TRANSFORM);
				for (int i = 0; i < p_value.Transform.Length; i++)
					p_writer.WriteFloatWithHeader(p_value.Transform[i]);
			}

			if (p_value.HasUnknown3F)
				p_writer.WriteByte(PROPERTY_UNKNOWN_3F);

			if (p_value.Model != null)
			{
				p_writer.WriteByte(PROPERTY_MODEL);
				p_writer.WriteStringWithHeader(p_value.Model);
			}

			if (p_value.HasUnknown54)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_54);
				p_writer.WriteIntWithHeader(p_value.Unknown54);
			}
		}
	}

	public class EVB_Effect
	{
		private const byte
			PROPERTY_NAME = 0x43,
			PROPERTY_VALUE = 0x44,
			PROPERTY_UNKNOWN_45 = 0x45,
			PROPERTY_UNKNOWN_46 = 0x46;

		public string Name;
		public int Value;
		public bool HasUnknown45;
		public bool HasUnknown46;

		public static EVB_Effect Read(LRBinaryReader p_reader)
		{
			EVB_Effect val = new EVB_Effect();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_NAME:
						val.Name = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_VALUE:
						val.Value = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_UNKNOWN_45:
						val.HasUnknown45 = true;
						break;
					case PROPERTY_UNKNOWN_46:
						val.HasUnknown46 = true;
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_Effect p_value)
		{
			if (p_value.Name != null)
			{
				p_writer.WriteByte(PROPERTY_NAME);
				p_writer.WriteStringWithHeader(p_value.Name);
			}

			p_writer.WriteByte(PROPERTY_VALUE);
			p_writer.WriteIntWithHeader(p_value.Value);

			if (p_value.HasUnknown45)
				p_writer.WriteByte(PROPERTY_UNKNOWN_45);

			if (p_value.HasUnknown46)
				p_writer.WriteByte(PROPERTY_UNKNOWN_46);
		}
	}

	public class EVB_SoundAction
	{
		private const byte
			PROPERTY_UNKNOWN_41 = 0x41,
			PROPERTY_MODEL = 0x33;

		public bool HasUnknown41;
		public string Model;

		public static EVB_SoundAction Read(LRBinaryReader p_reader)
		{
			EVB_SoundAction val = new EVB_SoundAction();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_UNKNOWN_41:
						val.HasUnknown41 = true;
						break;
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, EVB_SoundAction p_value)
		{
			if (p_value.HasUnknown41)
				p_writer.WriteByte(0x41);

			if (p_value.Model != null)
			{
				p_writer.WriteByte(0x33);
				p_writer.WriteStringWithHeader(p_value.Model);
			}
		}
	}
}
