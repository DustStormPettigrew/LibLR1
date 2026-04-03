using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Chassis format. Defines chassis configurations with physics and geometry data.
	/// </summary>
	public class CMB
	{
		private const byte
			ID_CHASSIS = 0x27;

		private Dictionary<string, CMB_Chassis> m_chassis;

		public Dictionary<string, CMB_Chassis> Chassis
		{
			get { return m_chassis; }
			set { m_chassis = value; }
		}

		public CMB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CMB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_CHASSIS:
					{
						m_chassis = p_reader.ReadDictionaryBlock<CMB_Chassis>(
							CMB_Chassis.Read,
							ID_CHASSIS
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
			p_writer.WriteByte(ID_CHASSIS);
			p_writer.WriteDictionaryBlock<CMB_Chassis>(
				CMB_Chassis.Write,
				m_chassis,
				ID_CHASSIS
			);
		}
	}

	public class CMB_Chassis
	{
		private const byte
			PROPERTY_WHEEL_MODELS = 0x28,
			PROPERTY_BRICK_SET = 0x39,
			PROPERTY_GRAVITY = 0x2A,
			PROPERTY_CENTER_OF_MASS = 0x2B,
			PROPERTY_MASS = 0x2C,
			PROPERTY_SUSPENSION = 0x2D,
			PROPERTY_TIRE_GRIP = 0x2E,
			PROPERTY_DRAG = 0x2F,
			PROPERTY_WHEEL_POSITIONS = 0x30,
			PROPERTY_AXLE_POSITIONS = 0x31,
			PROPERTY_UNKNOWN_32 = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_SPEED_LOW = 0x3A,
			PROPERTY_SPEED_MID = 0x3B,
			PROPERTY_SPEED_HIGH = 0x3C;

		public CMB_WheelModels WheelModels;
		public string BrickSet;
		public LRVector3 Gravity;
		public LRVector3 CenterOfMass;
		public float Mass;
		public float SuspensionMin;
		public float SuspensionMax;
		public float TireGripMin;
		public float TireGripMax;
		public float Drag;
		public float[] WheelPositions;
		public float[] AxlePositions;
		public int Unknown32;
		public int Unknown33;
		public bool HasSpeedLow;
		public int SpeedLow;
		public bool HasSpeedMid;
		public int SpeedMid;
		public bool HasSpeedHigh;
		public int SpeedHigh;

		public static CMB_Chassis Read(LRBinaryReader p_reader)
		{
			CMB_Chassis val = new CMB_Chassis();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_WHEEL_MODELS:
					{
						val.WheelModels = p_reader.ReadStruct<CMB_WheelModels>(
							CMB_WheelModels.Read
						);
						break;
					}
					case PROPERTY_BRICK_SET:
					{
						val.BrickSet = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_GRAVITY:
					{
						val.Gravity = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_CENTER_OF_MASS:
					{
						val.CenterOfMass = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_MASS:
					{
						val.Mass = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_SUSPENSION:
					{
						val.SuspensionMin = p_reader.ReadFloatWithHeader();
						val.SuspensionMax = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_TIRE_GRIP:
					{
						val.TireGripMin = p_reader.ReadFloatWithHeader();
						val.TireGripMax = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_DRAG:
					{
						val.Drag = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_WHEEL_POSITIONS:
					{
						val.WheelPositions = p_reader.ReadStruct<float[]>(
							(r) =>
							{
								float[] floats = new float[14];
								for (int i = 0; i < 14; i++)
									floats[i] = r.ReadFloatWithHeader();
								return floats;
							}
						);
						break;
					}
					case PROPERTY_AXLE_POSITIONS:
					{
						val.AxlePositions = p_reader.ReadStruct<float[]>(
							(r) =>
							{
								float[] floats = new float[12];
								for (int i = 0; i < 12; i++)
									floats[i] = r.ReadFloatWithHeader();
								return floats;
							}
						);
						break;
					}
					case PROPERTY_UNKNOWN_32:
					{
						val.Unknown32 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown33 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_SPEED_LOW:
					{
						val.HasSpeedLow = true;
						val.SpeedLow = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_SPEED_MID:
					{
						val.HasSpeedMid = true;
						val.SpeedMid = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_SPEED_HIGH:
					{
						val.HasSpeedHigh = true;
						val.SpeedHigh = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, CMB_Chassis p_value)
		{
			if (p_value.WheelModels != null)
			{
				p_writer.WriteByte(PROPERTY_WHEEL_MODELS);
				p_writer.WriteStruct<CMB_WheelModels>(CMB_WheelModels.Write, p_value.WheelModels);
			}

			if (p_value.BrickSet != null)
			{
				p_writer.WriteByte(PROPERTY_BRICK_SET);
				p_writer.WriteStringWithHeader(p_value.BrickSet);
			}

			p_writer.WriteByte(PROPERTY_CENTER_OF_MASS);
			LRVector3.Write(p_writer, p_value.CenterOfMass);

			p_writer.WriteByte(PROPERTY_MASS);
			p_writer.WriteFloatWithHeader(p_value.Mass);

			p_writer.WriteByte(PROPERTY_GRAVITY);
			LRVector3.Write(p_writer, p_value.Gravity);

			p_writer.WriteByte(PROPERTY_DRAG);
			p_writer.WriteFloatWithHeader(p_value.Drag);

			p_writer.WriteByte(PROPERTY_SUSPENSION);
			p_writer.WriteFloatWithHeader(p_value.SuspensionMin);
			p_writer.WriteFloatWithHeader(p_value.SuspensionMax);

			p_writer.WriteByte(PROPERTY_TIRE_GRIP);
			p_writer.WriteFloatWithHeader(p_value.TireGripMin);
			p_writer.WriteFloatWithHeader(p_value.TireGripMax);

			if (p_value.WheelPositions != null)
			{
				p_writer.WriteByte(PROPERTY_WHEEL_POSITIONS);
				p_writer.WriteToken(Token.LeftCurly);
				for (int i = 0; i < p_value.WheelPositions.Length; i++)
					p_writer.WriteFloatWithHeader(p_value.WheelPositions[i]);
				p_writer.WriteToken(Token.RightCurly);
			}

			if (p_value.AxlePositions != null)
			{
				p_writer.WriteByte(PROPERTY_AXLE_POSITIONS);
				p_writer.WriteToken(Token.LeftCurly);
				for (int i = 0; i < p_value.AxlePositions.Length; i++)
					p_writer.WriteFloatWithHeader(p_value.AxlePositions[i]);
				p_writer.WriteToken(Token.RightCurly);
			}

			p_writer.WriteByte(PROPERTY_UNKNOWN_32);
			p_writer.WriteIntWithHeader(p_value.Unknown32);

			p_writer.WriteByte(PROPERTY_UNKNOWN_33);
			p_writer.WriteIntWithHeader(p_value.Unknown33);

			if (p_value.HasSpeedLow)
			{
				p_writer.WriteByte(PROPERTY_SPEED_LOW);
				p_writer.WriteIntWithHeader(p_value.SpeedLow);
			}

			if (p_value.HasSpeedMid)
			{
				p_writer.WriteByte(PROPERTY_SPEED_MID);
				p_writer.WriteIntWithHeader(p_value.SpeedMid);
			}

			if (p_value.HasSpeedHigh)
			{
				p_writer.WriteByte(PROPERTY_SPEED_HIGH);
				p_writer.WriteIntWithHeader(p_value.SpeedHigh);
			}
		}
	}

	public class CMB_WheelModels
	{
		private const byte
			PROPERTY_FRONT_LEFT = 0x34,
			PROPERTY_FRONT_RIGHT = 0x35,
			PROPERTY_REAR_LEFT = 0x36,
			PROPERTY_REAR_RIGHT = 0x37,
			PROPERTY_SPARE = 0x38;

		public string FrontLeft;
		public string FrontRight;
		public string RearLeft;
		public string RearRight;
		public string Spare;

		public static CMB_WheelModels Read(LRBinaryReader p_reader)
		{
			CMB_WheelModels val = new CMB_WheelModels();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_FRONT_LEFT:
						val.FrontLeft = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_FRONT_RIGHT:
						val.FrontRight = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_REAR_LEFT:
						val.RearLeft = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_REAR_RIGHT:
						val.RearRight = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_SPARE:
						val.Spare = p_reader.ReadStringWithHeader();
						break;
					default:
						throw new UnexpectedPropertyException(
							propertyId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, CMB_WheelModels p_value)
		{
			if (p_value.FrontLeft != null)
			{
				p_writer.WriteByte(PROPERTY_FRONT_LEFT);
				p_writer.WriteStringWithHeader(p_value.FrontLeft);
			}
			if (p_value.FrontRight != null)
			{
				p_writer.WriteByte(PROPERTY_FRONT_RIGHT);
				p_writer.WriteStringWithHeader(p_value.FrontRight);
			}
			if (p_value.RearLeft != null)
			{
				p_writer.WriteByte(PROPERTY_REAR_LEFT);
				p_writer.WriteStringWithHeader(p_value.RearLeft);
			}
			if (p_value.RearRight != null)
			{
				p_writer.WriteByte(PROPERTY_REAR_RIGHT);
				p_writer.WriteStringWithHeader(p_value.RearRight);
			}
			if (p_value.Spare != null)
			{
				p_writer.WriteByte(PROPERTY_SPARE);
				p_writer.WriteStringWithHeader(p_value.Spare);
			}
		}
	}
}
