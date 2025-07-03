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
			PROPERTY_UNKNOWN_27             = 0x27,
			PROPERTY_UNKNOWN_28             = 0x28,
			PROPERTY_UNKNOWN_29             = 0x29,
			PROPERTY_MAYBE_COLLISION_MESHES = 0x2B,
			PROPERTY_EVENT_SCRIPT_FILE      = 0x2C,
			PROPERTY_UNKNOWN_2D             = 0x2D,
			PROPERTY_TRACK_SCENE            = 0x2E,
			PROPERTY_HUD_IMAGES_FILE        = 0x2F,

			PROPERTY_MUSIC_LIST_FILE    = 0x30,
			PROPERTY_UNKNOWN_31         = 0x31,
			PROPERTY_UNKNOWN_32         = 0x32,
			PROPERTY_POWERUP_FILES      = 0x33,
			PROPERTY_UNKNOWN_34         = 0x34,
			PROPERTY_UNKNOWN_37         = 0x37,
			PROPERTY_UNKNOWN_38         = 0x38,
			PROPERTY_UNKNOWN_39         = 0x39,
			PROPERTY_EMITTER_FILES      = 0x3A,
			PROPERTY_UNKNOWN_3B         = 0x3B,
			PROPERTY_COMMON_SOUNDS_FILE = 0x3C,
			PROPERTY_UNKNOWN_3D         = 0x3D,
			PROPERTY_SOUND_LIST_FILE    = 0x3F,

			PROPERTY_UNKNOWN_40  = 0x40,
			PROPERTY_START_POS   = 0x41,
			PROPERTY_SKYBOX      = 0x42,
			PROPERTY_UNKNOWN_43  = 0x43,
			PROPERTY_HAZARD_FILE = 0x44,
			PROPERTY_UNKNOWN_45  = 0x45,
			PROPERTY_BOUNDING_XZ = 0x46,
			PROPERTY_UNKNOWN_48  = 0x48,
			PROPERTY_UNKNOWN_49  = 0x49,
			PROPERTY_UNKNOWN_4A  = 0x4A;

		public string Unknown27;
		public LRVector3 Unknown28_A, Unknown28_B;
		public LRVector3 Unknown29;

		/// <summary>
		/// [0] : primary collision mesh. Explicit extension (WD[F|B])
		/// [1] : secondary collision mesh. Implicit extension (BVB/MDB)
		/// [2] : startfin collision mesh. Implicit extension (BVB/MDB)
		/// </summary>
		public string[] MaybeCollisionMeshes;
		public string EventScriptFile;
		public string Unknown2D;
		public string MaybeTrackScene;
		public string HudImagesFile;
		public string MusicListFile;
		public string Unknown31;
		public string Unknown32;

		/// <summary>
		/// [0] : powerup layout. PW[F|B]
		/// [1] : powerup material animation from /COMMON. MA[F|B]
		/// [2] : powerup scene? from /COMMON. WD[F|B]
		/// </summary>
		public string[] PowerupFiles;
		public string Unknown34;
		public string Unknown37;
		public string Unknown38;
		public string Unknown39;

		/// <summary>
		/// [0] global from /COMMON  EM[T|B]
		/// [1] track specific  EM[T|B]
		/// </summary>
		public string[] EmitterFiles;
		public string Unknown3B;
		public string CommonSoundsFile;
		public string Unknown3D;
		public string SoundListFile;
		public string Unknown40;
		public string StartPosFile;
		public string SkyBoxFile;
		public string Unknown43;
		public string HazardFile;
		public string Unknown45;
		public LRVector2 BoundingXY_A, BoundingXY_B;

		/// <summary>
		/// [0] raw checkpoint data. explicit extension. CP[F|B]
		/// [1] collision mesh. implicit extensions. BVB/MDB
		/// </summary>
		public string[] CheckpointFiles;
		public string Unknown49;
		public string Unknown4A;

		public static RAB_Track Read(LRBinaryReader p_reader)
		{
			RAB_Track val = new RAB_Track();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_UNKNOWN_27:
					{
						val.Unknown27 = p_reader.ReadStringWithHeader();
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
					case PROPERTY_MAYBE_COLLISION_MESHES:
					{
						val.MaybeCollisionMeshes = new string[3];
						for (int i = 0; i < val.MaybeCollisionMeshes.Length; i++)
						{
							val.MaybeCollisionMeshes[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_EVENT_SCRIPT_FILE:
					{
						val.EventScriptFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_2D:
					{
						val.Unknown2D = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_SCENE:
					{
						val.MaybeTrackScene = p_reader.ReadStringWithHeader();
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
					case PROPERTY_UNKNOWN_31:
					{
						val.Unknown31 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_32:
					{
						val.Unknown32 = p_reader.ReadStringWithHeader();
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
					case PROPERTY_UNKNOWN_34:
					{
						val.Unknown34 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_37:
					{
						val.Unknown37 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_38:
					{
						val.Unknown38 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_39:
					{
						val.Unknown39 = p_reader.ReadStringWithHeader();
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
					case PROPERTY_UNKNOWN_3B:
					{
						val.Unknown3B = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_COMMON_SOUNDS_FILE:
					{
						val.CommonSoundsFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_3D:
					{
						val.Unknown3D = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_SOUND_LIST_FILE:
					{
						val.SoundListFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_40:
					{
						val.Unknown40 = p_reader.ReadStringWithHeader();
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
					case PROPERTY_UNKNOWN_43:
					{
						val.Unknown43 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_HAZARD_FILE:
					{
						val.HazardFile = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_45:
					{
						val.Unknown45 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_BOUNDING_XZ:
					{
						val.BoundingXY_A = LRVector2.Read(p_reader);
						val.BoundingXY_B = LRVector2.Read(p_reader);
						break;
					}
					case PROPERTY_UNKNOWN_48:
					{
						val.CheckpointFiles = new string[2];
						for (int i = 0; i < val.CheckpointFiles.Length; i++)
						{
							val.CheckpointFiles[i] = p_reader.ReadStringWithHeader();
						}
						break;
					}
					case PROPERTY_UNKNOWN_49:
					{
						val.Unknown49 = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_UNKNOWN_4A:
					{
						val.Unknown4A = p_reader.ReadStringWithHeader();
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