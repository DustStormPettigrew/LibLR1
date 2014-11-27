using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;

namespace LibLR1
{
	/// <summary>
	/// WORK IN PROGRESS.
	/// 3D Scene format.
	/// </summary>
	public class WDB
	{
		private const byte
			ID_TDB                     = 0x27,
			ID_MDB                     = 0x28,
			ID_ADB                     = 0x29,
			ID_GDB                     = 0x2A,
			ID_GDB2                    = 0x2B,
			ID_SDB                     = 0x2C,
			ID_BDB                     = 0x2D,
			ID_STATIC_MODEL            = 0x2E,
			ID_ANIMATED_MODEL          = 0x2F,
			ID_BDB_MODEL               = 0x30,
			ID_BILLBOARD_SPRITE        = 0x37,
			ID_MAB                     = 0x3D,
			ID_BVB                     = 0x40,
			ID_BVB_MODEL               = 0x41,
			ID_CAMERA                  = 0x43,
			ID_AMBIENT_LIGHT           = 0x48,
			ID_DIRECTIONAL_LIGHT       = 0x49,
			PROPERTY_GDB               = 0x2A,
			PROPERTY_ADB_SDB           = 0x2C,
			PROPERTY_ANIMATED_MODEL    = 0x2F,
			PROPERTY_OBJECT_POSITION   = 0x31,
			PROPERTY_OBJECT_ROTATION   = 0x32,
			PROPERTY_GDB_ADB_SDB       = 0x33,
			PROPERTY_UNKNOWN_35        = 0x35,
			PROPERTY_UNKNOWN_3E        = 0x3E,
			PROPERTY_UNKNOWN_3F        = 0x3F,
			PROPERTY_BVB               = 0x40,
			PROPERTY_UNKNOWN_42        = 0x42,
			PROPERTY_CAMERA_NEAR_PLANE = 0x45,
			PROPERTY_CAMERA_FAR_PLANE  = 0x46,
			PROPERTY_CAMERA_FOV        = 0x47,
			PROPERTY_LIGHT_COLOR       = 0x4A,
			PROPERTY_LIGHT_DIRECTION   = 0x4B,
			PROPERTY_UNKNOWN_4C        = 0x4C;

		private string[] m_TDBs;
		private string[] m_MDBs;
		private string[] m_ADBs;
		private string[] m_GDBs;
		private string[] m_GDB2s;
		private string[] m_SDBs;
		private string[] m_BDBs;
		private string[] m_MABs;
		private string[] m_BVBs;
		private Dictionary<string, WDB_StaticModel> m_staticModels;
		private Dictionary<string, WDB_AnimatedModel> m_animatedModels;
		private Dictionary<string, WDB_BDBModel> m_BDBModels;
		private WDB_Billboard[] m_billboards;
		private Dictionary<string, WDB_BVBModel> m_BVBModels;
		private Dictionary<string, WDB_Camera> m_cameras;
		private Dictionary<string, WDB_AmbientLight> m_ambientLights;
		private Dictionary<string, WDB_DirectionalLight> m_directionalLights;

		public string[] TDBs
		{
			get { return m_TDBs; }
			set { m_TDBs = value; }
		}
		public string[] MDBs
		{
			get { return m_MDBs; }
			set { m_MDBs = value; }
		}
		public string[] ADBs
		{
			get { return m_ADBs; }
			set { m_ADBs = value; }
		}
		public string[] GDBs
		{
			get { return m_GDBs; }
			set { m_GDBs = value; }
		}
		public string[] SDBs
		{
			get { return m_SDBs; }
			set { m_SDBs = value; }
		}
		public string[] BDBs
		{
			get { return m_BDBs; }
			set { m_BDBs = value; }
		}
		public string[] MABs
		{
			get { return m_MABs; }
			set { m_MABs = value; }
		}
		public string[] BVBs
		{
			get { return m_BVBs; }
			set { m_BVBs = value; }
		}
		public Dictionary<string, WDB_StaticModel> StaticModels
		{
			get { return m_staticModels; }
			set { m_staticModels = value; }
		}
		public Dictionary<string, WDB_AnimatedModel> AnimatedModels
		{
			get { return m_animatedModels; }
			set { m_animatedModels = value; }
		}
		public Dictionary<string, WDB_BDBModel> BDBModels
		{
			get { return m_BDBModels; }
			set { m_BDBModels = value; }
		}
		public Dictionary<string, WDB_BVBModel> BVBModels
		{
			get { return m_BVBModels; }
			set { m_BVBModels = value; }
		}
		public Dictionary<string, WDB_Camera> Cameras
		{
			get { return m_cameras; }
			set { m_cameras = value; }
		}
		public Dictionary<string, WDB_AmbientLight> AmbientLights
		{
			get { return m_ambientLights; }
			set { m_ambientLights = value; }
		}
		public Dictionary<string, WDB_DirectionalLight> DirectionalLights
		{
			get { return m_directionalLights; }
			set { m_directionalLights = value; }
		}
		
