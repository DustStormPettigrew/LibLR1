using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.IO;

namespace LibLR1
{
	public class MAB
	{
		private const byte
			ID_MATERIALFRAMES = 0x27,
			ID_ANIMATIONS = 0x28;

		private MAB_MaterialFrame[] m_materialFrames;
		private MAB_Animation[] m_animations;

		public MAB_MaterialFrame[] MaterialFrames
		{
			get { return m_materialFrames; }
			set { m_materialFrames = value; }
		}
		public MAB_Animation[] Animations
		{
			get { return m_animations; }
			set { m_animations = value; }
		}
		
		public MAB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public MAB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_MATERIALFRAMES:
					{
						m_materialFrames = p_reader.ReadArrayBlock<MAB_MaterialFrame>(
							MAB_MaterialFrame.Read
						);
						break;
					}
					case ID_ANIMATIONS:
					{
						m_animations = p_reader.ReadStructArrayBlock<MAB_Animation>(
							MAB_Animation.Read,
							ID_ANIMATIONS
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
			p_writer.WriteByte(ID_MATERIALFRAMES);
			p_writer.WriteArrayBlock<MAB_MaterialFrame>(
				MAB_MaterialFrame.Write,
				m_materialFrames
			);
			p_writer.WriteByte(ID_ANIMATIONS);
			p_writer.WriteStructArrayBlock<MAB_Animation>(
				MAB_Animation.Write,
				m_animations,
				ID_ANIMATIONS
			);
		}
	}

	public class MAB_MaterialFrame
	{
		public string Material;
		public int Frame;

		public static MAB_MaterialFrame Read(LRBinaryReader p_reader)
		{
			MAB_MaterialFrame val = new MAB_MaterialFrame();
			val.Material = p_reader.ReadStringWithHeader();
			val.Frame    = p_reader.ReadIntWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, MAB_MaterialFrame p_value)
		{
			p_writer.WriteStringWithHeader(p_value.Material);
			p_writer.WriteIntWithHeader(p_value.Frame);
		}
	}

	public class MAB_Animation
	{
		private const byte
			PROPERTY_MATERIALFRAMES = 0x27,
			PROPERTY_NUM_FRAMES = 0x29,
			PROPERTY_SPEED = 0x2A;

		public int AnimationOffset;
		public int AnimationLength;
		public int Frames;
		public int Speed;

		public static MAB_Animation Read(LRBinaryReader p_reader)
		{
			MAB_Animation val = new MAB_Animation();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_MATERIALFRAMES:
					{
						val.AnimationOffset = p_reader.ReadIntWithHeader();
						val.AnimationLength = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_NUM_FRAMES:
					{
						val.Frames = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_SPEED:
					{
						val.Speed = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MAB_Animation p_value)
		{
			p_writer.WriteByte(PROPERTY_MATERIALFRAMES);
			p_writer.WriteIntWithHeader(p_value.AnimationOffset);
			p_writer.WriteIntWithHeader(p_value.AnimationLength);
			p_writer.WriteByte(PROPERTY_NUM_FRAMES);
			p_writer.WriteIntWithHeader(p_value.Frames);
			p_writer.WriteByte(PROPERTY_SPEED);
			p_writer.WriteIntWithHeader(p_value.Speed);
		}
	}
}