using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;

namespace LibLR1
{
	/// <summary>
	/// Track header format. Must be /GAMEDATA/<level>/<level>.RAB for the game to notice it.
	/// </summary>
	public class RAB
	{
		private const byte
			ID_TRACK = 0x35;

		private string m_trackTitle;
		private RAB_Track m_track;

		/// <summary>
		/// Not used by the game, as far as I can tell. It's "Magma Moon" in most of the files.
		/// </summary>
		public string TrackTitle
		{
			get { return m_trackTitle; }
			set { m_trackTitle = value; }
		}
		public RAB_Track Track
		{
			get { return m_track; }
			set { m_track = value; }
		}

		public RAB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public RAB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TRACK:
					{
						m_trackTitle = p_reader.ReadStringWithHeader();
						m_track = p_reader.ReadStruct<RAB_Track>(
							RAB_Track.Read
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

	public class RAB_Track
	{
		private const byte
			PROPERTY_BACKGROUND_SCENE = 0x27,
			PROPERTY_UNKNOWN_28 = 0x28,
			PROPERTY_UNKNOWN_29 = 0x29,
			PROPERTY_COLLISION_MESHES = 0x2B,
			PROPERTY_EVENT_SCRIPT_FILE = 0x2C,
			PROPERTY_FONT_FILE_REF = 0x2D,
			PROPERTY_TRACK_SCENE = 0x2E,
			PROPERTY_HUD_IMAGES_FILE = 0x2F,

			PROPERTY_MUSIC_LIST_FILE = 0x30,
			PROPERTY_TRACK_OBJECTS_FILE = 0x31,
			PROPERTY_PATH_FILE = 0x32,
			PROPERTY_POWERUP_FILES = 0x33,
			PROPERTY_PROJECTED_SHADOW_SCENE = 0x34,
			PROPERTY_ENVIRONMENT_TRIGGER_REF = 0x37,
			PROPERTY_TIMER_REF = 0x38,
			PROPERTY_TRACK_TRIGGER_REF = 0x39,
			PROPERTY_EMITTER_FILES = 0x3A,
			PROPERTY_BLENDED_SCENE = 0x3B,
			PROPERTY_COMMON_SOUNDS_FILE = 0x3C,
			PROPERTY_VOICE_BANK = 0x3D,
			PROPERTY_SOUND_LIST_FILE = 0x3F,

			PROPERTY_GLOBAL_WDF_REF = 0x40,
			PROPERTY_START_POS = 0x41,
			PROPERTY_SKYBOX = 0x42,
			PROPERTY_TRACK_MATERIAL = 0x43,
			PROPERTY_HAZARD_FILE = 0x44,
			PROPERTY_LOCAL_WDF_REF = 0x45,
			PROPERTY_BOUNDING_XZ = 0x46,
			PROPERTY_CHECKPOINT_FILES = 0x48,
			PROPERTY_CAMERA_RIG_BASENAME = 0x49,
			PROPERTY_TRACK_GAMEPLAY_REF = 0x4A;

		/// <summary>Logical .wdf ref resolving to a background/sky WDB scene. Evidence: backgrd.wdf -> WDB across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string BackgroundScene;
		public LRVector3 Unknown28_A, Unknown28_B;
		public LRVector3 Unknown29;

		/// <summary>
		/// [0] : primary collision mesh. Explicit extension (WD[F|B])
		/// [1] : secondary collision mesh. Implicit extension (BVB/MDB)
		/// [2] : startfin collision mesh. Implicit extension (BVB/MDB)
		/// </summary>
		public string[] CollisionMeshes;
		public string EventScriptFile;
		public string FontFileRef;
		public string TrackScene;
		public string HudImagesFile;
		public string MusicListFile;
		/// <summary>Logical .tob ref. No loose .TOB in install; JAM-resident suspected. Evidence: racecXrY.tob across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string TrackObjectsFile;
		/// <summary>Logical .pth ref. No loose .PTB in install; JAM-resident suspected. Evidence: theme-named .pth across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string PathFile;

		/// <summary>
		/// [0] : powerup layout. PW[F|B]
		/// [1] : powerup material animation from /COMMON. MA[F|B]
		/// [2] : powerup scene? from /COMMON. WD[F|B]
		/// </summary>
		public string[] PowerupFiles;
		/// <summary>Logical .wdf ref for a projected shadow WDB scene. File absent from loose install (JAM-resident suspected). Evidence: pshadow.wdf across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string ProjectedShadowScene;
		public string EnvironmentTriggerFileRef;
		public string TimerFileRef;
		public string MainTriggerFileRef;

		/// <summary>
		/// [0] global from /COMMON EM[T|B]
		/// [1] track specific EM[T|B]
		/// </summary>
		public string[] EmitterFiles;
		/// <summary>Logical .wdf ref resolving to a blended/alpha-objects WDB scene. Evidence: blended.wdf -> WDB across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string BlendedScene;
		public string CommonSoundsFile;
		/// <summary>Voice SBK bank for this circuit's opponents. Evidence: voiceCx -> VOICES/voiceCx.SBK across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string VoiceBank;
		public string SoundListFile;
		public string GlobalWdfRef;
		public string StartPosFile;
		public string SkyBoxFile;
		/// <summary>Logical .tmt ref resolving to track material physics TMB. Evidence: racecXrY.tmt -> TMB across all 14 tracks (03-Track-Loading-Graph.md).</summary>
		public string TrackMaterial;
		public string HazardFile;
		public string LocalWdfRef;
		public LRVector2 BoundingXY_A, BoundingXY_B;

		/// <summary>
		/// [0] raw checkpoint data. explicit extension. CP[F|B]
		/// [1] collision mesh. implicit extensions. BVB/MDB
		/// </summary>
		public string[] CheckpointFiles;
		public string CameraRigBasename;
		public string TrackGameplayRef;

		public static RAB_Track Read(LRBinaryReader p_reader)
		{
			RAB_Track val = new RAB_Track();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_BACKGROUND_SCENE:
					{
						val.BackgroundScene = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_28:
					{
						val.Unknown28_A = LRVector3.Read(p_reader);
						val.Unknown28_B = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_29:
					{
						val.Unknown29 = LRVector3.Read(p_reader);
						break;
					}
					case PROPERTY_COLLISION_MESHES:
					{
						val.CollisionMeshes = new string[3];
						for (int i = 0; i < val.CollisionMeshes.Length; i++)
						{
							val.CollisionMeshes[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_EVENT_SCRIPT_FILE:
					{
						val.EventScriptFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_FONT_FILE_REF:
					{
						val.FontFileRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_SCENE:
					{
						val.TrackScene = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_HUD_IMAGES_FILE:
					{
						val.HudImagesFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_MUSIC_LIST_FILE:
					{
						val.MusicListFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_OBJECTS_FILE:
					{
						val.TrackObjectsFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_PATH_FILE:
					{
						val.PathFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_POWERUP_FILES:
					{
						val.PowerupFiles = new string[3];
						for (int i = 0; i < val.PowerupFiles.Length; i++)
						{
							val.PowerupFiles[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_PROJECTED_SHADOW_SCENE:
					{
						val.ProjectedShadowScene = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_ENVIRONMENT_TRIGGER_REF:
					{
						val.EnvironmentTriggerFileRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TIMER_REF:
					{
						val.TimerFileRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_TRIGGER_REF:
					{
						val.MainTriggerFileRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_EMITTER_FILES:
					{
						val.EmitterFiles = new string[2];
						for (int i = 0; i < val.EmitterFiles.Length; i++)
						{
							val.EmitterFiles[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_BLENDED_SCENE:
					{
						val.BlendedScene = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_COMMON_SOUNDS_FILE:
					{
						val.CommonSoundsFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_VOICE_BANK:
					{
						val.VoiceBank = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_SOUND_LIST_FILE:
					{
						val.SoundListFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_GLOBAL_WDF_REF:
					{
						val.GlobalWdfRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_START_POS:
					{
						val.StartPosFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_SKYBOX:
					{
						val.SkyBoxFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_MATERIAL:
					{
						val.TrackMaterial = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_HAZARD_FILE:
					{
						val.HazardFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_LOCAL_WDF_REF:
					{
						val.LocalWdfRef = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_BOUNDING_XZ:
					{
						val.BoundingXY_A = LRVector2.Read(p_reader);
						val.BoundingXY_B = LRVector2.Read(p_reader);
						break;
					}
					case PROPERTY_CHECKPOINT_FILES:
					{
						val.CheckpointFiles = new string[2];
						for (int i = 0; i < val.CheckpointFiles.Length; i++)
						{
							val.CheckpointFiles[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_CAMERA_RIG_BASENAME:
					{
						val.CameraRigBasename = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_GAMEPLAY_REF:
					{
						val.TrackGameplayRef = p_reader.ReadStringWithHeader();
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
}
