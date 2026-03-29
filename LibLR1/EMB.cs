using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Particle Emitter List format. Defines named particle emitter configurations.
	/// </summary>
	public class EMB
	{
		private const byte
			ID_EMITTERS = 0x27;

		private Dictionary<string, EMB_Emitter> m_emitters;

		public Dictionary<string, EMB_Emitter> Emitters
		{
			get { return m_emitters; }
			set { m_emitters = value; }
		}

		public EMB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public EMB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_EMITTERS:
					{
						m_emitters = p_reader.ReadDictionaryBlock<EMB_Emitter>(
							EMB_Emitter.Read,
							ID_EMITTERS
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
			p_writer.WriteByte(ID_EMITTERS);
			p_writer.WriteDictionaryBlock<EMB_Emitter>(
				EMB_Emitter.Write,
				m_emitters,
				ID_EMITTERS
			);
		}
	}

	public class EMB_Emitter
	{
		private const byte
			PROPERTY_SIZE = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_DIRECTION = 0x2A,
			PROPERTY_POSITIONS = 0x2B,
			PROPERTY_SCALE_MIN = 0x2C,
			PROPERTY_SCALE_MAX = 0x2D,
			PROPERTY_VARIANT = 0x2E,
			PROPERTY_LIFETIME = 0x2F,
			PROPERTY_LOOP = 0x30,
			PROPERTY_SPEED_MIN = 0x31,
			PROPERTY_SPEED_MAX = 0x32,
			PROPERTY_RANGE = 0x33,
			PROPERTY_TEXTURE = 0x34,
			PROPERTY_COLOR = 0x35;

		public float Size;
		public float Unknown29;
		public LRVector3 Direction;
		public LRVector3[] Positions;
		public float ScaleMin;
		public float ScaleMax;
		public int Lifetime;
		public int Loop;
		public float SpeedMin;
		public float SpeedMax;
		public float Range;
		public string Texture;
		public bool HasVariant;
		public int Variant;
		public bool HasColor;
		public int Color;

		public static EMB_Emitter Read(LRBinaryReader p_reader)
		{
			EMB_Emitter val = new EMB_Emitter();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_SIZE:
					{
						val.Size = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.Unknown29 = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_DIRECTION:
					{
						val.Direction = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_POSITIONS:
					{
						val.Positions = p_reader.ReadVector3fArrayBlock();
						break;
					}
					case PROPERTY_SCALE_MIN:
					{
						val.ScaleMin = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_SCALE_MAX:
					{
						val.ScaleMax = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_VARIANT:
					{
						val.HasVariant = true;
						val.Variant = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_LIFETIME:
					{
						val.Lifetime = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_LOOP:
					{
						val.Loop = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_SPEED_MIN:
					{
						val.SpeedMin = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_SPEED_MAX:
					{
						val.SpeedMax = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_RANGE:
					{
						val.Range = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_TEXTURE:
					{
						val.Texture = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_COLOR:
					{
						val.HasColor = true;
						val.Color = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, EMB_Emitter p_value)
		{
			p_writer.WriteByte(PROPERTY_SIZE);
			p_writer.WriteFloatWithHeader(p_value.Size);

			p_writer.WriteByte(PROPERTY_UNKNOWN_29);
			p_writer.WriteFloatWithHeader(p_value.Unknown29);

			p_writer.WriteByte(PROPERTY_SCALE_MIN);
			p_writer.WriteFloatWithHeader(p_value.ScaleMin);

			p_writer.WriteByte(PROPERTY_SCALE_MAX);
			p_writer.WriteFloatWithHeader(p_value.ScaleMax);

			p_writer.WriteByte(PROPERTY_SPEED_MIN);
			p_writer.WriteFloatWithHeader(p_value.SpeedMin);

			p_writer.WriteByte(PROPERTY_SPEED_MAX);
			p_writer.WriteFloatWithHeader(p_value.SpeedMax);

			p_writer.WriteByte(PROPERTY_LIFETIME);
			p_writer.WriteIntWithHeader(p_value.Lifetime);

			p_writer.WriteByte(PROPERTY_LOOP);
			p_writer.WriteIntWithHeader(p_value.Loop);

			p_writer.WriteByte(PROPERTY_RANGE);
			p_writer.WriteFloatWithHeader(p_value.Range);

			p_writer.WriteByte(PROPERTY_DIRECTION);
			LRVector3.Write(p_writer, p_value.Direction);

			if (p_value.Positions != null)
			{
				p_writer.WriteByte(PROPERTY_POSITIONS);
				p_writer.WriteVector3fArrayBlock(p_value.Positions);
			}

			if (p_value.HasVariant)
			{
				p_writer.WriteByte(PROPERTY_VARIANT);
				p_writer.WriteIntWithHeader(p_value.Variant);
			}

			if (p_value.HasColor)
			{
				p_writer.WriteByte(PROPERTY_COLOR);
				p_writer.WriteIntWithHeader(p_value.Color);
			}

			if (p_value.Texture != null)
			{
				p_writer.WriteByte(PROPERTY_TEXTURE);
				p_writer.WriteStringWithHeader(p_value.Texture);
			}
		}
	}
}
