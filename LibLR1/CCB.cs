using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	public class CCB
	{
		private const byte
			ID_CARS = 0x27;
		
		private Dictionary<string, CCB_Car> m_cars;
		
		public Dictionary<string, CCB_Car> Cars
		{
			get { return m_cars; }
			set { m_cars = value; }
		}

		public CCB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CCB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_CARS:
					{
						m_cars = p_reader.ReadDictionaryBlock<CCB_Car>(
							CCB_Car.Read,
							ID_CARS
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
			p_writer.WriteByte(ID_CARS);
			p_writer.WriteDictionaryBlock<CCB_Car>(
				CCB_Car.Write,
				m_cars,
				ID_CARS
			);
		}
	}
	
	public class CCB_Car
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_UNKNOWN_2A = 0x2A,
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_UNKNOWN_2C = 0x2C,
			PROPERTY_UNKNOWN_2D = 0x2D;
		
		public string    Unknown28;
		public string    Unknown29;
		public string    Unknown2A;
		public string    Unknown2B;
		public float     Unknown2C;
		public LRVector3 Unknown2D;

		public static CCB_Car Read(LRBinaryReader p_reader)
		{
			CCB_Car val = new CCB_Car();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.Unknown29 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.Unknown2C = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2D:
					{
						val.Unknown2D = LRVector3.Read(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, CCB_Car p_value)
		{
			p_writer.WriteByte(PROPERTY_UNKNOWN_28);
			p_writer.WriteStringWithHeader(p_value.Unknown28);

			p_writer.WriteByte(PROPERTY_UNKNOWN_29);
			p_writer.WriteStringWithHeader(p_value.Unknown29);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2A);
			p_writer.WriteStringWithHeader(p_value.Unknown2A);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2B);
			p_writer.WriteStringWithHeader(p_value.Unknown2B);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2C);
			p_writer.WriteFloatWithHeader(p_value.Unknown2C);

			p_writer.WriteByte(PROPERTY_UNKNOWN_2D);
			LRVector3.Write(p_writer, p_value.Unknown2D);
		}
	}
}