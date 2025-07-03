using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	public class MIB
	{
		private const byte
			ID_NUM_ITEMS           = 0x27,
			ID_ITEM_IMAGE          = 0x38,
			ID_ITEM_TEXT           = 0x39,
			ID_ITEM_SCENE_VIEW_42  = 0x42,
			ID_ITEM_SCENE_VIEW_45  = 0x45,
			ID_ITEM_TEXT_BUTTON_46 = 0x46,
			PROPERTY_SCENE_NAME    = 0x2D,
			PROPERTY_RECT          = 0x2F,
			PROPERTY_PARENT        = 0x31,
			PROPERTY_UNKNOWN_32    = 0x32,
			PROPERTY_UNKNOWN_33    = 0x33,
			PROPERTY_POSITION      = 0x36;

		private Dictionary<string, MIB_ImageItem_38>      m_imageItems;
		private Dictionary<string, MIB_TextItem_39>       m_textItems;
		private Dictionary<string, MIB_SceneItem_42>      m_scene42Items;
		private Dictionary<string, MIB_SceneItem_45>      m_scene45Items;
		private Dictionary<string, MIB_TextButtonItem_46> m_textButtonItems;

		public int NumItems
		{
			get
			{
				return m_imageItems.Count
				     + m_scene42Items.Count
				     + m_scene45Items.Count
				     + m_textButtonItems.Count
				     + m_textItems.Count;
			}
		}
		public Dictionary<string, MIB_ImageItem_38> ImageItems
		{
			get { return m_imageItems; }
			set { m_imageItems = value; }
		}
		public Dictionary<string, MIB_TextItem_39> TextItems
		{
			get { return m_textItems; }
			set { m_textItems = value; }
		}
		public Dictionary<string, MIB_SceneItem_42> Scene42Items
		{
			get { return m_scene42Items; }
			set { m_scene42Items = value; }
		}
		public Dictionary<string, MIB_SceneItem_45> Scene45Items
		{
			get { return m_scene45Items; }
			set { m_scene45Items = value; }
		}
		public Dictionary<string, MIB_TextButtonItem_46> TextButtonItems
		{
			get { return m_textButtonItems; }
			set { m_textButtonItems = value; }
		}
		
		public MIB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public MIB(LRBinaryReader p_reader)
		{
			m_imageItems      = new Dictionary<string, MIB_ImageItem_38>();
			m_textItems       = new Dictionary<string, MIB_TextItem_39>();
			m_scene42Items    = new Dictionary<string, MIB_SceneItem_42>();
			m_scene45Items    = new Dictionary<string, MIB_SceneItem_45>();
			m_textButtonItems = new Dictionary<string, MIB_TextButtonItem_46>();

			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_NUM_ITEMS:
					{
						/*m_NumItems = */
						p_reader.ReadIntWithHeader();
						break;
					}
					case ID_ITEM_IMAGE:
					{
						m_imageItems = p_reader.ReadDictionaryBlock<MIB_ImageItem_38>(
							MIB_ImageItem_38.Read,
							ID_ITEM_IMAGE
						);
						break;
					}
					case ID_ITEM_TEXT:
					{
						m_textItems = p_reader.ReadDictionaryBlock<MIB_TextItem_39>(
							MIB_TextItem_39.Read,
							ID_ITEM_TEXT
						);
						break;
					}
					case ID_ITEM_SCENE_VIEW_42:
					{
						m_scene42Items = p_reader.ReadDictionaryBlock<MIB_SceneItem_42>(
							MIB_SceneItem_42.Read,
							ID_ITEM_SCENE_VIEW_42
						);
						break;
					}
					case ID_ITEM_SCENE_VIEW_45:
					{
						m_scene45Items = p_reader.ReadDictionaryBlock<MIB_SceneItem_45>(
							MIB_SceneItem_45.Read,
							ID_ITEM_SCENE_VIEW_45
						);
						break;
					}
					case ID_ITEM_TEXT_BUTTON_46:
					{
						m_textButtonItems = p_reader.ReadDictionaryBlock<MIB_TextButtonItem_46>(
							MIB_TextButtonItem_46.Read,
							ID_ITEM_TEXT_BUTTON_46
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
			p_writer.WriteByte(ID_NUM_ITEMS);
			p_writer.WriteIntWithHeader( NumItems);
			if (m_imageItems.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_IMAGE);
				p_writer.WriteDictionaryBlock<MIB_ImageItem_38>(
					MIB_ImageItem_38.Write,
					m_imageItems,
					ID_ITEM_IMAGE
				);
			}
			if (m_textItems.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_TEXT);
				p_writer.WriteDictionaryBlock<MIB_TextItem_39>(
					MIB_TextItem_39.Write,
					m_textItems,
					ID_ITEM_TEXT
				);
			}
			if (m_scene42Items.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_SCENE_VIEW_42);
				p_writer.WriteDictionaryBlock<MIB_SceneItem_42>(
					MIB_SceneItem_42.Write,
					m_scene42Items,
					ID_ITEM_SCENE_VIEW_42
				);
			}
			if (m_scene45Items.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_SCENE_VIEW_45);
				p_writer.WriteDictionaryBlock<MIB_SceneItem_45>(
					MIB_SceneItem_45.Write,
					m_scene45Items,
					ID_ITEM_SCENE_VIEW_45
				);
			}
			if (m_textButtonItems.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_TEXT_BUTTON_46);
				p_writer.WriteDictionaryBlock<MIB_TextButtonItem_46>(
					MIB_TextButtonItem_46.Write,
					m_textButtonItems,
					ID_ITEM_TEXT_BUTTON_46
				);
			}
		}
		
		public MIB_Item GetItem(string key)
		{
			if (m_imageItems.ContainsKey(key))
			{
				return m_imageItems[key];
			}
			if (m_textItems.ContainsKey(key))
			{
				return m_textItems[key];
			}
			if (m_scene42Items.ContainsKey(key))
			{
				return m_scene42Items[key];
			}
			if (m_scene45Items.ContainsKey(key))
			{
				return m_scene45Items[key];
			}
			if (m_textButtonItems.ContainsKey(key))
			{
				return m_textButtonItems[key];
			}
			throw new Exception("Could not find item `" + key + "`.");
		}
	}

	public abstract class MIB_Item
	{
		public MIB_Position Position;
	}

	public class MIB_Rect
	{
		public int X1, Y1, X2, Y2;

		public static MIB_Rect Read(LRBinaryReader p_reader)
		{
			MIB_Rect val = new MIB_Rect();
			val.X1 = p_reader.ReadIntWithHeader();
			val.Y1 = p_reader.ReadIntWithHeader();
			val.X2 = p_reader.ReadIntWithHeader();
			val.Y2 = p_reader.ReadIntWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, MIB_Rect p_value)
		{
			p_writer.WriteIntWithHeader(p_value.X1);
			p_writer.WriteIntWithHeader(p_value.Y1);
			p_writer.WriteIntWithHeader(p_value.X2);
			p_writer.WriteIntWithHeader(p_value.Y2);
		}
	}

	public class MIB_Position
	{
		private const byte
			PROPERTY_RECT = 0x2F,
			PROPERTY_PARENT = 0x31,
			PROPERTY_UNKNOWN_32 = 0x32;

		public MIB_Rect Rect;
		public bool HasParent;
		public string ParentItem;
		public int? Unknown_32;

		public static MIB_Position Read(LRBinaryReader p_reader)
		{
			MIB_Position val = new MIB_Position();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_RECT:
					{
						val.Rect = MIB_Rect.Read(p_reader);
						break;
					}
					case PROPERTY_PARENT:
					{
						val.HasParent = true;
						val.ParentItem = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_32:
					{
						val.Unknown_32 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_Position p_value)
		{
			p_writer.WriteByte(PROPERTY_RECT);
			MIB_Rect.Write(p_writer, p_value.Rect);
			if (p_value.HasParent)
			{
				p_writer.WriteByte(PROPERTY_PARENT);
				p_writer.WriteStringWithHeader(p_value.ParentItem);
			}
			if (p_value.Unknown_32.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_32);
				p_writer.WriteIntWithHeader(p_value.Unknown_32.Value);
			}
		}
	}

	public class MIB_ImageItem_38 : MIB_Item
	{
		private const byte
			PROPERTY_POSITION = 0x36;

		public static MIB_ImageItem_38 Read(LRBinaryReader p_reader)
		{
			MIB_ImageItem_38 val = new MIB_ImageItem_38();
			while (!p_reader.Next( Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = p_reader.ReadStruct<MIB_Position>(
							MIB_Position.Read
						);
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

		public static void Write(LRBinaryWriter p_writer, MIB_ImageItem_38 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
		}
	}

	public class MIB_TextItem_39 : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public int? Unknown_33;

		public static MIB_TextItem_39 Read(LRBinaryReader p_reader)
		{
			MIB_TextItem_39 val = new MIB_TextItem_39();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = p_reader.ReadStruct<MIB_Position>(
							MIB_Position.Read
						);
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown_33 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_TextItem_39 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown_33.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader( p_value.Unknown_33.Value);
			}
		}
	}

	public class MIB_SceneItem_42 : MIB_Item
	{
		private const byte
			PROPERTY_SCENE_NAME = 0x2D,
			PROPERTY_UNKNOWN_2E = 0x2E,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public bool HasSceneName;
		public string SceneName;
		public float[] Unknown2E;
		public bool HasUnknown33;
		public int Unknown33_0, Unknown33_1;

		public static MIB_SceneItem_42 Read(LRBinaryReader p_reader)
		{
			MIB_SceneItem_42 val = new MIB_SceneItem_42();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = p_reader.ReadStruct<MIB_Position>(
							MIB_Position.Read
						);
						break;
					}
					case PROPERTY_SCENE_NAME:
					{
						val.HasSceneName = true;
						val.SceneName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2E:
					{
						val.Unknown2E = new float[9];
						for (int i = 0; i < 9; i++)
						{
							val.Unknown2E[i] = p_reader.ReadFloatWithHeader();
						}
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.HasUnknown33 = true;
						val.Unknown33_0 = p_reader.ReadIntWithHeader();
						val.Unknown33_1 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_SceneItem_42 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.HasSceneName)
			{
				p_writer.WriteByte(PROPERTY_SCENE_NAME);
				p_writer.WriteStringWithHeader( p_value.SceneName);
			}
			p_writer.WriteByte(PROPERTY_UNKNOWN_2E);
			for (int i = 0; i < 9; i++)
			{
				p_writer.WriteFloatWithHeader( p_value.Unknown2E[i]);
			}
			if (p_value.HasUnknown33)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader(p_value.Unknown33_0);
				p_writer.WriteIntWithHeader(p_value.Unknown33_1);
			}
		}
	}

	public class MIB_SceneItem_45 : MIB_Item
	{
		private const byte
			PROPERTY_SCENE_NAME = 0x2D,
			PROPERTY_POSITION = 0x36;

		public bool HasSceneName;
		public string SceneName;

		public static MIB_SceneItem_45 Read(LRBinaryReader p_reader)
		{
			MIB_SceneItem_45 val = new MIB_SceneItem_45();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = p_reader.ReadStruct<MIB_Position>(
							MIB_Position.Read
						);
						break;
					}
					case PROPERTY_SCENE_NAME:
					{
						val.HasSceneName = true;
						val.SceneName = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_SceneItem_45 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.HasSceneName)
			{
				p_writer.WriteByte(PROPERTY_SCENE_NAME);
				p_writer.WriteStringWithHeader(p_value.SceneName);
			}
		}
	}

	public class MIB_TextButtonItem_46 : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_32 = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION   = 0x36;

		public int? Unknown_32;
		public int? Unknown_33;

		public static MIB_TextButtonItem_46 Read(LRBinaryReader p_reader)
		{
			MIB_TextButtonItem_46 val = new MIB_TextButtonItem_46();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION:
					{
						val.Position = p_reader.ReadStruct<MIB_Position>(
							MIB_Position.Read
						);
						break;
					}
					case PROPERTY_UNKNOWN_32:
					{
						val.Unknown_32 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown_33 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_TextButtonItem_46 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown_32.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_32);
				p_writer.WriteIntWithHeader(p_value.Unknown_32.Value);
			}
			if (p_value.Unknown_33.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader(p_value.Unknown_33.Value);
			}
		}
	}
}