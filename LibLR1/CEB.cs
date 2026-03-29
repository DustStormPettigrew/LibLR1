using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Cutscene Event format. Defines cutscene data including scene names, language strings, effects, sounds, text displays, and event sequencing.
	/// </summary>
	public class CEB
	{
		private const byte
			ID_SCENE_NAMES = 0x27,
			ID_LANGUAGES = 0x28,
			ID_IMAGES = 0x2A,
			ID_VERTEX_COLORS = 0x2B,
			ID_SOUNDS = 0x2F,
			ID_PARTICLES = 0x3C,
			ID_TEXT_DISPLAYS = 0x3F,
			ID_MOVIES = 0x4D,
			ID_START_EVENTS = 0x56,
			ID_END_EVENTS = 0x57,
			ID_CUTSCENE_NAMES = 0x5C,
			ID_EFFECTS = 0x60;

		private string[] m_sceneNames;
		private string[] m_languages;
		private string[] m_images;
		private string[] m_cutsceneNames;
		private Dictionary<string, CEB_Movie> m_movies;
		private Dictionary<string, CEB_Effect> m_effects;
		private Dictionary<string, CEB_Sound> m_sounds;
		private Dictionary<string, CEB_Particle> m_particles;
		private Dictionary<string, CEB_VertexColor> m_vertexColors;
		private Dictionary<string, CEB_TextDisplay> m_textDisplays;
		private KeyValuePair<string, CEB_Sequence>[] m_startEvents;
		private KeyValuePair<string, CEB_Sequence>[] m_endEvents;

		public string[] SceneNames
		{
			get { return m_sceneNames; }
			set { m_sceneNames = value; }
		}

		public string[] Languages
		{
			get { return m_languages; }
			set { m_languages = value; }
		}

		public string[] Images
		{
			get { return m_images; }
			set { m_images = value; }
		}

		public string[] CutsceneNames
		{
			get { return m_cutsceneNames; }
			set { m_cutsceneNames = value; }
		}

		public Dictionary<string, CEB_Movie> Movies
		{
			get { return m_movies; }
			set { m_movies = value; }
		}

		public Dictionary<string, CEB_Effect> Effects
		{
			get { return m_effects; }
			set { m_effects = value; }
		}

		public Dictionary<string, CEB_Sound> Sounds
		{
			get { return m_sounds; }
			set { m_sounds = value; }
		}

		public Dictionary<string, CEB_Particle> Particles
		{
			get { return m_particles; }
			set { m_particles = value; }
		}

		public Dictionary<string, CEB_VertexColor> VertexColors
		{
			get { return m_vertexColors; }
			set { m_vertexColors = value; }
		}

		public Dictionary<string, CEB_TextDisplay> TextDisplays
		{
			get { return m_textDisplays; }
			set { m_textDisplays = value; }
		}

		public KeyValuePair<string, CEB_Sequence>[] StartEvents
		{
			get { return m_startEvents; }
			set { m_startEvents = value; }
		}

		public KeyValuePair<string, CEB_Sequence>[] EndEvents
		{
			get { return m_endEvents; }
			set { m_endEvents = value; }
		}

		public CEB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public CEB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_SCENE_NAMES:
					{
						m_sceneNames = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_LANGUAGES:
					{
						m_languages = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_IMAGES:
					{
						m_images = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_CUTSCENE_NAMES:
					{
						m_cutsceneNames = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_MOVIES:
					{
						m_movies = p_reader.ReadDictionaryBlock<CEB_Movie>(
							CEB_Movie.Read,
							ID_MOVIES
						);
						break;
					}
					case ID_EFFECTS:
					{
						m_effects = p_reader.ReadDictionaryBlock<CEB_Effect>(
							CEB_Effect.Read,
							ID_EFFECTS
						);
						break;
					}
					case ID_SOUNDS:
					{
						m_sounds = p_reader.ReadDictionaryBlock<CEB_Sound>(
							CEB_Sound.Read,
							ID_SOUNDS
						);
						break;
					}
					case ID_PARTICLES:
					{
						m_particles = p_reader.ReadDictionaryBlock<CEB_Particle>(
							CEB_Particle.Read,
							ID_PARTICLES
						);
						break;
					}
					case ID_VERTEX_COLORS:
					{
						m_vertexColors = p_reader.ReadDictionaryBlock<CEB_VertexColor>(
							CEB_VertexColor.Read,
							ID_VERTEX_COLORS
						);
						break;
					}
					case ID_TEXT_DISPLAYS:
					{
						m_textDisplays = p_reader.ReadDictionaryBlock<CEB_TextDisplay>(
							CEB_TextDisplay.Read,
							ID_TEXT_DISPLAYS
						);
						break;
					}
					case ID_START_EVENTS:
					{
						m_startEvents = p_reader.ReadCollidableDictionaryBlock<CEB_Sequence>(
							CEB_Sequence.ReadStart,
							ID_START_EVENTS
						);
						break;
					}
					case ID_END_EVENTS:
					{
						m_endEvents = p_reader.ReadCollidableDictionaryBlock<CEB_Sequence>(
							CEB_Sequence.ReadEnd,
							ID_END_EVENTS
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
			if (m_sceneNames != null)
			{
				p_writer.WriteByte(ID_SCENE_NAMES);
				p_writer.WriteStringArrayBlock(m_sceneNames);
			}

			if (m_languages != null)
			{
				p_writer.WriteByte(ID_LANGUAGES);
				p_writer.WriteStringArrayBlock(m_languages);
			}

			if (m_images != null)
			{
				p_writer.WriteByte(ID_IMAGES);
				p_writer.WriteStringArrayBlock(m_images);
			}

			if (m_cutsceneNames != null)
			{
				p_writer.WriteByte(ID_CUTSCENE_NAMES);
				p_writer.WriteStringArrayBlock(m_cutsceneNames);
			}

			if (m_movies != null)
			{
				p_writer.WriteByte(ID_MOVIES);
				p_writer.WriteDictionaryBlock<CEB_Movie>(
					CEB_Movie.Write,
					m_movies,
					ID_MOVIES
				);
			}

			if (m_effects != null)
			{
				p_writer.WriteByte(ID_EFFECTS);
				p_writer.WriteDictionaryBlock<CEB_Effect>(
					CEB_Effect.Write,
					m_effects,
					ID_EFFECTS
				);
			}

			if (m_sounds != null)
			{
				p_writer.WriteByte(ID_SOUNDS);
				p_writer.WriteDictionaryBlock<CEB_Sound>(
					CEB_Sound.Write,
					m_sounds,
					ID_SOUNDS
				);
			}

			if (m_particles != null)
			{
				p_writer.WriteByte(ID_PARTICLES);
				p_writer.WriteDictionaryBlock<CEB_Particle>(
					CEB_Particle.Write,
					m_particles,
					ID_PARTICLES
				);
			}

			if (m_vertexColors != null)
			{
				p_writer.WriteByte(ID_VERTEX_COLORS);
				p_writer.WriteDictionaryBlock<CEB_VertexColor>(
					CEB_VertexColor.Write,
					m_vertexColors,
					ID_VERTEX_COLORS
				);
			}

			if (m_textDisplays != null)
			{
				p_writer.WriteByte(ID_TEXT_DISPLAYS);
				p_writer.WriteDictionaryBlock<CEB_TextDisplay>(
					CEB_TextDisplay.Write,
					m_textDisplays,
					ID_TEXT_DISPLAYS
				);
			}

			if (m_startEvents != null)
			{
				p_writer.WriteByte(ID_START_EVENTS);
				p_writer.WriteCollidableDictionaryBlock<CEB_Sequence>(
					CEB_Sequence.WriteStart,
					m_startEvents,
					ID_START_EVENTS
				);
			}

			if (m_endEvents != null)
			{
				p_writer.WriteByte(ID_END_EVENTS);
				p_writer.WriteCollidableDictionaryBlock<CEB_Sequence>(
					CEB_Sequence.WriteEnd,
					m_endEvents,
					ID_END_EVENTS
				);
			}
		}
	}

	public class CEB_Effect
	{
		private const byte
			PROPERTY_DURATION = 0x61,
			PROPERTY_FADE_IN = 0x62,
			PROPERTY_UNKNOWN_63 = 0x63,
			PROPERTY_FADE_OUT = 0x64,
			PROPERTY_COLOR = 0x66;

		public int Duration;
		public bool FadeIn;
		public bool HasUnknown63;
		public bool FadeOut;
		public bool HasColor;
		public int ColorR;
		public int ColorG;
		public int ColorB;

		public static CEB_Effect Read(LRBinaryReader p_reader)
		{
			CEB_Effect val = new CEB_Effect();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_DURATION:
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_FADE_IN:
						val.FadeIn = true;
						break;
					case PROPERTY_UNKNOWN_63:
						val.HasUnknown63 = true;
						break;
					case PROPERTY_FADE_OUT:
						val.FadeOut = true;
						break;
					case PROPERTY_COLOR:
						val.HasColor = true;
						val.ColorR = p_reader.ReadIntWithHeader();
						val.ColorG = p_reader.ReadIntWithHeader();
						val.ColorB = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, CEB_Effect p_value)
		{
			p_writer.WriteByte(0x61);
			p_writer.WriteIntWithHeader(p_value.Duration);

			if (p_value.FadeIn)
				p_writer.WriteByte(0x62);

			if (p_value.HasUnknown63)
				p_writer.WriteByte(0x63);

			if (p_value.FadeOut)
				p_writer.WriteByte(0x64);

			if (p_value.HasColor)
			{
				p_writer.WriteByte(0x66);
				p_writer.WriteIntWithHeader(p_value.ColorR);
				p_writer.WriteIntWithHeader(p_value.ColorG);
				p_writer.WriteIntWithHeader(p_value.ColorB);
			}
		}
	}

	public class CEB_Sound
	{
		private const byte
			PROPERTY_INDICES = 0x30,
			PROPERTY_VOLUME = 0x32,
			PROPERTY_UNKNOWN_33 = 0x33,
			PROPERTY_PITCH = 0x34,
			PROPERTY_LOOP = 0x35;

		public int Index1;
		public int Index2;
		public bool HasVolume;
		public float Volume;
		public bool HasUnknown33;
		public float Unknown33;
		public bool HasPitch;
		public float Pitch;
		public bool Loop;

		public static CEB_Sound Read(LRBinaryReader p_reader)
		{
			CEB_Sound val = new CEB_Sound();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_INDICES:
						val.Index1 = p_reader.ReadIntWithHeader();
						val.Index2 = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_VOLUME:
						val.HasVolume = true;
						val.Volume = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_UNKNOWN_33:
						val.HasUnknown33 = true;
						val.Unknown33 = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_PITCH:
						val.HasPitch = true;
						val.Pitch = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_LOOP:
						val.Loop = true;
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

		public static void Write(LRBinaryWriter p_writer, CEB_Sound p_value)
		{
			p_writer.WriteByte(0x30);
			p_writer.WriteIntWithHeader(p_value.Index1);
			p_writer.WriteIntWithHeader(p_value.Index2);

			if (p_value.Loop)
				p_writer.WriteByte(0x35);

			if (p_value.HasUnknown33)
			{
				p_writer.WriteByte(0x33);
				p_writer.WriteFloatWithHeader(p_value.Unknown33);
			}

			if (p_value.HasPitch)
			{
				p_writer.WriteByte(0x34);
				p_writer.WriteFloatWithHeader(p_value.Pitch);
			}

			if (p_value.HasVolume)
			{
				p_writer.WriteByte(0x32);
				p_writer.WriteFloatWithHeader(p_value.Volume);
			}
		}
	}

	public class CEB_Particle
	{
		private const byte
			PROPERTY_DATA = 0x3D;

		public int Index;
		public string EmitterName;

		public static CEB_Particle Read(LRBinaryReader p_reader)
		{
			CEB_Particle val = new CEB_Particle();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_DATA:
						val.Index = p_reader.ReadIntWithHeader();
						val.EmitterName = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, CEB_Particle p_value)
		{
			p_writer.WriteByte(0x3D);
			p_writer.WriteIntWithHeader(p_value.Index);
			p_writer.WriteStringWithHeader(p_value.EmitterName);
		}
	}

	public class CEB_VertexColor
	{
		private const byte
			PROPERTY_COLOR = 0x2D,
			PROPERTY_MODEL_A = 0x5D,
			PROPERTY_MODEL_B = 0x5E;

		public int ColorR;
		public int ColorG;
		public int ColorB;
		public string ModelA;
		public string ModelB;

		public static CEB_VertexColor Read(LRBinaryReader p_reader)
		{
			CEB_VertexColor val = new CEB_VertexColor();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_COLOR:
						val.ColorR = p_reader.ReadIntWithHeader();
						val.ColorG = p_reader.ReadIntWithHeader();
						val.ColorB = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_MODEL_A:
						val.ModelA = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_MODEL_B:
						val.ModelB = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, CEB_VertexColor p_value)
		{
			p_writer.WriteByte(0x2D);
			p_writer.WriteIntWithHeader(p_value.ColorR);
			p_writer.WriteIntWithHeader(p_value.ColorG);
			p_writer.WriteIntWithHeader(p_value.ColorB);

			if (p_value.ModelA != null)
			{
				p_writer.WriteByte(0x5D);
				p_writer.WriteStringWithHeader(p_value.ModelA);
			}

			if (p_value.ModelB != null)
			{
				p_writer.WriteByte(0x5E);
				p_writer.WriteStringWithHeader(p_value.ModelB);
			}
		}
	}

	public class CEB_TextDisplay
	{
		private const byte
			PROPERTY_POSITION = 0x40,
			PROPERTY_FONT = 0x42,
			PROPERTY_SCALE = 0x44,
			PROPERTY_CENTERED = 0x45,
			PROPERTY_COLOR = 0x66;

		public int PositionX;
		public int PositionY;
		public string Font;
		public bool HasScale;
		public float Scale;
		public bool Centered;
		public bool HasColor;
		public int ColorR;
		public int ColorG;
		public int ColorB;

		public static CEB_TextDisplay Read(LRBinaryReader p_reader)
		{
			CEB_TextDisplay val = new CEB_TextDisplay();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_POSITION:
						val.PositionX = p_reader.ReadIntWithHeader();
						val.PositionY = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_FONT:
						val.Font = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_SCALE:
						val.HasScale = true;
						val.Scale = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_CENTERED:
						val.Centered = true;
						break;
					case PROPERTY_COLOR:
						val.HasColor = true;
						val.ColorR = p_reader.ReadIntWithHeader();
						val.ColorG = p_reader.ReadIntWithHeader();
						val.ColorB = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, CEB_TextDisplay p_value)
		{
			p_writer.WriteByte(0x40);
			p_writer.WriteIntWithHeader(p_value.PositionX);
			p_writer.WriteIntWithHeader(p_value.PositionY);

			if (p_value.Font != null)
			{
				p_writer.WriteByte(0x42);
				p_writer.WriteStringWithHeader(p_value.Font);
			}

			if (p_value.Centered)
				p_writer.WriteByte(0x45);

			if (p_value.HasScale)
			{
				p_writer.WriteByte(0x44);
				p_writer.WriteFloatWithHeader(p_value.Scale);
			}

			if (p_value.HasColor)
			{
				p_writer.WriteByte(0x66);
				p_writer.WriteIntWithHeader(p_value.ColorR);
				p_writer.WriteIntWithHeader(p_value.ColorG);
				p_writer.WriteIntWithHeader(p_value.ColorB);
			}
		}
	}

	public class CEB_Movie
	{
		private const byte
			PROPERTY_NAME = 0x4D,
			PROPERTY_UNKNOWN_45 = 0x45,
			PROPERTY_UNKNOWN_46 = 0x46;

		public string Name;
		public bool HasUnknown45;
		public bool HasUnknown46;

		public static CEB_Movie Read(LRBinaryReader p_reader)
		{
			CEB_Movie val = new CEB_Movie();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_NAME:
						val.Name = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_UNKNOWN_45:
						val.HasUnknown45 = true;
						break;
					case PROPERTY_UNKNOWN_46:
						val.HasUnknown46 = true;
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

		public static void Write(LRBinaryWriter p_writer, CEB_Movie p_value)
		{
			if (p_value.Name != null)
			{
				p_writer.WriteByte(PROPERTY_NAME);
				p_writer.WriteStringWithHeader(p_value.Name);
			}

			if (p_value.HasUnknown45)
				p_writer.WriteByte(PROPERTY_UNKNOWN_45);

			if (p_value.HasUnknown46)
				p_writer.WriteByte(PROPERTY_UNKNOWN_46);
		}
	}

	public class CEB_Sequence
	{
		public byte TargetType;
		public string TargetRef;

		public static CEB_Sequence ReadStart(LRBinaryReader p_reader)
		{
			CEB_Sequence val = new CEB_Sequence();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				if (propId == 0x4E)
				{
					val.TargetRef = p_reader.ReadStringWithHeader();
				}
				else
				{
					val.TargetType = propId;
				}
			}
			return val;
		}

		public static CEB_Sequence ReadEnd(LRBinaryReader p_reader)
		{
			CEB_Sequence val = new CEB_Sequence();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				if (propId == 0x4F)
				{
					val.TargetRef = p_reader.ReadStringWithHeader();
				}
				else
				{
					val.TargetType = propId;
				}
			}
			return val;
		}

		public static void WriteStart(LRBinaryWriter p_writer, CEB_Sequence p_value)
		{
			p_writer.WriteByte(p_value.TargetType);
			p_writer.WriteByte(0x4E);
			p_writer.WriteStringWithHeader(p_value.TargetRef);
		}

		public static void WriteEnd(LRBinaryWriter p_writer, CEB_Sequence p_value)
		{
			p_writer.WriteByte(p_value.TargetType);
			p_writer.WriteByte(0x4F);
			p_writer.WriteStringWithHeader(p_value.TargetRef);
		}
	}
}
