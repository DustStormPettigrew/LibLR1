using System.Collections.Generic;
using System.IO;
using LibLR1.Exceptions;
using LibLR1.Utils;

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
		private Dictionary<string, CDB_Cutscene> m_Cutscenes;
		
		public string[] WDBs
		{
			get { return m_WDBs; }
			set { m_WDBs = value; }
		}
		public Dictionary<string, CDB_Cutscene> Cutscenes
		{
			get { return m_Cutscenes; }
			set { m_Cutscenes = value; }
		}
		
		public CDB(Stream stream)
		{
			while (stream.Position < stream.Length)
			{
				byte block_id = BinaryFileHelper.ReadByte(stream);
				switch (block_id)
				{
					case ID_WDBS:
					{
						m_WDBs = BinaryFileHelper.ReadStringArrayBlock(stream);
						break;
					}
					case ID_CUTSCENE:
					{
						m_Cutscenes = BinaryFileHelper.ReadDictionaryBlock<CDB_Cutscene>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_Cutscene>(
								CDB_Cutscene.FromStream
							),
							ID_CUTSCENE
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
		
		public CDB(string path, bool decompress = true)
			: this(decompress ? BinaryFileHelper.Decompress(path) : (Stream)(new FileStream(path, FileMode.Open, FileAccess.Read)))
		{
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
		
		public static CDB_Cutscene FromStream(Stream stream)
		{
			CDB_Cutscene val = new CDB_Cutscene();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_SPEED:
					{
						val.Speed = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_CAMERA:
					{
						val.Cameras = BinaryFileHelper.ReadCollidableDictionaryBlock<CDB_Camera>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_Camera>(
								CDB_Camera.FromStream
							),
							PROPERTY_CAMERA
						);
						break;
					}
					case PROPERTY_MODEL:
					{
						val.Models = BinaryFileHelper.ReadCollidableDictionaryBlock<CDB_Model>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_Model>(
								CDB_Model.FromStream
							),
							PROPERTY_MODEL
						);
						break;
					}
					case PROPERTY_EVENT:
					{
						val.Events = BinaryFileHelper.ReadCollidableDictionaryBlock<CDB_Event>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_Event>(
								CDB_Event.FromStream
							),
							PROPERTY_EVENT
						);
						break;
					}
					case PROPERTY_LIGHTTYPE_AMBIENT:
					{
						val.AmbientLights = BinaryFileHelper.ReadCollidableDictionaryBlock<CDB_AmbientLight>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_AmbientLight>(
								CDB_AmbientLight.FromStream
							),
							PROPERTY_LIGHTTYPE_AMBIENT
						);
						break;
					}
					case PROPERTY_LIGHTTYPE_DIRECTIONAL:
					{
						val.DirectionalLights = BinaryFileHelper.ReadCollidableDictionaryBlock<CDB_DirectionalLight>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_DirectionalLight>(
								CDB_DirectionalLight.FromStream
							),
							PROPERTY_LIGHTTYPE_DIRECTIONAL
						);
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
		
		public static CDB_Camera FromStream(Stream stream)
		{
			CDB_Camera val = new CDB_Camera();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_CAMERA_NAME:
					{
						val.ModelName = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_ANIM_SEQ_ID:
					{
						val.AnimationSequenceId = BinaryFileHelper.ReadIntWithHeader(stream);
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
		
		public static CDB_Model FromStream(Stream stream)
		{
			CDB_Model val = new CDB_Model();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_MODEL_NAME_STATIC:
					{
						val.StaticModelName = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_MODEL_NAME:
					{
						val.ModelName = BinaryFileHelper.ReadStringWithHeader(stream);
						break;
					}
					case PROPERTY_MODEL_SPRITE_REFERENCE:
					{
						val.SpriteRefSceneId = BinaryFileHelper.ReadIntWithHeader(stream);
						val.SpriteRefItemId  = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_MODEL_LOCATION:
					{
						val.Location = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_MODEL_ROTATION:
					{
						val.RotationFwd = LRVector3.FromStream(stream);
						val.RotationUp  = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_ANIM_SEQ_ID:
					{
						val.AnimationSequenceId = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_UNKNOWN_36:
					{
						val.Unknown = BinaryFileHelper.ReadArrayBlock<CDB_5ints>(
							stream,
							new BinaryFileHelper.ReadObject<CDB_5ints>(
								CDB_5ints.FromStream
							)
						);
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
		
		public static CDB_AmbientLight FromStream(Stream stream)
		{
			CDB_AmbientLight val = new CDB_AmbientLight();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.EndFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_LIGHT_COLOR:
					{
						val.Color = LRColor.FromStreamNoAlpha(stream);
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
		
		public static CDB_Event FromStream(Stream stream)
		{
			CDB_Event val = new CDB_Event();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_MODEL_LOCATION:
					{
						val.Location = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_MODEL_ROTATION:
					{
						val.RotationFwd = LRVector3.FromStream(stream);
						val.RotationUp  = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.Duration = BinaryFileHelper.ReadIntWithHeader(stream);
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
		
		public static CDB_DirectionalLight FromStream(Stream stream)
		{
			CDB_DirectionalLight val = new CDB_DirectionalLight();
			while (BinaryFileHelper.Next(stream, BinaryFileHelper.TYPE_RIGHT_CURLY) == false)
			{
				byte property_id = BinaryFileHelper.ReadByte(stream);
				switch (property_id)
				{
					case PROPERTY_START_FRAME:
					{
						val.StartFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_DURATION:
					{
						val.EndFrame = BinaryFileHelper.ReadIntWithHeader(stream);
						break;
					}
					case PROPERTY_LIGHT_COLOR:
					{
						val.Color = LRColor.FromStreamNoAlpha(stream);
						break;
					}
					case PROPERTY_LIGHT_DIRECTION:
					{
						val.Direction = LRVector3.FromStream(stream);
						break;
					}
					case PROPERTY_UNKNOWN_3C:
					{
						val.Unknown3C    = new int[2];
						val.Unknown3C[0] = BinaryFileHelper.ReadIntWithHeader(stream);
						val.Unknown3C[1] = BinaryFileHelper.ReadIntWithHeader(stream);
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
	}
	
	public class CDB_5ints
	{
		public int
			Unknown1,
			Unknown2,
			Unknown3,
			Unknown4,
			Unknown5;
		
		public static CDB_5ints FromStream(Stream stream)
		{
			CDB_5ints val = new CDB_5ints();
			val.Unknown1 = BinaryFileHelper.ReadIntWithHeader(stream);
			val.Unknown2 = BinaryFileHelper.ReadIntWithHeader(stream);
			val.Unknown3 = BinaryFileHelper.ReadIntWithHeader(stream);
			val.Unknown4 = BinaryFileHelper.ReadIntWithHeader(stream);
			val.Unknown5 = BinaryFileHelper.ReadIntWithHeader(stream);
			return val;
		}
	}
}