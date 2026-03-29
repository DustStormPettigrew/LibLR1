using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Menu Style format. Defines menu layouts with image bindings, text styles, scene configurations, and part containers.
	/// </summary>
	public class MSB
	{
		private const byte
			ID_STYLE_ID = 0x27,
			ID_IMAGES = 0x32,
			ID_TEXT_STYLES = 0x33,
			ID_IMAGE_BOXES = 0x34,
			ID_ICONS = 0x35,
			ID_SCENES = 0x37,
			ID_PIECE_SETS = 0x38,
			ID_PART_CONTAINERS = 0x39,
			ID_SLIDERS = 0x3B,
			ID_TEXT_EDITS = 0x3D,
			ID_BUTTONS = 0x3E;

		private int m_styleId;
		private Dictionary<string, MSB_Image> m_images;
		private Dictionary<string, MSB_TextStyle> m_textStyles;
		private Dictionary<string, MSB_ImageBox> m_imageBoxes;
		private Dictionary<string, MSB_Icon> m_icons;
		private Dictionary<string, MSB_Scene> m_scenes;
		private Dictionary<string, MSB_PieceSet> m_pieceSets;
		private Dictionary<string, MSB_PartContainer> m_partContainers;
		private Dictionary<string, MSB_Slider> m_sliders;
		private MSB_TextEdit[] m_textEdits;
		private Dictionary<string, MSB_Button> m_buttons;

		public int StyleId
		{
			get { return m_styleId; }
			set { m_styleId = value; }
		}

		public Dictionary<string, MSB_Image> Images
		{
			get { return m_images; }
			set { m_images = value; }
		}

		public Dictionary<string, MSB_TextStyle> TextStyles
		{
			get { return m_textStyles; }
			set { m_textStyles = value; }
		}

		public Dictionary<string, MSB_ImageBox> ImageBoxes
		{
			get { return m_imageBoxes; }
			set { m_imageBoxes = value; }
		}

		public Dictionary<string, MSB_Icon> Icons
		{
			get { return m_icons; }
			set { m_icons = value; }
		}

		public Dictionary<string, MSB_Scene> Scenes
		{
			get { return m_scenes; }
			set { m_scenes = value; }
		}

		public Dictionary<string, MSB_PieceSet> PieceSets
		{
			get { return m_pieceSets; }
			set { m_pieceSets = value; }
		}

		public Dictionary<string, MSB_PartContainer> PartContainers
		{
			get { return m_partContainers; }
			set { m_partContainers = value; }
		}

		public Dictionary<string, MSB_Slider> Sliders
		{
			get { return m_sliders; }
			set { m_sliders = value; }
		}

		public MSB_TextEdit[] TextEdits
		{
			get { return m_textEdits; }
			set { m_textEdits = value; }
		}

		public Dictionary<string, MSB_Button> Buttons
		{
			get { return m_buttons; }
			set { m_buttons = value; }
		}

		public MSB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public MSB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_STYLE_ID:
					{
						m_styleId = p_reader.ReadIntWithHeader();
						break;
					}
					case ID_IMAGES:
					{
						m_images = p_reader.ReadDictionaryBlock<MSB_Image>(
							MSB_Image.Read,
							ID_IMAGES
						);
						break;
					}
					case ID_TEXT_STYLES:
					{
						m_textStyles = p_reader.ReadDictionaryBlock<MSB_TextStyle>(
							MSB_TextStyle.Read,
							ID_TEXT_STYLES
						);
						break;
					}
					case ID_IMAGE_BOXES:
					{
						m_imageBoxes = p_reader.ReadDictionaryBlock<MSB_ImageBox>(
							MSB_ImageBox.Read,
							ID_IMAGE_BOXES
						);
						break;
					}
					case ID_ICONS:
					{
						m_icons = p_reader.ReadDictionaryBlock<MSB_Icon>(
							MSB_Icon.Read,
							ID_ICONS
						);
						break;
					}
					case ID_SCENES:
					{
						m_scenes = p_reader.ReadDictionaryBlock<MSB_Scene>(
							MSB_Scene.Read,
							ID_SCENES
						);
						break;
					}
					case ID_PIECE_SETS:
					{
						m_pieceSets = p_reader.ReadDictionaryBlock<MSB_PieceSet>(
							MSB_PieceSet.Read,
							ID_PIECE_SETS
						);
						break;
					}
					case ID_PART_CONTAINERS:
					{
						m_partContainers = p_reader.ReadDictionaryBlock<MSB_PartContainer>(
							MSB_PartContainer.Read,
							ID_PART_CONTAINERS
						);
						break;
					}
					case ID_SLIDERS:
					{
						m_sliders = p_reader.ReadDictionaryBlock<MSB_Slider>(
							MSB_Slider.Read,
							ID_SLIDERS
						);
						break;
					}
					case ID_TEXT_EDITS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_textEdits = new MSB_TextEdit[count];
						for (int i = 0; i < count; i++)
						{
							byte entryType = p_reader.ReadByte();
							string name = p_reader.ReadStringWithHeader();
							m_textEdits[i] = p_reader.ReadStruct<MSB_TextEdit>(MSB_TextEdit.Read);
							m_textEdits[i].Name = name;
						}
						p_reader.Expect(Token.RightCurly);
						break;
					}
					case ID_BUTTONS:
					{
						m_buttons = p_reader.ReadDictionaryBlock<MSB_Button>(
							MSB_Button.Read,
							ID_BUTTONS
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
			p_writer.WriteByte(ID_STYLE_ID);
			p_writer.WriteIntWithHeader(m_styleId);

			if (m_images != null)
			{
				p_writer.WriteByte(ID_IMAGES);
				p_writer.WriteDictionaryBlock<MSB_Image>(
					MSB_Image.Write,
					m_images,
					ID_IMAGES
				);
			}

			if (m_textStyles != null)
			{
				p_writer.WriteByte(ID_TEXT_STYLES);
				p_writer.WriteDictionaryBlock<MSB_TextStyle>(
					MSB_TextStyle.Write,
					m_textStyles,
					ID_TEXT_STYLES
				);
			}

			if (m_imageBoxes != null)
			{
				p_writer.WriteByte(ID_IMAGE_BOXES);
				p_writer.WriteDictionaryBlock<MSB_ImageBox>(
					MSB_ImageBox.Write,
					m_imageBoxes,
					ID_IMAGE_BOXES
				);
			}

			if (m_icons != null)
			{
				p_writer.WriteByte(ID_ICONS);
				p_writer.WriteDictionaryBlock<MSB_Icon>(
					MSB_Icon.Write,
					m_icons,
					ID_ICONS
				);
			}

			if (m_scenes != null)
			{
				p_writer.WriteByte(ID_SCENES);
				p_writer.WriteDictionaryBlock<MSB_Scene>(
					MSB_Scene.Write,
					m_scenes,
					ID_SCENES
				);
			}

			if (m_pieceSets != null)
			{
				p_writer.WriteByte(ID_PIECE_SETS);
				p_writer.WriteDictionaryBlock<MSB_PieceSet>(
					MSB_PieceSet.Write,
					m_pieceSets,
					ID_PIECE_SETS
				);
			}

			if (m_partContainers != null)
			{
				p_writer.WriteByte(ID_PART_CONTAINERS);
				p_writer.WriteDictionaryBlock<MSB_PartContainer>(
					MSB_PartContainer.Write,
					m_partContainers,
					ID_PART_CONTAINERS
				);
			}

			if (m_sliders != null)
			{
				p_writer.WriteByte(ID_SLIDERS);
				p_writer.WriteDictionaryBlock<MSB_Slider>(
					MSB_Slider.Write,
					m_sliders,
					ID_SLIDERS
				);
			}

			if (m_textEdits != null)
			{
				p_writer.WriteByte(ID_TEXT_EDITS);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_textEdits.Length);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				for (int i = 0; i < m_textEdits.Length; i++)
				{
					p_writer.WriteByte(0x3A);
					p_writer.WriteStringWithHeader(m_textEdits[i].Name);
					p_writer.WriteStruct<MSB_TextEdit>(MSB_TextEdit.Write, m_textEdits[i]);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_buttons != null)
			{
				p_writer.WriteByte(ID_BUTTONS);
				p_writer.WriteDictionaryBlock<MSB_Button>(
					MSB_Button.Write,
					m_buttons,
					ID_BUTTONS
				);
			}
		}
	}

	public class MSB_Image
	{
		private const byte
			PROPERTY_BINDING = 0x28;

		public string Binding;

		public static MSB_Image Read(LRBinaryReader p_reader)
		{
			MSB_Image val = new MSB_Image();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_BINDING:
						val.Binding = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MSB_Image p_value)
		{
			if (p_value.Binding != null)
			{
				p_writer.WriteByte(PROPERTY_BINDING);
				p_writer.WriteStringWithHeader(p_value.Binding);
			}
		}
	}

	public class MSB_Layout
	{
		private const byte
			PROPERTY_DIMENSIONS = 0x2B,
			PROPERTY_OFFSETS = 0x2C,
			PROPERTY_SCALE = 0x2D,
			PROPERTY_COLORS = 0x2A;

		public int[] Dimensions;
		public int[] Offsets;
		public bool HasScale;
		public int Scale;
		public int[] Colors;

		public static MSB_Layout Read(LRBinaryReader p_reader)
		{
			MSB_Layout val = new MSB_Layout();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_DIMENSIONS:
					{
						List<int> dims = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							dims.Add(p_reader.ReadIntWithHeader());
						}
						val.Dimensions = dims.ToArray();
						break;
					}
					case PROPERTY_OFFSETS:
					{
						List<int> offs = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							offs.Add(p_reader.ReadIntWithHeader());
						}
						val.Offsets = offs.ToArray();
						break;
					}
					case PROPERTY_SCALE:
						val.HasScale = true;
						val.Scale = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_COLORS:
					{
						List<int> colors = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							colors.Add(p_reader.ReadIntWithHeader());
						}
						val.Colors = colors.ToArray();
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

		public static void Write(LRBinaryWriter p_writer, MSB_Layout p_value)
		{
			if (p_value.Dimensions != null)
			{
				p_writer.WriteByte(0x2B);
				for (int i = 0; i < p_value.Dimensions.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Dimensions[i]);
			}

			if (p_value.Offsets != null)
			{
				p_writer.WriteByte(0x2C);
				for (int i = 0; i < p_value.Offsets.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Offsets[i]);
			}

			if (p_value.HasScale)
			{
				p_writer.WriteByte(0x2D);
				p_writer.WriteIntWithHeader(p_value.Scale);
			}

			if (p_value.Colors != null)
			{
				p_writer.WriteByte(0x2A);
				for (int i = 0; i < p_value.Colors.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Colors[i]);
			}
		}
	}

	public class MSB_Icon
	{
		private const byte
			PROPERTY_LAYOUT = 0x3A,
			PROPERTY_BINDING = 0x28;

		public MSB_Layout Layout;
		public string[] Bindings;

		public static MSB_Icon Read(LRBinaryReader p_reader)
		{
			MSB_Icon val = new MSB_Icon();
			List<string> bindings = new List<string>();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_LAYOUT:
						val.Layout = p_reader.ReadStruct<MSB_Layout>(MSB_Layout.Read);
						break;
					case PROPERTY_BINDING:
						bindings.Add(p_reader.ReadStringWithHeader());
						while (p_reader.Next(Token.String))
							bindings.Add(p_reader.ReadStringWithHeader());
						break;
					default:
						throw new UnexpectedPropertyException(
							propId,
							p_reader.BaseStream.Position - 1
						);
				}
			}
			if (bindings.Count > 0)
				val.Bindings = bindings.ToArray();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, MSB_Icon p_value)
		{
			if (p_value.Layout != null)
			{
				p_writer.WriteByte(0x3A);
				p_writer.WriteStruct<MSB_Layout>(MSB_Layout.Write, p_value.Layout);
			}

			if (p_value.Bindings != null)
			{
				for (int i = 0; i < p_value.Bindings.Length; i++)
				{
					p_writer.WriteByte(0x28);
					p_writer.WriteStringWithHeader(p_value.Bindings[i]);
				}
			}
		}
	}

	public class MSB_Scene
	{
		private const byte
			PROPERTY_LAYOUT = 0x3A,
			PROPERTY_COUNT = 0x2F,
			PROPERTY_INDICES = 0x31,
			PROPERTY_RECTS = 0x30,
			PROPERTY_BINDING = 0x28;

		public MSB_Layout Layout;
		public int Count;
		public int[] Indices;
		public int[] Rects;
		public string Binding;

		public static MSB_Scene Read(LRBinaryReader p_reader)
		{
			MSB_Scene val = new MSB_Scene();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_LAYOUT:
						val.Layout = p_reader.ReadStruct<MSB_Layout>(MSB_Layout.Read);
						break;
					case PROPERTY_COUNT:
						val.Count = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_INDICES:
					{
						List<int> indices = new List<int>();
						for (int i = 0; i < val.Count; i++)
							indices.Add(p_reader.ReadIntWithHeader());
						val.Indices = indices.ToArray();
						break;
					}
					case PROPERTY_RECTS:
					{
						List<int> rects = new List<int>();
						for (int i = 0; i < val.Count * 4; i++)
							rects.Add(p_reader.ReadIntWithHeader());
						val.Rects = rects.ToArray();
						break;
					}
					case PROPERTY_BINDING:
						val.Binding = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MSB_Scene p_value)
		{
			if (p_value.Layout != null)
			{
				p_writer.WriteByte(0x3A);
				p_writer.WriteStruct<MSB_Layout>(MSB_Layout.Write, p_value.Layout);
			}

			p_writer.WriteByte(0x2F);
			p_writer.WriteIntWithHeader(p_value.Count);

			if (p_value.Indices != null)
			{
				p_writer.WriteByte(0x31);
				for (int i = 0; i < p_value.Indices.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Indices[i]);
			}

			if (p_value.Rects != null)
			{
				p_writer.WriteByte(0x30);
				for (int i = 0; i < p_value.Rects.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Rects[i]);
			}

			if (p_value.Binding != null)
			{
				p_writer.WriteByte(0x28);
				p_writer.WriteStringWithHeader(p_value.Binding);
			}
		}
	}

	public class MSB_PieceSet
	{
		private const byte
			PROPERTY_DIMENSIONS = 0x2B,
			PROPERTY_SCALE = 0x2D,
			PROPERTY_PARAMS = 0x2F;

		public int[] Dimensions;
		public int Scale;
		public int Param1;
		public int Param2;

		public static MSB_PieceSet Read(LRBinaryReader p_reader)
		{
			MSB_PieceSet val = new MSB_PieceSet();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_DIMENSIONS:
					{
						List<int> dims = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							dims.Add(p_reader.ReadIntWithHeader());
						}
						val.Dimensions = dims.ToArray();
						break;
					}
					case PROPERTY_SCALE:
						val.Scale = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_PARAMS:
						val.Param1 = p_reader.ReadIntWithHeader();
						val.Param2 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MSB_PieceSet p_value)
		{
			if (p_value.Dimensions != null)
			{
				p_writer.WriteByte(0x2B);
				for (int i = 0; i < p_value.Dimensions.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Dimensions[i]);
			}

			p_writer.WriteByte(0x2D);
			p_writer.WriteIntWithHeader(p_value.Scale);

			p_writer.WriteByte(0x2F);
			p_writer.WriteIntWithHeader(p_value.Param1);
			p_writer.WriteIntWithHeader(p_value.Param2);
		}
	}

	public class MSB_PartContainer
	{
		private const byte
			PROPERTY_LAYOUT = 0x3A,
			PROPERTY_CLEARBOX = 0x34,
			PROPERTY_ARROWS = 0x35,
			PROPERTY_COLORS = 0x2A;

		public MSB_Layout Layout;
		public string Clearbox;
		public string LeftArrow;
		public string RightArrow;
		public int[] Colors;

		public static MSB_PartContainer Read(LRBinaryReader p_reader)
		{
			MSB_PartContainer val = new MSB_PartContainer();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_LAYOUT:
						val.Layout = p_reader.ReadStruct<MSB_Layout>(MSB_Layout.Read);
						break;
					case PROPERTY_CLEARBOX:
						val.Clearbox = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_ARROWS:
						val.LeftArrow = p_reader.ReadStringWithHeader();
						val.RightArrow = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_COLORS:
					{
						List<int> colors = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							colors.Add(p_reader.ReadIntWithHeader());
						}
						val.Colors = colors.ToArray();
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

		public static void Write(LRBinaryWriter p_writer, MSB_PartContainer p_value)
		{
			if (p_value.Layout != null)
			{
				p_writer.WriteByte(0x3A);
				p_writer.WriteStruct<MSB_Layout>(MSB_Layout.Write, p_value.Layout);
			}

			if (p_value.Clearbox != null)
			{
				p_writer.WriteByte(0x34);
				p_writer.WriteStringWithHeader(p_value.Clearbox);
			}

			if (p_value.LeftArrow != null)
			{
				p_writer.WriteByte(0x35);
				p_writer.WriteStringWithHeader(p_value.LeftArrow);
				p_writer.WriteStringWithHeader(p_value.RightArrow);
			}

			if (p_value.Colors != null)
			{
				p_writer.WriteByte(0x2A);
				for (int i = 0; i < p_value.Colors.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Colors[i]);
			}
		}
	}

	public class MSB_TextEdit
	{
		private const byte
			PROPERTY_DIMENSIONS = 0x2B,
			PROPERTY_OFFSETS = 0x2C,
			PROPERTY_SCALE = 0x2D;

		public string Name;
		public int[] Dimensions;
		public int[] Offsets;
		public bool HasScale;
		public int Scale;

		public static MSB_TextEdit Read(LRBinaryReader p_reader)
		{
			MSB_TextEdit val = new MSB_TextEdit();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_DIMENSIONS:
					{
						List<int> dims = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							dims.Add(p_reader.ReadIntWithHeader());
						}
						val.Dimensions = dims.ToArray();
						break;
					}
					case PROPERTY_OFFSETS:
					{
						List<int> offs = new List<int>();
						while (p_reader.Next(Token.Int32))
						{
							offs.Add(p_reader.ReadIntWithHeader());
						}
						val.Offsets = offs.ToArray();
						break;
					}
					case PROPERTY_SCALE:
						val.HasScale = true;
						val.Scale = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MSB_TextEdit p_value)
		{
			if (p_value.Dimensions != null)
			{
				p_writer.WriteByte(0x2B);
				for (int i = 0; i < p_value.Dimensions.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Dimensions[i]);
			}

			if (p_value.Offsets != null)
			{
				p_writer.WriteByte(0x2C);
				for (int i = 0; i < p_value.Offsets.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Offsets[i]);
			}

			if (p_value.HasScale)
			{
				p_writer.WriteByte(0x2D);
				p_writer.WriteIntWithHeader(p_value.Scale);
			}
		}
	}

	public class MSB_TextStyle
	{
		private const byte
			PROPERTY_FONT = 0x29,
			PROPERTY_COLOR = 0x2A,
			PROPERTY_PARAM = 0x2F;

		public string Font;
		public int[] Color;
		public bool HasParam;
		public int Param;

		public static MSB_TextStyle Read(LRBinaryReader p_reader)
		{
			MSB_TextStyle val = new MSB_TextStyle();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_FONT:
						val.Font = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_COLOR:
					{
						List<int> colors = new List<int>();
						while (p_reader.Next(Token.Int32))
							colors.Add(p_reader.ReadIntWithHeader());
						val.Color = colors.ToArray();
						break;
					}
					case PROPERTY_PARAM:
						val.HasParam = true;
						val.Param = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MSB_TextStyle p_value)
		{
			if (p_value.Font != null)
			{
				p_writer.WriteByte(0x29);
				p_writer.WriteStringWithHeader(p_value.Font);
			}

			if (p_value.Color != null)
			{
				p_writer.WriteByte(0x2A);
				for (int i = 0; i < p_value.Color.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Color[i]);
			}

			if (p_value.HasParam)
			{
				p_writer.WriteByte(0x2F);
				p_writer.WriteIntWithHeader(p_value.Param);
			}
		}
	}

	public class MSB_ImageBox
	{
		private const byte
			PROPERTY_BINDINGS = 0x28,
			PROPERTY_COLORS = 0x2A;

		public string[] Bindings;
		public int[] Colors;

		public static MSB_ImageBox Read(LRBinaryReader p_reader)
		{
			MSB_ImageBox val = new MSB_ImageBox();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_BINDINGS:
					{
						List<string> bindings = new List<string>();
						while (p_reader.Next(Token.String))
							bindings.Add(p_reader.ReadStringWithHeader());
						val.Bindings = bindings.ToArray();
						break;
					}
					case PROPERTY_COLORS:
					{
						List<int> colors = new List<int>();
						while (p_reader.Next(Token.Int32))
							colors.Add(p_reader.ReadIntWithHeader());
						val.Colors = colors.ToArray();
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

		public static void Write(LRBinaryWriter p_writer, MSB_ImageBox p_value)
		{
			if (p_value.Bindings != null)
			{
				p_writer.WriteByte(0x28);
				for (int i = 0; i < p_value.Bindings.Length; i++)
					p_writer.WriteStringWithHeader(p_value.Bindings[i]);
			}

			if (p_value.Colors != null)
			{
				p_writer.WriteByte(0x2A);
				for (int i = 0; i < p_value.Colors.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Colors[i]);
			}
		}
	}

	public class MSB_Button
	{
		private const byte
			PROPERTY_LAYOUT = 0x3A,
			PROPERTY_BINDINGS = 0x28,
			PROPERTY_FONTS = 0x29,
			PROPERTY_ALIGNMENT = 0x2E,
			PROPERTY_COLORS = 0x2A;

		public MSB_Layout Layout;
		public string[] Bindings;
		public string[] Fonts;
		public bool HasAlignment;
		public int Alignment;
		public int[] Colors;

		public static MSB_Button Read(LRBinaryReader p_reader)
		{
			MSB_Button val = new MSB_Button();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_LAYOUT:
						val.Layout = p_reader.ReadStruct<MSB_Layout>(MSB_Layout.Read);
						break;
					case PROPERTY_BINDINGS:
					{
						List<string> bindings = new List<string>();
						while (p_reader.Next(Token.String))
							bindings.Add(p_reader.ReadStringWithHeader());
						val.Bindings = bindings.ToArray();
						break;
					}
					case PROPERTY_FONTS:
					{
						List<string> fonts = new List<string>();
						while (p_reader.Next(Token.String))
							fonts.Add(p_reader.ReadStringWithHeader());
						val.Fonts = fonts.ToArray();
						break;
					}
					case PROPERTY_ALIGNMENT:
						val.HasAlignment = true;
						val.Alignment = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_COLORS:
					{
						List<int> colors = new List<int>();
						while (p_reader.Next(Token.Int32))
							colors.Add(p_reader.ReadIntWithHeader());
						val.Colors = colors.ToArray();
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

		public static void Write(LRBinaryWriter p_writer, MSB_Button p_value)
		{
			if (p_value.Layout != null)
			{
				p_writer.WriteByte(0x3A);
				p_writer.WriteStruct<MSB_Layout>(MSB_Layout.Write, p_value.Layout);
			}

			if (p_value.Bindings != null)
			{
				p_writer.WriteByte(0x28);
				for (int i = 0; i < p_value.Bindings.Length; i++)
					p_writer.WriteStringWithHeader(p_value.Bindings[i]);
			}

			if (p_value.Fonts != null)
			{
				p_writer.WriteByte(0x29);
				for (int i = 0; i < p_value.Fonts.Length; i++)
					p_writer.WriteStringWithHeader(p_value.Fonts[i]);
			}

			if (p_value.HasAlignment)
			{
				p_writer.WriteByte(0x2E);
				p_writer.WriteIntWithHeader(p_value.Alignment);
			}

			if (p_value.Colors != null)
			{
				p_writer.WriteByte(0x2A);
				for (int i = 0; i < p_value.Colors.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Colors[i]);
			}
		}
	}

	public class MSB_Slider
	{
		private const byte
			PROPERTY_LAYOUT = 0x3A,
			PROPERTY_DIMENSIONS = 0x2B,
			PROPERTY_IMAGES = 0x32,
			PROPERTY_ARROWS = 0x35;

		public MSB_Layout Layout;
		public int[] Dimensions;
		public string[] Images;
		public string[] Arrows;

		public static MSB_Slider Read(LRBinaryReader p_reader)
		{
			MSB_Slider val = new MSB_Slider();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_LAYOUT:
						val.Layout = p_reader.ReadStruct<MSB_Layout>(MSB_Layout.Read);
						break;
					case PROPERTY_DIMENSIONS:
					{
						List<int> dims = new List<int>();
						while (p_reader.Next(Token.Int32))
							dims.Add(p_reader.ReadIntWithHeader());
						val.Dimensions = dims.ToArray();
						break;
					}
					case PROPERTY_IMAGES:
					{
						List<string> images = new List<string>();
						while (p_reader.Next(Token.String))
							images.Add(p_reader.ReadStringWithHeader());
						val.Images = images.ToArray();
						break;
					}
					case PROPERTY_ARROWS:
					{
						List<string> arrows = new List<string>();
						while (p_reader.Next(Token.String))
							arrows.Add(p_reader.ReadStringWithHeader());
						val.Arrows = arrows.ToArray();
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

		public static void Write(LRBinaryWriter p_writer, MSB_Slider p_value)
		{
			if (p_value.Layout != null)
			{
				p_writer.WriteByte(0x3A);
				p_writer.WriteStruct<MSB_Layout>(MSB_Layout.Write, p_value.Layout);
			}

			if (p_value.Dimensions != null)
			{
				p_writer.WriteByte(0x2B);
				for (int i = 0; i < p_value.Dimensions.Length; i++)
					p_writer.WriteIntWithHeader(p_value.Dimensions[i]);
			}

			if (p_value.Images != null)
			{
				p_writer.WriteByte(0x32);
				for (int i = 0; i < p_value.Images.Length; i++)
					p_writer.WriteStringWithHeader(p_value.Images[i]);
			}

			if (p_value.Arrows != null)
			{
				p_writer.WriteByte(0x35);
				for (int i = 0; i < p_value.Arrows.Length; i++)
					p_writer.WriteStringWithHeader(p_value.Arrows[i]);
			}
		}
	}
}
