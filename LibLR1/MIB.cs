using System;
using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

namespace LibLR1 {
    public class MIB {
        private const byte
            ID_NUM_ITEMS = 0x27,
            ID_ITEM_IMAGE = 0x38,
            ID_ITEM_TEXT = 0x39,
            ID_ITEM_SCENE_VIEW_42 = 0x42,
            ID_ITEM_SCENE_VIEW_45 = 0x45,
            ID_ITEM_TEXT_BUTTON_46 = 0x46,
            PROPERTY_SCENE_NAME = 0x2D,
            PROPERTY_RECT = 0x2F,
            PROPERTY_PARENT = 0x31,
            PROPERTY_UNKNOWN_32 = 0x32,
            PROPERTY_UNKNOWN_33 = 0x33,
            PROPERTY_POSITION = 0x36;

        private Dictionary<string, MIB_ImageItem_38> m_ImageItems;
        private Dictionary<string, MIB_TextItem_39> m_TextItems;
        private Dictionary<string, MIB_SceneItem_42> m_Scene42Items;
        private Dictionary<string, MIB_SceneItem_45> m_Scene45Items;
        private Dictionary<string, MIB_TextButtonItem_46> m_TextButtonItems;

        public int NumItems {
            get { return m_ImageItems.Count + m_Scene42Items.Count + m_Scene45Items.Count + m_TextButtonItems.Count + m_TextItems.Count; }
        }
        public Dictionary<string, MIB_ImageItem_38> ImageItems { get { return m_ImageItems; } set { m_ImageItems = value; } }
        public Dictionary<string, MIB_TextItem_39> TextItems { get { return m_TextItems; } set { m_TextItems = value; } }
        public Dictionary<string, MIB_SceneItem_42> Scene42Items { get { return m_Scene42Items; } set { m_Scene42Items = value; } }
        public Dictionary<string, MIB_SceneItem_45> Scene45Items { get { return m_Scene45Items; } set { m_Scene45Items = value; } }
        public Dictionary<string, MIB_TextButtonItem_46> TextButtonItems { get { return m_TextButtonItems; } set { m_TextButtonItems = value; } }

        public MIB(Stream stream) {
            m_ImageItems = new Dictionary<string, MIB_ImageItem_38>();
            m_TextItems = new Dictionary<string, MIB_TextItem_39>();
            m_Scene42Items = new Dictionary<string, MIB_SceneItem_42>();
            m_Scene45Items = new Dictionary<string, MIB_SceneItem_45>();
            m_TextButtonItems = new Dictionary<string, MIB_TextButtonItem_46>();

            while (stream.Position < stream.Length) {
                byte block_id = BinaryFileHelper.ReadByte(stream);
                switch (block_id) {
                    case ID_NUM_ITEMS:
                        /*m_NumItems = */
                        BinaryFileHelper.ReadIntWithHeader(stream);
                        break;
                    case ID_ITEM_IMAGE:
                        m_ImageItems = BinaryFileHelper.ReadDictionaryBlock<MIB_ImageItem_38>(
                            stream,
                            new BinaryFileHelper.ReadObject<MIB_ImageItem_38>(
                                MIB_ImageItem_38.FromStream
                            ),
                            ID_ITEM_IMAGE
                        );
                        break;
                    case ID_ITEM_TEXT:
                        m_TextItems = BinaryFileHelper.ReadDictionaryBlock<MIB_TextItem_39>(
                            stream,
                            new BinaryFileHelper.ReadObject<MIB_TextItem_39>(
                                MIB_TextItem_39.FromStream
                            ),
                            ID_ITEM_TEXT
                        );
                        break;
                    case ID_ITEM_SCENE_VIEW_42:
                        m_Scene42Items = BinaryFileHelper.ReadDictionaryBlock<MIB_SceneItem_42>(
                            stream,
                            new BinaryFileHelper.ReadObject<MIB_SceneItem_42>(
                                MIB_SceneItem_42.FromStream
                            ),
                            ID_ITEM_SCENE_VIEW_42
                        );
                        break;
                    case ID_ITEM_SCENE_VIEW_45:
                        m_Scene45Items = BinaryFileHelper.ReadDictionaryBlock<MIB_SceneItem_45>(
                            stream,
                            new BinaryFileHelper.ReadObject<MIB_SceneItem_45>(
                                MIB_SceneItem_45.FromStream
                            ),
                            ID_ITEM_SCENE_VIEW_45
                        );
                        break;
                    case ID_ITEM_TEXT_BUTTON_46:
                        m_TextButtonItems = BinaryFileHelper.ReadDictionaryBlock<MIB_TextButtonItem_46>(
                            stream,
                            new BinaryFileHelper.ReadObject<MIB_TextButtonItem_46>(
                                MIB_TextButtonItem_46.FromStream
                            ),
                            ID_ITEM_TEXT_BUTTON_46
                        );
                        break;
                    default:
                        throw new UnexpectedBlockException(block_id, stream.Position - 1);
                }
            }
        }

