using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	public class PWB
	{
		public const byte
			COLOR_RED    = 0x2A,
			COLOR_YELLOW = 0x2B,
			COLOR_BLUE   = 0x2C,
			COLOR_GREEN  = 0x2D;

		private const byte
			ID_COLOR_BRICKS = 0x27,
			ID_WHITE_BRICKS = 0x2F;

		private List<PWB_ColorBrick> m_colorBricks;
		private List<PWB_WhiteBrick> m_whiteBricks;

		public List<PWB_ColorBrick> ColorBricks
		{
			get { return m_colorBricks; }
			set { m_colorBricks = value; }
		}

		public List<PWB_WhiteBrick> WhiteBricks
		{
			get { return m_whiteBricks; }
			set { m_whiteBricks = value; }
		}

		public PWB()
		{
			m_colorBricks = new List<PWB_ColorBrick>();
			m_whiteBricks = new List<PWB_WhiteBrick>();
		}

		public PWB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public PWB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_COLOR_BRICKS:
					{
						m_colorBricks = p_reader.ReadStructListBlock<PWB_ColorBrick>(
							PWB_ColorBrick.Read,
							ID_COLOR_BRICKS
						);
						break;
					}
					case ID_WHITE_BRICKS:
					{
						m_whiteBricks = p_reader.ReadStructListBlock<PWB_WhiteBrick>(
							PWB_WhiteBrick.Read,
							ID_WHITE_BRICKS
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
			p_writer.WriteByte(ID_COLOR_BRICKS);
			p_writer.WriteStructListBlock<PWB_ColorBrick>(
				PWB_ColorBrick.Write,
				m_colorBricks,
				ID_COLOR_BRICKS
			);
			p_writer.WriteByte(ID_WHITE_BRICKS);
			p_writer.WriteStructListBlock<PWB_WhiteBrick>(
				PWB_WhiteBrick.Write,
				m_whiteBricks,
				ID_WHITE_BRICKS
			);
		}
	}

	public class PWB_ColorBrick
	{
		private const byte
			PROPERTY_POSITION     = 0x28,
			PROPERTY_COLOR_RED    = 0x2A,
			PROPERTY_COLOR_YELLOW = 0x2B,
			PROPERTY_COLOR_BLUE   = 0x2C,
			PROPERTY_COLOR_GREEN  = 0x2D;

		public LRVector3 Position;
		public byte Color;

		public PWB_ColorBrick()
			: this(new LRVector3(), PROPERTY_COLOR_RED) { }

		public PWB_ColorBrick(LRVector3 position, byte color)
		{
			Position = position;
			Color = color;
		}

		public static PWB_ColorBrick Read(LRBinaryReader p_reader)
		{
			PWB_ColorBrick val = new PWB_ColorBrick();
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
					case PROPERTY_COLOR_RED:
					case PROPERTY_COLOR_YELLOW:
					case PROPERTY_COLOR_BLUE:
					case PROPERTY_COLOR_GREEN:
					{
						val.Color = propertyId;
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

		public static void Write(LRBinaryWriter p_writer, PWB_ColorBrick p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			LRVector3.Write(p_writer, p_value.Position);
			p_writer.WriteByte(p_value.Color);
		}
	}

	public class PWB_WhiteBrick
	{
		private const byte
			PROPERTY_POSITION = 0x28;

		public LRVector3 Position;

		public PWB_WhiteBrick()
			: this(new LRVector3()) { }

		public PWB_WhiteBrick(LRVector3 position)
		{
			Position = position;
		}

		public static PWB_WhiteBrick Read(LRBinaryReader p_reader)
		{
			PWB_WhiteBrick val = new PWB_WhiteBrick();
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

		public static void Write(LRBinaryWriter p_writer, PWB_WhiteBrick p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			LRVector3.Write(p_writer, p_value.Position);
		}
	}
}