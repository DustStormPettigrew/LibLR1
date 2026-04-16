using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Menu item format.
	/// </summary>
	public class MIB
	{
		private const byte
			ID_NUM_ITEMS = 0x27,
			ID_ITEM_37 = 0x37,
			ID_ITEM_IMAGE = 0x38,
			ID_ITEM_TEXT = 0x39,
			ID_ITEM_3A = 0x3A,
			ID_ITEM_3B = 0x3B,
			ID_ITEM_3D = 0x3D,
			ID_ITEM_3E = 0x3E,
			ID_ITEM_3F = 0x3F,
			ID_ITEM_40 = 0x40,
			ID_ITEM_SCENE_VIEW_42 = 0x42,
			ID_ITEM_43 = 0x43,
			ID_ITEM_SCENE_VIEW_45 = 0x45,
			ID_ITEM_TEXT_BUTTON_46 = 0x46,
			PROPERTY_SCENE_NAME = 0x2D,
			PROPERTY_RECT = 0x2F,
			PROPERTY_PARENT = 0x31,
			PROPERTY_UNKNOWN_32 = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		private Dictionary<string, MIB_Item_37> m_items37;
		private Dictionary<string, MIB_ImageItem_38> m_imageItems;
		private Dictionary<string, MIB_TextItem_39> m_textItems;
		private Dictionary<string, MIB_Item_3A> m_items3A;
		private Dictionary<string, MIB_Item_3B> m_items3B;
		private Dictionary<string, MIB_Item_3D> m_items3D;
		private Dictionary<string, MIB_Item_3E> m_items3E;
		private Dictionary<string, MIB_Item_3F> m_items3F;
		private Dictionary<string, MIB_Item_40> m_items40;
		private Dictionary<string, MIB_SceneItem_42> m_scene42Items;
		private Dictionary<string, MIB_TextButtonItem_46> m_items43;
		private Dictionary<string, MIB_SceneItem_45> m_scene45Items;
		private Dictionary<string, MIB_TextButtonItem_46> m_textButtonItems;

		public int NumItems
		{
			get
			{
				return m_imageItems.Count
				     + m_items37.Count
				     + m_scene42Items.Count
				     + m_scene45Items.Count
				     + m_textButtonItems.Count
				     + m_textItems.Count
				     + m_items3A.Count
				     + m_items3B.Count
				     + m_items3D.Count
				     + m_items3E.Count
				     + m_items3F.Count
				     + m_items40.Count
				     + m_items43.Count;
			}
		}
		public Dictionary<string, MIB_Item_37> Items37
		{
			get { return m_items37; }
			set { m_items37 = value; }
		}
		public Dictionary<string, MIB_Item_3A> Items3A
		{
			get { return m_items3A; }
			set { m_items3A = value; }
		}
		public Dictionary<string, MIB_Item_3B> Items3B
		{
			get { return m_items3B; }
			set { m_items3B = value; }
		}
		public Dictionary<string, MIB_Item_3D> Items3D
		{
			get { return m_items3D; }
			set { m_items3D = value; }
		}
		public Dictionary<string, MIB_Item_3E> Items3E
		{
			get { return m_items3E; }
			set { m_items3E = value; }
		}
		public Dictionary<string, MIB_Item_3F> Items3F
		{
			get { return m_items3F; }
			set { m_items3F = value; }
		}
		public Dictionary<string, MIB_Item_40> Items40
		{
			get { return m_items40; }
			set { m_items40 = value; }
		}
		public Dictionary<string, MIB_TextButtonItem_46> Items43
		{
			get { return m_items43; }
			set { m_items43 = value; }
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
			m_items37 = new Dictionary<string, MIB_Item_37>();
			m_imageItems = new Dictionary<string, MIB_ImageItem_38>();
			m_textItems = new Dictionary<string, MIB_TextItem_39>();
			m_items3A = new Dictionary<string, MIB_Item_3A>();
			m_items3B = new Dictionary<string, MIB_Item_3B>();
			m_items3D = new Dictionary<string, MIB_Item_3D>();
			m_items3E = new Dictionary<string, MIB_Item_3E>();
			m_items3F = new Dictionary<string, MIB_Item_3F>();
			m_items40 = new Dictionary<string, MIB_Item_40>();
			m_scene42Items = new Dictionary<string, MIB_SceneItem_42>();
			m_items43 = new Dictionary<string, MIB_TextButtonItem_46>();
			m_scene45Items = new Dictionary<string, MIB_SceneItem_45>();
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
					case ID_ITEM_37:
					{
						m_items37 = p_reader.ReadDictionaryBlock<MIB_Item_37>(
							MIB_Item_37.Read,
							ID_ITEM_37
						);
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
					case ID_ITEM_3A:
					{
						m_items3A = p_reader.ReadDictionaryBlock<MIB_Item_3A>(
							MIB_Item_3A.Read,
							ID_ITEM_3A
						);
						break;
					}
					case ID_ITEM_3B:
					{
						m_items3B = p_reader.ReadDictionaryBlock<MIB_Item_3B>(
							MIB_Item_3B.Read,
							ID_ITEM_3B
						);
						break;
					}
					case ID_ITEM_3D:
					{
						m_items3D = p_reader.ReadDictionaryBlock<MIB_Item_3D>(
							MIB_Item_3D.Read,
							ID_ITEM_3D
						);
						break;
					}
					case ID_ITEM_3E:
					{
						m_items3E = p_reader.ReadDictionaryBlock<MIB_Item_3E>(
							MIB_Item_3E.Read,
							ID_ITEM_3E
						);
						break;
					}
					case ID_ITEM_3F:
					{
						m_items3F = p_reader.ReadDictionaryBlock<MIB_Item_3F>(
							MIB_Item_3F.Read,
							ID_ITEM_3F
						);
						break;
					}
					case ID_ITEM_40:
					{
						m_items40 = p_reader.ReadDictionaryBlock<MIB_Item_40>(
							MIB_Item_40.Read,
							ID_ITEM_40
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
					case ID_ITEM_43:
					{
						m_items43 = p_reader.ReadDictionaryBlock<MIB_TextButtonItem_46>(
							MIB_TextButtonItem_46.Read,
							ID_ITEM_43
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
			if (m_items37.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_37);
				p_writer.WriteDictionaryBlock<MIB_Item_37>(
					MIB_Item_37.Write,
					m_items37,
					ID_ITEM_37
				);
			}
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
			if (m_items3A.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_3A);
				p_writer.WriteDictionaryBlock<MIB_Item_3A>(
					MIB_Item_3A.Write,
					m_items3A,
					ID_ITEM_3A
				);
			}
			if (m_items3B.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_3B);
				p_writer.WriteDictionaryBlock<MIB_Item_3B>(
					MIB_Item_3B.Write,
					m_items3B,
					ID_ITEM_3B
				);
			}
			if (m_items3D.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_3D);
				p_writer.WriteDictionaryBlock<MIB_Item_3D>(
					MIB_Item_3D.Write,
					m_items3D,
					ID_ITEM_3D
				);
			}
			if (m_items3E.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_3E);
				p_writer.WriteDictionaryBlock<MIB_Item_3E>(
					MIB_Item_3E.Write,
					m_items3E,
					ID_ITEM_3E
				);
			}
			if (m_items3F.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_3F);
				p_writer.WriteDictionaryBlock<MIB_Item_3F>(
					MIB_Item_3F.Write,
					m_items3F,
					ID_ITEM_3F
				);
			}
			if (m_items40.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_40);
				p_writer.WriteDictionaryBlock<MIB_Item_40>(
					MIB_Item_40.Write,
					m_items40,
					ID_ITEM_40
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
			if (m_items43.Count > 0)
			{
				p_writer.WriteByte(ID_ITEM_43);
				p_writer.WriteDictionaryBlock<MIB_TextButtonItem_46>(
					MIB_TextButtonItem_46.Write,
					m_items43,
					ID_ITEM_43
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
			if (m_items37.ContainsKey(key))
			{
				return m_items37[key];
			}
			if (m_imageItems.ContainsKey(key))
			{
				return m_imageItems[key];
			}
			if (m_textItems.ContainsKey(key))
			{
				return m_textItems[key];
			}
			if (m_items3A.ContainsKey(key))
			{
				return m_items3A[key];
			}
			if (m_items3B.ContainsKey(key))
			{
				return m_items3B[key];
			}
			if (m_items3D.ContainsKey(key))
			{
				return m_items3D[key];
			}
			if (m_items3E.ContainsKey(key))
			{
				return m_items3E[key];
			}
			if (m_items3F.ContainsKey(key))
			{
				return m_items3F[key];
			}
			if (m_items40.ContainsKey(key))
			{
				return m_items40[key];
			}
			if (m_scene42Items.ContainsKey(key))
			{
				return m_scene42Items[key];
			}
			if (m_items43.ContainsKey(key))
			{
				return m_items43[key];
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

		public virtual bool TryGetParent(out string p_parentItem)
		{
			if (Position != null && Position.HasParent)
			{
				p_parentItem = Position.ParentItem;
				return true;
			}

			p_parentItem = null;
			return false;
		}
	}

	internal static class MIB_Helper
	{
		public static List<string> ReadStringList(LRBinaryReader p_reader)
		{
			List<string> output = new List<string>();
			while (p_reader.Next(Token.String))
			{
				output.Add(p_reader.ReadStringWithHeader());
			}

			return output;
		}

		public static List<int> ReadIntList(LRBinaryReader p_reader)
		{
			List<int> output = new List<int>();
			while (p_reader.Next(Token.Int32))
			{
				output.Add(p_reader.ReadIntWithHeader());
			}

			return output;
		}

		public static List<float> ReadFloatList(LRBinaryReader p_reader)
		{
			List<float> output = new List<float>();
			while (p_reader.Next(Token.Float))
			{
				output.Add(p_reader.ReadFloatWithHeader());
			}

			return output;
		}

		public static void WriteStringList(LRBinaryWriter p_writer, List<string> p_values)
		{
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writer.WriteStringWithHeader(p_values[i]);
			}
		}

		public static void WriteIntList(LRBinaryWriter p_writer, List<int> p_values)
		{
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writer.WriteIntWithHeader(p_values[i]);
			}
		}

		public static void WriteFloatList(LRBinaryWriter p_writer, List<float> p_values)
		{
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writer.WriteFloatWithHeader(p_values[i]);
			}
		}
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

	public class MIB_Item_37 : MIB_Item
	{
		private const byte
			PROPERTY_RECT = 0x2F,
			PROPERTY_PARENT = 0x31,
			PROPERTY_UNKNOWN_2A = 0x2A;

		public MIB_Rect Rect;
		public bool HasParent;
		public string ParentItem;
		public List<int> Unknown2A = new List<int>();

		public override bool TryGetParent(out string p_parentItem)
		{
			if (HasParent)
			{
				p_parentItem = ParentItem;
				return true;
			}

			return base.TryGetParent(out p_parentItem);
		}

		public static MIB_Item_37 Read(LRBinaryReader p_reader)
		{
			MIB_Item_37 val = new MIB_Item_37();
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
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = MIB_Helper.ReadIntList(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_37 p_value)
		{
			p_writer.WriteByte(PROPERTY_RECT);
			MIB_Rect.Write(p_writer, p_value.Rect);
			if (p_value.Unknown2A.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2A);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2A);
			}
			if (p_value.HasParent)
			{
				p_writer.WriteByte(PROPERTY_PARENT);
				p_writer.WriteStringWithHeader(p_value.ParentItem);
			}
		}
	}

	public class MIB_Item_3A : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_2A = 0x2A,
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public List<string> Unknown28 = new List<string>();
		public List<int> Unknown2A = new List<int>();
		public List<int> Unknown2B = new List<int>();
		public int? Unknown33;

		public static MIB_Item_3A Read(LRBinaryReader p_reader)
		{
			MIB_Item_3A val = new MIB_Item_3A();
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
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2A:
					{
						val.Unknown2A = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown33 = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3A p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown28.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_28);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown28);
			}
			if (p_value.Unknown33.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader(p_value.Unknown33.Value);
			}
			if (p_value.Unknown2B.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2B);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2B);
			}
			if (p_value.Unknown2A.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2A);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2A);
			}
		}
	}

	public class MIB_Item_3B : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_UNKNOWN_2C = 0x2C,
			PROPERTY_UNKNOWN_32 = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public List<string> Unknown28 = new List<string>();
		public List<int> Unknown2B = new List<int>();
		public int? Unknown2C;
		public int? Unknown32;
		public int? Unknown33;

		public static MIB_Item_3B Read(LRBinaryReader p_reader)
		{
			MIB_Item_3B val = new MIB_Item_3B();
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
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.Unknown2C = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3B p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown32.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_32);
				p_writer.WriteIntWithHeader(p_value.Unknown32.Value);
			}
			if (p_value.Unknown33.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader(p_value.Unknown33.Value);
			}
			if (p_value.Unknown2B.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2B);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2B);
			}
			if (p_value.Unknown28.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_28);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown28);
			}
			if (p_value.Unknown2C.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2C);
				p_writer.WriteIntWithHeader(p_value.Unknown2C.Value);
			}
		}
	}

	public class MIB_Item_3D : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_3B = 0x3B;

		public MIB_Item_3B Unknown3B;
		public List<string> Unknown28 = new List<string>();

		public override bool TryGetParent(out string p_parentItem)
		{
			if (Unknown3B != null && Unknown3B.TryGetParent(out p_parentItem))
			{
				return true;
			}

			return base.TryGetParent(out p_parentItem);
		}

		public static MIB_Item_3D Read(LRBinaryReader p_reader)
		{
			MIB_Item_3D val = new MIB_Item_3D();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_3B:
					{
						val.Unknown3B = p_reader.ReadStruct<MIB_Item_3B>(
							MIB_Item_3B.Read
						);
						break;
					}
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = MIB_Helper.ReadStringList(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3D p_value)
		{
			if (p_value.Unknown3B != null)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3B);
				p_writer.WriteStruct<MIB_Item_3B>(
					MIB_Item_3B.Write,
					p_value.Unknown3B
				);
			}
			if (p_value.Unknown28.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_28);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown28);
			}
		}
	}

	public class MIB_Item_3E : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_2C = 0x2C,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_UNKNOWN_3A = 0x3A,
			PROPERTY_UNKNOWN_3B = 0x3B,
			PROPERTY_POSITION = 0x36;

		public int? Unknown2C;
		public List<string> Unknown3A = new List<string>();
		public List<string> Unknown3B = new List<string>();
		public List<int> Unknown33 = new List<int>();

		public static MIB_Item_3E Read(LRBinaryReader p_reader)
		{
			MIB_Item_3E val = new MIB_Item_3E();
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
					case PROPERTY_UNKNOWN_3A:
					{
						val.Unknown3A = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_3B:
					{
						val.Unknown3B = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown33 = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2C:
					{
						val.Unknown2C = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3E p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown3A.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3A);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown3A);
			}
			if (p_value.Unknown3B.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3B);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown3B);
			}
			if (p_value.Unknown33.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown33);
			}
			if (p_value.Unknown2C.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2C);
				p_writer.WriteIntWithHeader(p_value.Unknown2C.Value);
			}
		}
	}

	public class MIB_Item_40 : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_UNKNOWN_38 = 0x38,
			PROPERTY_UNKNOWN_3B = 0x3B,
			PROPERTY_POSITION = 0x36;

		public List<string> Unknown38 = new List<string>();
		public List<string> Unknown3B = new List<string>();
		public List<int> Unknown33 = new List<int>();

		public static MIB_Item_40 Read(LRBinaryReader p_reader)
		{
			MIB_Item_40 val = new MIB_Item_40();
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
					case PROPERTY_UNKNOWN_38:
					{
						val.Unknown38 = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_3B:
					{
						val.Unknown3B = MIB_Helper.ReadStringList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_33:
					{
						val.Unknown33 = MIB_Helper.ReadIntList(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_40 p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown3B.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3B);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown3B);
			}
			if (p_value.Unknown38.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_38);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown38);
			}
			if (p_value.Unknown33.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown33);
			}
		}
	}

	public class MIB_Item_3F_Sub
	{
		private const byte
			PROPERTY_UNKNOWN_2E = 0x2E,
			PROPERTY_UNKNOWN_2F = 0x2F;

		public List<int> Unknown2F = new List<int>();
		public List<float> Unknown2E = new List<float>();

		public static MIB_Item_3F_Sub Read(LRBinaryReader p_reader)
		{
			MIB_Item_3F_Sub val = new MIB_Item_3F_Sub();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_2F:
					{
						val.Unknown2F = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2E:
					{
						val.Unknown2E = MIB_Helper.ReadFloatList(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3F_Sub p_value)
		{
			if (p_value.Unknown2F.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2F);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2F);
			}
			if (p_value.Unknown2E.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2E);
				MIB_Helper.WriteFloatList(p_writer, p_value.Unknown2E);
			}
		}
	}

	public class MIB_Item_3F : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_2E = 0x2E,
			PROPERTY_UNKNOWN_2F = 0x2F,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public List<int> Unknown2F = new List<int>();
		public List<float> Unknown2E = new List<float>();
		public List<int> Unknown33Ints = new List<int>();
		public List<float> Unknown33Floats = new List<float>();

		public static MIB_Item_3F Read(LRBinaryReader p_reader)
		{
			MIB_Item_3F val = new MIB_Item_3F();
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
						val.Unknown33Ints = MIB_Helper.ReadIntList(p_reader);
						val.Unknown33Floats = MIB_Helper.ReadFloatList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2F:
					{
						val.Unknown2F = MIB_Helper.ReadIntList(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_2E:
					{
						val.Unknown2E = MIB_Helper.ReadFloatList(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, MIB_Item_3F p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION);
			p_writer.WriteStruct<MIB_Position>(
				MIB_Position.Write,
				p_value.Position
			);
			if (p_value.Unknown33Ints.Count > 0 || p_value.Unknown33Floats.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown33Ints);
				MIB_Helper.WriteFloatList(p_writer, p_value.Unknown33Floats);
			}
			if (p_value.Unknown2F.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2F);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown2F);
			}
			if (p_value.Unknown2E.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2E);
				MIB_Helper.WriteFloatList(p_writer, p_value.Unknown2E);
			}
		}
	}

	public class MIB_ImageItem_38 : MIB_Item
	{
		private const byte
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_POSITION = 0x36;

		public List<string> Unknown28 = new List<string>();

		public static MIB_ImageItem_38 Read(LRBinaryReader p_reader)
		{
			MIB_ImageItem_38 val = new MIB_ImageItem_38();
			while (!p_reader.Next( Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28 = MIB_Helper.ReadStringList(p_reader);
						break;
					}
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
			if (p_value.Unknown28.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_28);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown28);
			}
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
			PROPERTY_UNKNOWN_3A = 0x3A,
			PROPERTY_POSITION = 0x36;

		public bool HasSceneName;
		public string SceneName;
		public float[] Unknown2E;
		public bool HasUnknown33;
		public int Unknown33_0, Unknown33_1;
		public List<string> Unknown3A = new List<string>();

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
					case PROPERTY_UNKNOWN_3A:
					{
						val.Unknown3A = MIB_Helper.ReadStringList(p_reader);
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
			if (p_value.Unknown3A.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_3A);
				MIB_Helper.WriteStringList(p_writer, p_value.Unknown3A);
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
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_UNKNOWN_32 = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_POSITION = 0x36;

		public List<int> Unknown_2B = new List<int>();
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
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown_2B = MIB_Helper.ReadIntList(p_reader);
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
			if (p_value.Unknown_2B.Count > 0)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_2B);
				MIB_Helper.WriteIntList(p_writer, p_value.Unknown_2B);
			}
			if (p_value.Unknown_33.HasValue)
			{
				p_writer.WriteByte(PROPERTY_UNKNOWN_33);
				p_writer.WriteIntWithHeader(p_value.Unknown_33.Value);
			}
		}
	}
}