		public WDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public WDB(LRBinaryReader p_reader)
		{
			m_TDBs = new string[0];
			m_MDBs = new string[0];
			m_ADBs = new string[0];
			m_GDBs = new string[0];
			m_SDBs = new string[0];
			m_BDBs = new string[0];
			m_MABs = new string[0];
			m_BVBs = new string[0];
			m_staticModels = new Dictionary<string, WDB_StaticModel>();
			m_animatedModels = new Dictionary<string, WDB_AnimatedModel>();
			m_BDBModels = new Dictionary<string, WDB_BDBModel>();
			m_BVBModels = new Dictionary<string, WDB_BVBModel>();
			m_cameras = new Dictionary<string, WDB_Camera>();
			m_ambientLights = new Dictionary<string, WDB_AmbientLight>();
			m_directionalLights = new Dictionary<string, WDB_DirectionalLight>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TDB:
					{
						m_TDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_MDB:
					{
						m_MDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_ADB:
					{
						m_ADBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_GDB:
					{
						m_GDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_GDB2:
					{
						m_GDB2s = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_SDB:
					{
						m_SDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_BDB:
					{
						m_BDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_MAB:
					{
						m_SDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_BVB:
					{
						m_BVBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_STATIC_MODEL:
					{
						m_staticModels = p_reader.ReadDictionaryBlock<WDB_StaticModel>(
							WDB_StaticModel.Read,
							ID_STATIC_MODEL
						);
						break;
					}
					case ID_ANIMATED_MODEL:
					{
						m_animatedModels = p_reader.ReadDictionaryBlock<WDB_AnimatedModel>(
							WDB_AnimatedModel.Read,
							ID_ANIMATED_MODEL
						);
						break;
					}
					case ID_BDB_MODEL:
					{
						m_BDBModels = p_reader.ReadDictionaryBlock<WDB_BDBModel>(
							WDB_BDBModel.Read,
							ID_BDB_MODEL
						);
						break;
					}
					case ID_BILLBOARD_SPRITE:
					{
						m_billboards = p_reader.ReadStructArrayBlock<WDB_Billboard>(
							WDB_Billboard.Read,
							ID_BILLBOARD_SPRITE
						);
						break;
					}
					case ID_BVB_MODEL:
					{
						m_BVBModels = p_reader.ReadDictionaryBlock<WDB_BVBModel>(
							WDB_BVBModel.Read,
							ID_BVB_MODEL
						);
						break;
					}
					case ID_CAMERA:
					{
						m_cameras = p_reader.ReadDictionaryBlock<WDB_Camera>(
							WDB_Camera.Read,
							ID_CAMERA
						);
						break;
					}
					case ID_AMBIENT_LIGHT:
					{
						m_ambientLights = p_reader.ReadDictionaryBlock<WDB_AmbientLight>(
							WDB_AmbientLight.Read,
							ID_AMBIENT_LIGHT
						);
						break;
					}
					case ID_DIRECTIONAL_LIGHT:
					{
						m_directionalLights = p_reader.ReadDictionaryBlock<WDB_DirectionalLight>(
							WDB_DirectionalLight.Read,
							ID_DIRECTIONAL_LIGHT
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
	}

	public class WDB_Ref_GDB
	{
		public int IndexGDB;
		public float Unknown;

		public WDB_Ref_GDB()
			: this(0, 0) { }

		public WDB_Ref_GDB(int indexgdb, float unknown)
		{
			IndexGDB = indexgdb;
			Unknown = unknown;
		}

		public static WDB_Ref_GDB Read(LRBinaryReader p_reader)
		{
			WDB_Ref_GDB val = new WDB_Ref_GDB();
			val.IndexGDB = p_reader.ReadIntWithHeader();
			val.Unknown  = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class WDB_Ref_GDB_ADB_SDB
	{
		public int? IndexGDB;
		public int IndexADB, IndexSDB;
		public float Unknown;

		public WDB_Ref_GDB_ADB_SDB()
			: this(0, 0, 0, 0) { }

		public WDB_Ref_GDB_ADB_SDB(int? indexgdb, int indexadb, int indexsdb, float unknown)
		{
			IndexGDB = indexgdb;
			IndexADB = indexadb;
			IndexSDB = indexsdb;
			Unknown = unknown;
		}

		public static WDB_Ref_GDB_ADB_SDB Read(LRBinaryReader p_reader)
		{
			WDB_Ref_GDB_ADB_SDB val = new WDB_Ref_GDB_ADB_SDB();
			val.IndexGDB = p_reader.ReadIntWithHeader();
			val.IndexADB = p_reader.ReadIntWithHeader();
			val.IndexSDB = p_reader.ReadIntWithHeader();
			val.Unknown = p_reader.ReadFloatWithHeader();
			return val;
		}

		public static WDB_Ref_GDB_ADB_SDB ReadNoGDB(LRBinaryReader p_reader)
		{
			WDB_Ref_GDB_ADB_SDB val = new WDB_Ref_GDB_ADB_SDB();
			val.IndexGDB = null;
			val.IndexADB = p_reader.ReadIntWithHeader();
			val.IndexSDB = p_reader.ReadIntWithHeader();
			val.Unknown = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class WDB_Ref_GDB_BDB
	{
		public int IndexGDB, IndexBDB;
		public float Unknown;

		public WDB_Ref_GDB_BDB()
			: this(0, 0, 0) { }

		public WDB_Ref_GDB_BDB(int indexgdb, int indexbdb, float unknown)
		{
			IndexGDB = indexgdb;
			IndexBDB = indexbdb;
			Unknown = unknown;
		}

		public static WDB_Ref_GDB_BDB Read(LRBinaryReader p_reader)
		{
			WDB_Ref_GDB_BDB val = new WDB_Ref_GDB_BDB();
			val.IndexGDB = p_reader.ReadIntWithHeader();
			val.IndexBDB = p_reader.ReadIntWithHeader();
			val.Unknown = p_reader.ReadFloatWithHeader();
			return val;
		}
	}

	public class WDB_Ref_CameraModel
	{
		public int AnimatedModelId;
		public int BoneId;

		public static WDB_Ref_CameraModel Read(LRBinaryReader p_reader)
		{
			WDB_Ref_CameraModel val = new WDB_Ref_CameraModel();
			val.AnimatedModelId = p_reader.ReadIntWithHeader();
			val.BoneId = p_reader.ReadIntWithHeader();
			return val;
		}
	}

	public class WDB_StaticModel
	{
		private const byte
			PROPERTY_GDB = 0x2A,
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_OBJECT_ROTATION = 0x32,
			PROPERTY_UNKNOWN_3E = 0x3E,
			PROPERTY_UNKNOWN_3F = 0x3F,
			PROPERTY_UNKNOWN_42 = 0x42;

		public WDB_Ref_GDB ModelRef;
		public LRVector3 Position;
		public LRVector3 RotationFwd, RotationUp;
		public WDB_Unknown3E[] Unknown_3E;
		public WDB_Unknown3F Unknown_3F;
		public bool Unknown_42;

		public static WDB_StaticModel Read(LRBinaryReader p_reader)
		{
			WDB_StaticModel val = new WDB_StaticModel();
			while (!p_reader.Next( Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_GDB:
					{
						val.ModelRef = WDB_Ref_GDB.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp  = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_3E:
					{
						val.Unknown_3E = p_reader.ReadArrayBlock<WDB_Unknown3E>(
							WDB_Unknown3E.Read
						);
						break;
					}
					case PROPERTY_UNKNOWN_3F:
					{
						val.Unknown_3F = WDB_Unknown3F.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_42:
					{
						val.Unknown_42 = true;
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
	}

	public class WDB_BDBModel
	{
		private const byte
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_OBJECT_ROTATION = 0x32,
			PROPERTY_BDB = 0x34,
			PROPERTY_UNKNOWN_3E = 0x3E,
			PROPERTY_UNKNOWN_42 = 0x42;

		public WDB_Ref_GDB_BDB ModelRef;
		public LRVector3 Position;
		public LRVector3 RotationFwd, RotationUp;
		public WDB_Unknown3E[] Unknown_3E;
		public bool Unknown_42;

		public static WDB_BDBModel Read(LRBinaryReader p_reader)
		{
			WDB_BDBModel val = new WDB_BDBModel();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_BDB:
					{
						val.ModelRef = WDB_Ref_GDB_BDB.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp  = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_3E:
					{
						val.Unknown_3E = p_reader.ReadArrayBlock<WDB_Unknown3E>(
							WDB_Unknown3E.Read
						);
						break;
					}
					case PROPERTY_UNKNOWN_42:
					{
						val.Unknown_42 = true;
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
	}

	public class WDB_Billboard
	{
		private const byte
			PROPERTY_UNKNOWN_2B = 0x2B,
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_UNKNOWN_38 = 0x38,
			PROPERTY_TEXTURE_NAME = 0x39,
			PROPERTY_UNKNOWN_3A = 0x3A,
			PROPERTY_UNKNOWN_3B = 0x3B,
			PROPERTY_UNKNOWN_3C = 0x3C,
			PROPERTY_UNKNOWN_3E = 0x3E;

		public LRVector3 Position;
		public LRVector3 Unknown38;
		public string TextureName;
		public float Unknown3A;
		public float Unknown3B;
		public float Unknown3C;
		public int Unknown2B_A, Unknown2B_B;
		public int Unknown3E_A, Unknown3E_B;

		public static WDB_Billboard Read(LRBinaryReader p_reader)
		{
			WDB_Billboard val = new WDB_Billboard();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_2B:
					{
						val.Unknown2B_A = p_reader.ReadIntWithHeader();
						val.Unknown2B_B = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_38:
					{
						val.Unknown38 = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_TEXTURE_NAME:
					{
						val.TextureName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3A:
					{
						val.Unknown3A = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3B:
					{
						val.Unknown3B = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3C:
					{
						val.Unknown3C = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3E:
					{
						val.Unknown3E_A = p_reader.ReadIntWithHeader();
						val.Unknown3E_B = p_reader.ReadIntWithHeader();
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
	}

	public class WDB_BVBModel
	{
		private const byte
			PROPERTY_BVB = 0x40,
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_OBJECT_ROTATION = 0x32;

		public int ModelRef;
		public LRVector3 Position;
		public LRVector3 RotationFwd, RotationUp;

		public static WDB_BVBModel Read(LRBinaryReader p_reader)
		{
			WDB_BVBModel val = new WDB_BVBModel();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_BVB:
					{
						val.ModelRef = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp = LRVector3.Read(p_reader);
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
	}

	public class WDB_AnimatedModel
	{
		private const byte
			PROPERTY_ADB_SDB = 0x2C,
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_OBJECT_ROTATION = 0x32,
			PROPERTY_GDB_ADB_SDB = 0x33,
			PROPERTY_UNKNOWN_35 = 0x35,
			PROPERTY_UNKNOWN_3E = 0x3E,
			PROPERTY_UNKNOWN_3F = 0x3F,
			PROPERTY_UNKNOWN_42 = 0x42,
			PROPERTY_UNKNOWN_4C = 0x4C;

		public WDB_Ref_GDB_ADB_SDB ModelRef;
		public LRVector3 Position;
		public LRVector3 RotationFwd, RotationUp;
		public int Unknown_35;
		public WDB_Unknown3E[] Unknown_3E;
		public WDB_Unknown3F Unknown_3F;
		public bool Unknown_42;
		public bool Unknown_4C;

		public static WDB_AnimatedModel Read(LRBinaryReader p_reader)
		{
			WDB_AnimatedModel val = new WDB_AnimatedModel();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_ADB_SDB:
					{
						val.ModelRef = WDB_Ref_GDB_ADB_SDB.ReadNoGDB(p_reader);
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_GDB_ADB_SDB:
					{
						val.ModelRef = WDB_Ref_GDB_ADB_SDB.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_35:
					{
						val.Unknown_35 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3E:
					{
						val.Unknown_3E = p_reader.ReadArrayBlock<WDB_Unknown3E>(
							WDB_Unknown3E.Read
						);
					}
					break;
					case PROPERTY_UNKNOWN_3F:
					{
						val.Unknown_3F = WDB_Unknown3F.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_42:
					{
						val.Unknown_42 = true;
						break;
					}
					case PROPERTY_UNKNOWN_4C:
					{
						val.Unknown_4C = true;
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
	}

	public class WDB_Camera
	{
		private const byte
			PROPERTY_ANIMATED_MODEL = 0x2F,
			PROPERTY_OBJECT_POSITION = 0x31,
			PROPERTY_OBJECT_ROTATION = 0x32,
			PROPERTY_UNKNOWN_35 = 0x35,
			PROPERTY_CAMERA_NEAR_PLANE = 0x45,
			PROPERTY_CAMERA_FAR_PLANE = 0x46,
			PROPERTY_CAMERA_FOV = 0x47;

		public WDB_Ref_CameraModel Model;
		public LRVector3 Position;
		public LRVector3 RotationFwd, RotationUp;
		public int Unknown_35;
		public float NearPlane;
		public float FarPlane;
		public float Fov;

		public static WDB_Camera Read(LRBinaryReader p_reader)
		{
			WDB_Camera val = new WDB_Camera();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_ANIMATED_MODEL:
					{
						val.Model = WDB_Ref_CameraModel.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_POSITION:
					{
						val.Position = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_OBJECT_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp  = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_35:
					{
						val.Unknown_35 = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_CAMERA_NEAR_PLANE:
					{
						val.NearPlane = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_CAMERA_FAR_PLANE:
					{
						val.FarPlane = p_reader.ReadFloatWithHeader();
						break;
					}
					case PROPERTY_CAMERA_FOV:
					{
						val.Fov = p_reader.ReadFloatWithHeader();
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
	}

	public class WDB_AmbientLight
	{
		private const byte
			PROPERTY_LIGHT_COLOR = 0x4A;

		public LRColor Color;

		public static WDB_AmbientLight Read(LRBinaryReader p_reader)
		{
			WDB_AmbientLight val = new WDB_AmbientLight();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte property_id = p_reader.ReadByte();
				switch (property_id)
				{
					case PROPERTY_LIGHT_COLOR:
					{
						val.Color = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					default:
					{
						throw new UnexpectedPropertyException(
							property_id,
							p_reader.BaseStream.Position - 1
						);
					}
				}
			}
			return val;
		}
	}

	public class WDB_DirectionalLight
	{
		private const byte
			PROPERTY_LIGHT_COLOR = 0x4A,
			PROPERTY_LIGHT_DIRECTION = 0x4B;

		public LRColor Color;
		public LRVector3 Direction;

		public static WDB_DirectionalLight Read(LRBinaryReader p_reader)
		{
			WDB_DirectionalLight val = new WDB_DirectionalLight();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_LIGHT_COLOR:
					{
						val.Color = LRColor.ReadNoAlpha(p_reader);
						break;
					}
					case PROPERTY_LIGHT_DIRECTION:
					{
						val.Direction = LRVector3.Read(p_reader);
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
	}

	public class WDB_Unknown3E
	{
		public int Unknown1, Unknown2, Unknown3, Unknown4;

		public WDB_Unknown3E()
			: this(0, 0, 0, 0) { }

		public WDB_Unknown3E(int unknown1, int unknown2, int unknown3, int unknown4)
		{
			Unknown1 = unknown1;
			Unknown2 = unknown2;
			Unknown3 = unknown3;
			Unknown4 = unknown4;
		}

		public static WDB_Unknown3E Read(LRBinaryReader p_reader)
		{
			WDB_Unknown3E val = new WDB_Unknown3E();
			val.Unknown1 = p_reader.ReadIntWithHeader();
			val.Unknown2 = p_reader.ReadIntWithHeader();
			val.Unknown3 = p_reader.ReadIntWithHeader();
			val.Unknown4 = p_reader.ReadIntWithHeader();
			return val;
		}
	}

	public class WDB_Unknown3F
	{
		public float Unknown1, Unknown2;

		public WDB_Unknown3F()
			: this(0, 0) { }

		public WDB_Unknown3F(float unknown1, float unknown2)
		{
			Unknown1 = unknown1;
			Unknown2 = unknown2;
		}

		public static WDB_Unknown3F Read(LRBinaryReader p_reader)
		{
			WDB_Unknown3F val = new WDB_Unknown3F();
			val.Unknown1 = p_reader.ReadFloatWithHeader();
			val.Unknown2 = p_reader.ReadFloatWithHeader();
			return val;
		}
	}
}