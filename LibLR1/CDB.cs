using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;

namespace LibLR1
{
	/// <summary>
	/// Something to do with cutscenes. Haven't looked into this in a while. Incomplete.
	/// </summary>
	public class CDB
	{
		private const byte
			ID_WDBS     = 0x28,
			ID_CUTSCENE = 0x27;
		
		private const byte
			PROPERTY_CAMERA                = 0x29,
			PROPERTY_CAMERA_NAME           = 0x2A,
			PROPERTY_START_FRAME           = 0x2B,
			PROPERTY_DURATION              = 0x2C,
			PROPERTY_ANIM_SEQ_ID           = 0x2D,
			PROPERTY_MODEL                 = 0x2E,
			PROPERTY_MODEL_NAME            = 0x30,
			PROPERTY_MODEL_LOCATION        = 0x33,
			PROPERTY_MODEL_ROTATION        = 0x34,
			PROPERTY_LIGHTTYPE_AMBIENT     = 0x35,
			PROPERTY_EVENT                 = 0x37,
			PROPERTY_LIGHT_COLOR           = 0x38,
			PROPERTY_LIGHT_DIRECTION       = 0x39,
			PROPERTY_LIGHTTYPE_DIRECTIONAL = 0x3A,
			PROPERTY_SPEED                 = 0x3B;
		
		private string[]                         m_WDBs;
		private Dictionary<string, CDB_Cutscene> m_cutscenes;
		
		public string[] WDBs
		{
			get { return m_WDBs; }
			set { m_WDBs = value; }
		}
		public Dictionary<string, CDB_Cutscene> Cutscenes
		{
			get { return m_cutscenes; }
			set { m_cutscenes = value; }
		}