        public MIB(string path, bool decompress = true)
            : this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read))) { }

        public void Save(Stream stream) {
            stream.WriteByte(ID_NUM_ITEMS);
            BinaryFileHelper.WriteIntWithHeader(stream, NumItems);
            if (m_ImageItems.Count > 0) {
                stream.WriteByte(ID_ITEM_IMAGE);
                BinaryFileHelper.WriteDictionaryBlock<MIB_ImageItem_38>(
                    stream,
                    new BinaryFileHelper.WriteObject<MIB_ImageItem_38>(
                        MIB_ImageItem_38.ToStream
                    ),
                    m_ImageItems,
                    ID_ITEM_IMAGE
                );
            }
            if (m_TextItems.Count > 0) {
                stream.WriteByte(ID_ITEM_TEXT);
                BinaryFileHelper.WriteDictionaryBlock<MIB_TextItem_39>(
                    stream,
                    new BinaryFileHelper.WriteObject<MIB_TextItem_39>(
                        MIB_TextItem_39.ToStream
                    ),
                    m_TextItems,
                    ID_ITEM_TEXT
                );
            }
            if (m_Scene42Items.Count > 0) {
                stream.WriteByte(ID_ITEM_SCENE_VIEW_42);
                BinaryFileHelper.WriteDictionaryBlock<MIB_SceneItem_42>(
                    stream,
                    new BinaryFileHelper.WriteObject<MIB_SceneItem_42>(
                        MIB_SceneItem_42.ToStream
                    ),
                    m_Scene42Items,
                    ID_ITEM_SCENE_VIEW_42
                );
            }
            if (m_Scene45Items.Count > 0) {
                stream.WriteByte(ID_ITEM_SCENE_VIEW_45);
                BinaryFileHelper.WriteDictionaryBlock<MIB_SceneItem_45>(
                    stream,
                    new BinaryFileHelper.WriteObject<MIB_SceneItem_45>(
                        MIB_SceneItem_45.ToStream
                    ),
                    m_Scene45Items,
                    ID_ITEM_SCENE_VIEW_45
                );
            }
            if (m_TextButtonItems.Count > 0) {
                stream.WriteByte(ID_ITEM_TEXT_BUTTON_46);
                BinaryFileHelper.WriteDictionaryBlock<MIB_TextButtonItem_46>(
                    stream,
                    new BinaryFileHelper.WriteObject<MIB_TextButtonItem_46>(
                        MIB_TextButtonItem_46.ToStream
                    ),
                    m_TextButtonItems,
                    ID_ITEM_TEXT_BUTTON_46
                );
            }
        }

        public void Save(string path) {
            using (FileStream fsOut = new FileStream(path, FileMode.Create, FileAccess.Write))
                Save(fsOut);
        }

        public MIB_Item GetItem(string key) {
            if (m_ImageItems.ContainsKey(key))
                return m_ImageItems[key];
            if (m_TextItems.ContainsKey(key))
                return m_TextItems[key];
            if (m_Scene42Items.ContainsKey(key))
                return m_Scene42Items[key];
            if (m_Scene45Items.ContainsKey(key))
                return m_Scene45Items[key];
            if (m_TextButtonItems.ContainsKey(key))
                return m_TextButtonItems[key];
            throw new Exception("Could not find item `" + key + "`.");
        }
    }

    public abstract class MIB_Item {
        public MIB_Position Position;
    }

    public class MIB_Rect {
        public int X1, Y1, X2, Y2;

        public static MIB_Rect FromStream(Stream stream) {
            MIB_Rect val = new MIB_Rect();
            val.X1 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.Y1 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.X2 = BinaryFileHelper.ReadIntWithHeader(stream);
            val.Y2 = BinaryFileHelper.ReadIntWithHeader(stream);
            return val;
        }

        public static void ToStream(Stream stream, MIB_Rect value) {
            BinaryFileHelper.WriteIntWithHeader(stream, value.X1);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Y1);
            BinaryFileHelper.WriteIntWithHeader(stream, value.X2);
            BinaryFileHelper.WriteIntWithHeader(stream, value.Y2);
        }
    }

    public class MIB_Position {
        private const byte
            PROPERTY_RECT = 0x2F,
            PROPERTY_PARENT = 0x31,
            PROPERTY_UNKNOWN_32 = 0x32;

        public MIB_Rect Rect;
        public bool HasParent;
        public string ParentItem;
        public int? Unknown_32;

        public static MIB_Position FromStream(Stream stream) {
            MIB_Position val = new MIB_Position();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_RECT: {
                            val.Rect = MIB_Rect.FromStream(stream);
                        }
                        break;
                    case PROPERTY_PARENT: {
                            val.HasParent = true;
                            val.ParentItem = BinaryFileHelper.ReadStringWithHeader(stream);
                        }
                        break;
                    case PROPERTY_UNKNOWN_32: {
                            val.Unknown_32 = BinaryFileHelper.ReadIntWithHeader(stream);
                        }
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_Position value) {
            stream.WriteByte(PROPERTY_RECT);
            MIB_Rect.ToStream(stream, value.Rect);
            if (value.HasParent) {
                stream.WriteByte(PROPERTY_PARENT);
                BinaryFileHelper.WriteStringWithHeader(stream, value.ParentItem);
            }
            if (value.Unknown_32.HasValue) {
                stream.WriteByte(PROPERTY_UNKNOWN_32);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown_32.Value);
            }
        }
    }

    public class MIB_ImageItem_38 : MIB_Item {
        private const byte
            PROPERTY_POSITION = 0x36;

        public static MIB_ImageItem_38 FromStream(Stream stream) {
            MIB_ImageItem_38 val = new MIB_ImageItem_38();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = BinaryFileHelper.ReadStruct<MIB_Position>(
                                stream,
                                new BinaryFileHelper.ReadObject<MIB_Position>(
                                    MIB_Position.FromStream
                                )
                            );
                        }
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_ImageItem_38 value) {
            stream.WriteByte(PROPERTY_POSITION);
            BinaryFileHelper.WriteStruct<MIB_Position>(
                stream,
                new BinaryFileHelper.WriteObject<MIB_Position>(
                    MIB_Position.ToStream
                ),
                value.Position
            );
        }
    }

    public class MIB_TextItem_39 : MIB_Item {
        private const byte
            PROPERTY_UNKNOWN_33 = 0x33,
            PROPERTY_POSITION = 0x36;

        public int? Unknown_33;

        public static MIB_TextItem_39 FromStream(Stream stream) {
            MIB_TextItem_39 val = new MIB_TextItem_39();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = BinaryFileHelper.ReadStruct<MIB_Position>(
                                stream,
                                new BinaryFileHelper.ReadObject<MIB_Position>(
                                    MIB_Position.FromStream
                                )
                            );
                        }
                        break;
                    case PROPERTY_UNKNOWN_33: {
                            val.Unknown_33 = BinaryFileHelper.ReadIntWithHeader(stream);
                        }
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_TextItem_39 value) {
            stream.WriteByte(PROPERTY_POSITION);
            BinaryFileHelper.WriteStruct<MIB_Position>(
                stream,
                new BinaryFileHelper.WriteObject<MIB_Position>(
                    MIB_Position.ToStream
                ),
                value.Position
            );
            if (value.Unknown_33.HasValue) {
                stream.WriteByte(PROPERTY_UNKNOWN_33);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown_33.Value);
            }
        }
    }

    public class MIB_SceneItem_42 : MIB_Item {
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

        public static MIB_SceneItem_42 FromStream(Stream stream) {
            MIB_SceneItem_42 val = new MIB_SceneItem_42();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = BinaryFileHelper.ReadStruct<MIB_Position>(
                                stream,
                                new BinaryFileHelper.ReadObject<MIB_Position>(
                                    MIB_Position.FromStream
                                )
                            );
                        }
                        break;
                    case PROPERTY_SCENE_NAME: {
                            val.HasSceneName = true;
                            val.SceneName = BinaryFileHelper.ReadStringWithHeader(stream);
                        }
                        break;
                    case PROPERTY_UNKNOWN_2E: {
                            val.Unknown2E = new float[9];
                            for (int i = 0; i < 9; i++)
                                val.Unknown2E[i] = BinaryFileHelper.ReadFloatWithHeader(stream);
                        } break;
                    case PROPERTY_UNKNOWN_33: {
                            val.HasUnknown33 = true;
                            val.Unknown33_0 = BinaryFileHelper.ReadIntWithHeader(stream);
                            val.Unknown33_1 = BinaryFileHelper.ReadIntWithHeader(stream);
                        } break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_SceneItem_42 value) {
            stream.WriteByte(PROPERTY_POSITION);
            BinaryFileHelper.WriteStruct<MIB_Position>(
                stream,
                new BinaryFileHelper.WriteObject<MIB_Position>(
                    MIB_Position.ToStream
                ),
                value.Position
            );
            if (value.HasSceneName) {
                stream.WriteByte(PROPERTY_SCENE_NAME);
                BinaryFileHelper.WriteStringWithHeader(stream, value.SceneName);
            }
            stream.WriteByte(PROPERTY_UNKNOWN_2E);
            for (int i = 0; i < 9; i++)
                BinaryFileHelper.WriteFloatWithHeader(stream, value.Unknown2E[i]);
            if (value.HasUnknown33) {
                stream.WriteByte(PROPERTY_UNKNOWN_33);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown33_0);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown33_1);
            }
        }
    }

    public class MIB_SceneItem_45 : MIB_Item {
        private const byte
            PROPERTY_SCENE_NAME = 0x2D,
            PROPERTY_POSITION = 0x36;

        public bool HasSceneName;
        public string SceneName;

        public static MIB_SceneItem_45 FromStream(Stream stream) {
            MIB_SceneItem_45 val = new MIB_SceneItem_45();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = BinaryFileHelper.ReadStruct<MIB_Position>(
                                stream,
                                new BinaryFileHelper.ReadObject<MIB_Position>(
                                    MIB_Position.FromStream
                                )
                            );
                        }
                        break;
                    case PROPERTY_SCENE_NAME: {
                            val.HasSceneName = true;
                            val.SceneName = BinaryFileHelper.ReadStringWithHeader(stream);
                        }
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_SceneItem_45 value) {
            stream.WriteByte(PROPERTY_POSITION);
            BinaryFileHelper.WriteStruct<MIB_Position>(
                stream,
                new BinaryFileHelper.WriteObject<MIB_Position>(
                    MIB_Position.ToStream
                ),
                value.Position
            );
            if (value.HasSceneName) {
                stream.WriteByte(PROPERTY_SCENE_NAME);
                BinaryFileHelper.WriteStringWithHeader(stream, value.SceneName);
            }
        }
    }

    public class MIB_TextButtonItem_46 : MIB_Item {
        private const byte
               PROPERTY_UNKNOWN_32 = 0x32,
               PROPERTY_UNKNOWN_33 = 0x33,
               PROPERTY_POSITION = 0x36;

        public int? Unknown_32;
        public int? Unknown_33;

        public static MIB_TextButtonItem_46 FromStream(Stream stream) {
            MIB_TextButtonItem_46 val = new MIB_TextButtonItem_46();
            while (!BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY)) {
                byte property_id = BinaryFileHelper.ReadByte(stream);
                switch (property_id) {
                    case PROPERTY_POSITION: {
                            val.Position = BinaryFileHelper.ReadStruct<MIB_Position>(
                                stream,
                                new BinaryFileHelper.ReadObject<MIB_Position>(
                                    MIB_Position.FromStream
                                )
                            );
                        }
                        break;
                    case PROPERTY_UNKNOWN_32: {
                            val.Unknown_32 = BinaryFileHelper.ReadIntWithHeader(stream);
                        }
                        break;
                    case PROPERTY_UNKNOWN_33: {
                            val.Unknown_33 = BinaryFileHelper.ReadIntWithHeader(stream);
                        }
                        break;
                    default:
                        throw new UnexpectedPropertyException(property_id, stream.Position - 1);
                }
            }
            return val;
        }

        public static void ToStream(Stream stream, MIB_TextButtonItem_46 value) {
            stream.WriteByte(PROPERTY_POSITION);
            BinaryFileHelper.WriteStruct<MIB_Position>(
                stream,
                new BinaryFileHelper.WriteObject<MIB_Position>(
                    MIB_Position.ToStream
                ),
                value.Position
            );
            if (value.Unknown_32.HasValue) {
                stream.WriteByte(PROPERTY_UNKNOWN_32);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown_32.Value);
            }
            if (value.Unknown_33.HasValue) {
                stream.WriteByte(PROPERTY_UNKNOWN_33);
                BinaryFileHelper.WriteIntWithHeader(stream, value.Unknown_33.Value);
            }
        }
    }
}