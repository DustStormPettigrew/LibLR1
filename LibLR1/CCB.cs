using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1
{
	public class CCB
	{
		private const byte
			ID_CARS = 0x27;
		
		private Dictionary<string, CCB_Car> m_Cars;
		
		public Dictionary<string, CCB_Car> Cars
		{
			get { return m_Cars; }
			set { m_Cars = value; }
		}
		
		public CCB(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_CARS:
					{
						m_Cars = BinaryFileHelper.ReadDictionaryBlock<CCB_Car>(
							stream,
							new BinaryFileHelper.ReadObject<CCB_Car>(
								CCB_Car.FromStream
							),
							ID_CARS
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
		
		public CCB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
		}
		
		public void Save(Stream stream)
		{
			stream.WriteByte(ID_CARS);
			BinaryFileHelper.WriteDictionaryBlock<CCB_Car>(
				stream,
				new BinaryFileHelper.WriteObject<CCB_Car>(
					CCB_Car.ToStream
				),
				m_Cars,
				ID_CARS
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
		
		public static CCB_Car FromStream(Stream stream)
		{
			CCB_Car val = new CCB_Car();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.Unknown29 = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.Unknown2C = BinaryFileHelper.ReadFloatWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_2D:
					{
						val.Unknown2D = LRVector3.FromStream(stream);
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
		
		public static void ToStream(Stream stream, CCB_Car value)
		{
			stream.WriteByte(PROPERTY_UNKNOWN_28);
			BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown28);
			
			stream.WriteByte(PROPERTY_UNKNOWN_29);
			BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown29);
			
			stream.WriteByte(PROPERTY_UNKNOWN_2A);
			BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown2A);
			
			stream.WriteByte(PROPERTY_UNKNOWN_2B);
			BinaryFileHelper.WriteStringWithHeader(stream, value.Unknown2B);
			
			stream.WriteByte(PROPERTY_UNKNOWN_2C);
			BinaryFileHelper.WriteFloatWithHeader(stream, value.Unknown2C);
			
			stream.WriteByte(PROPERTY_UNKNOWN_2D);
			LRVector3.ToStream(stream, value.Unknown2D);
		}
	}
}