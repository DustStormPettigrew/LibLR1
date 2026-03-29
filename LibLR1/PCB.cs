using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Part Configuration format. Defines body part databases with model references, set associations, and part categories.
	/// </summary>
	public class PCB
	{
		private const byte
			ID_WDF_FILES = 0x27,
			ID_PATHS = 0x28,
			ID_HATS = 0x29,
			ID_HEADS = 0x2A,
			ID_CHESTS = 0x2B,
			ID_LEGS = 0x2C,
			ID_HAT_MODELS = 0x2D,
			ID_SELECTORS = 0x2E,
			ID_BODY_PARTS = 0x2F;

		private string[] m_paths;
		private string[] m_wdfFiles;
		private PCB_PartEntry[] m_hats;
		private string m_headsHeader;
		private PCB_PartEntry[] m_heads;
		private string m_chestsCategory;
		private string m_chestsHook;
		private PCB_BodyEntry[] m_chests;
		private string m_legsCategory;
		private string m_legsHook;
		private PCB_BodyEntry[] m_legs;
		private string[] m_hatModels;
		private string[] m_selectors;
		private string[] m_bodyParts;

		public string[] Paths
		{
			get { return m_paths; }
			set { m_paths = value; }
		}

		public string[] WdfFiles
		{
			get { return m_wdfFiles; }
			set { m_wdfFiles = value; }
		}

		public PCB_PartEntry[] Hats
		{
			get { return m_hats; }
			set { m_hats = value; }
		}

		public string HeadsHeader
		{
			get { return m_headsHeader; }
			set { m_headsHeader = value; }
		}

		public PCB_PartEntry[] Heads
		{
			get { return m_heads; }
			set { m_heads = value; }
		}

		public string ChestsCategory
		{
			get { return m_chestsCategory; }
			set { m_chestsCategory = value; }
		}

		public string ChestsHook
		{
			get { return m_chestsHook; }
			set { m_chestsHook = value; }
		}

		public PCB_BodyEntry[] Chests
		{
			get { return m_chests; }
			set { m_chests = value; }
		}

		public string LegsCategory
		{
			get { return m_legsCategory; }
			set { m_legsCategory = value; }
		}

		public string LegsHook
		{
			get { return m_legsHook; }
			set { m_legsHook = value; }
		}

		public PCB_BodyEntry[] Legs
		{
			get { return m_legs; }
			set { m_legs = value; }
		}

		public string[] HatModels
		{
			get { return m_hatModels; }
			set { m_hatModels = value; }
		}

		public string[] Selectors
		{
			get { return m_selectors; }
			set { m_selectors = value; }
		}

		public string[] BodyParts
		{
			get { return m_bodyParts; }
			set { m_bodyParts = value; }
		}

		public PCB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public PCB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_PATHS:
					{
						m_paths = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_WDF_FILES:
					{
						m_wdfFiles = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_HATS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_hats = new PCB_PartEntry[count];
						for (int i = 0; i < count; i++)
						{
							m_hats[i] = new PCB_PartEntry();
							m_hats[i].Name = p_reader.ReadStringWithHeader();
							m_hats[i].SetIndex = p_reader.ReadIntWithHeader();
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_HEADS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_headsHeader = p_reader.ReadStringWithHeader();
						m_heads = new PCB_PartEntry[count];
						for (int i = 0; i < count; i++)
						{
							m_heads[i] = new PCB_PartEntry();
							m_heads[i].Name = p_reader.ReadStringWithHeader();
							m_heads[i].SetIndex = p_reader.ReadIntWithHeader();
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_CHESTS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_chestsCategory = p_reader.ReadStringWithHeader();
						m_chestsHook = p_reader.ReadStringWithHeader();
						m_chests = new PCB_BodyEntry[count];
						for (int i = 0; i < count; i++)
						{
							m_chests[i] = new PCB_BodyEntry();
							m_chests[i].Name = p_reader.ReadStringWithHeader();
							m_chests[i].Unknown1 = p_reader.ReadIntWithHeader();
							m_chests[i].SetIndex = p_reader.ReadIntWithHeader();
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_LEGS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_legsCategory = p_reader.ReadStringWithHeader();
						m_legsHook = p_reader.ReadStringWithHeader();
						m_legs = new PCB_BodyEntry[count];
						for (int i = 0; i < count; i++)
						{
							m_legs[i] = new PCB_BodyEntry();
							m_legs[i].Name = p_reader.ReadStringWithHeader();
							m_legs[i].Unknown1 = p_reader.ReadIntWithHeader();
							m_legs[i].SetIndex = p_reader.ReadIntWithHeader();
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_HAT_MODELS:
					{
						m_hatModels = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_SELECTORS:
					{
						m_selectors = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_BODY_PARTS:
					{
						m_bodyParts = p_reader.ReadStringArrayBlock();
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
			if (m_paths != null)
			{
				p_writer.WriteByte(ID_PATHS);
				p_writer.WriteStringArrayBlock(m_paths);
			}

			if (m_wdfFiles != null)
			{
				p_writer.WriteByte(ID_WDF_FILES);
				p_writer.WriteStringArrayBlock(m_wdfFiles);
			}

			if (m_hats != null)
			{
				p_writer.WriteByte(ID_HATS);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_hats.Length);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				for (int i = 0; i < m_hats.Length; i++)
				{
					p_writer.WriteStringWithHeader(m_hats[i].Name);
					p_writer.WriteIntWithHeader(m_hats[i].SetIndex);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_heads != null)
			{
				p_writer.WriteByte(ID_HEADS);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_heads.Length);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				p_writer.WriteStringWithHeader(m_headsHeader);
				for (int i = 0; i < m_heads.Length; i++)
				{
					p_writer.WriteStringWithHeader(m_heads[i].Name);
					p_writer.WriteIntWithHeader(m_heads[i].SetIndex);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_chests != null)
			{
				p_writer.WriteByte(ID_CHESTS);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_chests.Length);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				p_writer.WriteStringWithHeader(m_chestsCategory);
				p_writer.WriteStringWithHeader(m_chestsHook);
				for (int i = 0; i < m_chests.Length; i++)
				{
					p_writer.WriteStringWithHeader(m_chests[i].Name);
					p_writer.WriteIntWithHeader(m_chests[i].Unknown1);
					p_writer.WriteIntWithHeader(m_chests[i].SetIndex);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_legs != null)
			{
				p_writer.WriteByte(ID_LEGS);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_legs.Length);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				p_writer.WriteStringWithHeader(m_legsCategory);
				p_writer.WriteStringWithHeader(m_legsHook);
				for (int i = 0; i < m_legs.Length; i++)
				{
					p_writer.WriteStringWithHeader(m_legs[i].Name);
					p_writer.WriteIntWithHeader(m_legs[i].Unknown1);
					p_writer.WriteIntWithHeader(m_legs[i].SetIndex);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_selectors != null)
			{
				p_writer.WriteByte(ID_SELECTORS);
				p_writer.WriteStringArrayBlock(m_selectors);
			}

			if (m_hatModels != null)
			{
				p_writer.WriteByte(ID_HAT_MODELS);
				p_writer.WriteStringArrayBlock(m_hatModels);
			}

			if (m_bodyParts != null)
			{
				p_writer.WriteByte(ID_BODY_PARTS);
				p_writer.WriteStringArrayBlock(m_bodyParts);
			}
		}
	}

	public class PCB_PartEntry
	{
		public string Name;
		public int SetIndex;
	}

	public class PCB_BodyEntry
	{
		public string Name;
		public int Unknown1;
		public int SetIndex;
	}
}