		public CDB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CDB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_WDBS:
					{
						m_WDBs = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_CUTSCENE:
					{
						m_cutscenes = p_reader.ReadDictionaryBlock<CDB_Cutscene>(
							CDB_Cutscene.Read,
							ID_CUTSCENE
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
	
	public class CDB_Cutscene
	{
		private const byte
			PROPERTY_CAMERA                = 0x29,
			PROPERTY_CAMERA_NAME           = 0x2A,
			PROPERTY_START_FRAME           = 0x2B,
			PROPERTY_DURATION              = 0x2C,
			PROPERTY_ANIM_SEQ_ID           = 0x2D,
			PROPERTY_MODEL                 = 0x2E,
			PROPERTY_MODEL_NAME            = 0x30,
			PROPERTY_MODEL_LOCATION        = 0x33,
			PROPERTY_MODEL_ROTATION        = 0x34,
			PROPERTY_LIGHTTYPE_AMBIENT     = 0x35,
			PROPERTY_EVENT                 = 0x37,
			PROPERTY_LIGHT_COLOR           = 0x38,
			PROPERTY_LIGHT_DIRECTION       = 0x39,
			PROPERTY_LIGHTTYPE_DIRECTIONAL = 0x3A,
			PROPERTY_SPEED                 = 0x3B;
		
		public int Speed;
		public int Duration;
		public KeyValuePair<string, CDB_Camera>[]           Cameras           = new KeyValuePair<string, CDB_Camera>[0];
		public KeyValuePair<string, CDB_Model>[]            Models            = new KeyValuePair<string, CDB_Model>[0];
		public KeyValuePair<string, CDB_Event>[]            Events            = new KeyValuePair<string, CDB_Event>[0];
		public KeyValuePair<string, CDB_AmbientLight>[]     AmbientLights     = new KeyValuePair<string, CDB_AmbientLight>[0];
		public KeyValuePair<string, CDB_DirectionalLight>[] DirectionalLights = new KeyValuePair<string, CDB_DirectionalLight>[0];
		
		public static CDB_Cutscene Read(LRBinaryReader p_reader)
		{
			CDB_Cutscene val = new CDB_Cutscene();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_SPEED:
					{
						val.Speed = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_CAMERA:
					{
						val.Cameras = p_reader.ReadCollidableDictionaryBlock<CDB_Camera>(
							CDB_Camera.Read,
							PROPERTY_CAMERA
						);
						break;
					}
					case PROPERTY_MODEL:
					{
						val.Models = p_reader.ReadCollidableDictionaryBlock<CDB_Model>(
							CDB_Model.Read,
							PROPERTY_MODEL
						);
						break;
					}
					case PROPERTY_EVENT:
					{
						val.Events = p_reader.ReadCollidableDictionaryBlock<CDB_Event>(
							CDB_Event.Read,
							PROPERTY_EVENT
						);
						break;
					}
					case PROPERTY_LIGHTTYPE_AMBIENT:
					{
						val.AmbientLights = p_reader.ReadCollidableDictionaryBlock<CDB_AmbientLight>(
							CDB_AmbientLight.Read,
							PROPERTY_LIGHTTYPE_AMBIENT
						);
						break;
					}
					case PROPERTY_LIGHTTYPE_DIRECTIONAL:
					{
						val.DirectionalLights = p_reader.ReadCollidableDictionaryBlock<CDB_DirectionalLight>(
							CDB_DirectionalLight.Read,
							PROPERTY_LIGHTTYPE_DIRECTIONAL
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
	}
	
	public class CDB_Camera
	{
		private const byte
			PROPERTY_CAMERA_NAME = 0x2A,
			PROPERTY_START_FRAME = 0x2B,
			PROPERTY_DURATION    = 0x2C,
			PROPERTY_ANIM_SEQ_ID = 0x2D;
		
		public string ModelName;
		public int    StartFrame;
		public int    Duration;
		public int    AnimationSequenceId;

		public static CDB_Camera Read(LRBinaryReader p_reader)
		{
			CDB_Camera val = new CDB_Camera();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_CAMERA_NAME:
					{
						val.ModelName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_ANIM_SEQ_ID:
					{
						val.AnimationSequenceId = p_reader.ReadIntWithHeader();
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
	
	public class CDB_Model
	{
		private const byte
			PROPERTY_START_FRAME            = 0x2B,
			PROPERTY_DURATION               = 0x2C,
			PROPERTY_ANIM_SEQ_ID            = 0x2D,
			PROPERTY_MODEL_NAME_STATIC      = 0x2F,
			PROPERTY_MODEL_NAME             = 0x30,
			PROPERTY_MODEL_SPRITE_REFERENCE = 0x32,
			PROPERTY_MODEL_LOCATION         = 0x33,
			PROPERTY_MODEL_ROTATION         = 0x34,
			PROPERTY_LIGHTTYPE_AMBIENT      = 0x35,
			PROPERTY_UNKNOWN_36             = 0x36,
			PROPERTY_SPEED                  = 0x3B;
		
		public string      ModelName;
		public string      StaticModelName;
		public int         SpriteRefSceneId, SpriteRefItemId;
		public LRVector3   Location;
		public LRVector3   RotationFwd, RotationUp;
		public int         StartFrame;
		public int         Duration;
		public int         AnimationSequenceId;
		public CDB_5ints[] Unknown;

		public static CDB_Model Read(LRBinaryReader p_reader)
		{
			CDB_Model val = new CDB_Model();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_MODEL_NAME_STATIC:
					{
						val.StaticModelName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_MODEL_NAME:
					{
						val.ModelName = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_MODEL_SPRITE_REFERENCE:
					{
						val.SpriteRefSceneId = p_reader.ReadIntWithHeader();
						val.SpriteRefItemId = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_MODEL_LOCATION:
					{
						val.Location = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_MODEL_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp  = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_ANIM_SEQ_ID:
					{
						val.AnimationSequenceId = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_36:
					{
						val.Unknown = p_reader.ReadArrayBlock<CDB_5ints>(
							CDB_5ints.Read
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
	}
	
	public class CDB_AmbientLight
	{
		private const byte
			PROPERTY_START_FRAME = 0x2B,
			PROPERTY_DURATION    = 0x2C,
			PROPERTY_LIGHT_COLOR = 0x38;
		
		public int     StartFrame;
		public int     EndFrame;
		public LRColor Color;

		public static CDB_AmbientLight Read(LRBinaryReader p_reader)
		{
			CDB_AmbientLight val = new CDB_AmbientLight();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.EndFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_LIGHT_COLOR:
					{
						val.Color = LRColor.ReadNoAlpha(p_reader);
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
	
	public class CDB_Event
	{
		private const byte
			PROPERTY_START_FRAME    = 0x2B,
			PROPERTY_DURATION       = 0x2C,
			PROPERTY_MODEL_LOCATION = 0x33,
			PROPERTY_MODEL_ROTATION = 0x34;
		
		public LRVector3 Location;
		public LRVector3 RotationFwd, RotationUp;
		public int       StartFrame;
		public int       Duration;

		public static CDB_Event Read(LRBinaryReader p_reader)
		{
			CDB_Event val = new CDB_Event();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_MODEL_LOCATION:
					{
						val.Location = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_MODEL_ROTATION:
					{
						val.RotationFwd = LRVector3.Read(p_reader);
						val.RotationUp  = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = p_reader.ReadIntWithHeader();
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
	
	public class CDB_DirectionalLight
	{
		private const byte
			PROPERTY_START_FRAME     = 0x2B,
			PROPERTY_DURATION        = 0x2C,
			PROPERTY_LIGHT_COLOR     = 0x38,
			PROPERTY_LIGHT_DIRECTION = 0x39,
			PROPERTY_UNKNOWN_3C      = 0x3C;
		
		public int       StartFrame;
		public int       EndFrame;
		public LRColor   Color;
		public LRVector3 Direction;
		public int[]     Unknown3C;

		public static CDB_DirectionalLight Read(LRBinaryReader p_reader)
		{
			CDB_DirectionalLight val = new CDB_DirectionalLight();
			while (p_reader.Next(Token.RightCurly) == false)
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_DURATION:
					{
						val.EndFrame = p_reader.ReadIntWithHeader();
						break;
					}
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
					case PROPERTY_UNKNOWN_3C:
					{
						val.Unknown3C    = new int[2];
						val.Unknown3C[0] = p_reader.ReadIntWithHeader();
						val.Unknown3C[1] = p_reader.ReadIntWithHeader();
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
	
	public class CDB_5ints
	{
		public int
			Unknown1,
			Unknown2,
			Unknown3,
			Unknown4,
			Unknown5;

		public static CDB_5ints Read(LRBinaryReader p_reader)
		{
			CDB_5ints val = new CDB_5ints();
			val.Unknown1 = p_reader.ReadIntWithHeader();
			val.Unknown2 = p_reader.ReadIntWithHeader();
			val.Unknown3 = p_reader.ReadIntWithHeader();
			val.Unknown4 = p_reader.ReadIntWithHeader();
			val.Unknown5 = p_reader.ReadIntWithHeader();
			return val;
		}
	}
}