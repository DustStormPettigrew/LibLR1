using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Track listing format.
	/// </summary>
	public class RCB
	{
		private const byte
			ID_TRACK = 0x27;

		private Dictionary<string, RCB_Track> m_tracks;

		public Dictionary<string, RCB_Track> Tracks { get { return m_tracks; } set { m_tracks = value; } }

		public RCB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public RCB(LRBinaryReader p_reader)
		{
			m_tracks = new Dictionary<string, RCB_Track>();
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_TRACK:
					{
						m_tracks = p_reader.ReadDictionaryBlock<RCB_Track>(
							RCB_Track.Read,
							ID_TRACK
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
			p_writer.WriteByte(ID_TRACK);
			p_writer.WriteDictionaryBlock<RCB_Track>(
				RCB_Track.Write,
				m_tracks,
				ID_TRACK
			);
		}
	}

	public class RCB_Track
	{
		private const byte
			PROPERTY_POSITION_IN_CIRCUIT = 0x28,
			PROPERTY_FOLDER              = 0x29,
			PROPERTY_CIRCUIT             = 0x2A,
			PROPERTY_TRACK_ID            = 0x2B,
			PROPERTY_MIRROR_FLAG         = 0x2C,
			PROPERTY_THEME_STRING        = 0x2D,
			PROPERTY_MASCOT              = 0x2E;

		public int    PositionInCircuit;
		public string Folder;
		public string Circuit;
		public int    NameIndex;
		public bool   Mirror;
		public string ThemeStr;
		public string Mascot;

		public RCB_Track()
			: this(0, "", "", 0, false, "", "") { }

		public RCB_Track(
			int p_positionincircuit,
			string p_folder,
			string p_circuit,
			int p_nameindex,
			bool p_mirror,
			string p_themestr,
			string p_mascot)
		{
			PositionInCircuit = p_positionincircuit;
			Folder            = p_folder;
			Circuit           = p_circuit;
			NameIndex         = p_nameindex;
			Mirror            = p_mirror;
			ThemeStr          = p_themestr;
			Mascot            = p_mascot;
		}

		public static RCB_Track Read(LRBinaryReader p_reader)
		{
			RCB_Track val = new RCB_Track();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propertyId = p_reader.ReadByte();
				switch (propertyId)
				{
					case PROPERTY_POSITION_IN_CIRCUIT:
					{
						val.PositionInCircuit = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_FOLDER:
					{
						val.Folder = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_CIRCUIT:
					{
						val.Circuit = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_TRACK_ID:
					{
						val.NameIndex = p_reader.ReadIntWithHeader();
						break;
					}
					case PROPERTY_MIRROR_FLAG:
					{
						val.Mirror = true;
						break;
					}
					case PROPERTY_THEME_STRING:
					{
						val.ThemeStr = p_reader.ReadStringWithHeader();
						break;
					}
					case PROPERTY_MASCOT:
					{
						val.Mascot = p_reader.ReadStringWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, RCB_Track p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION_IN_CIRCUIT);
			p_writer.WriteIntWithHeader(p_value.PositionInCircuit);
			p_writer.WriteByte(PROPERTY_FOLDER);
			p_writer.WriteStringWithHeader(p_value.Folder);
			if (p_value.Circuit != "")
			{
				p_writer.WriteByte(PROPERTY_CIRCUIT);
				p_writer.WriteStringWithHeader(p_value.Circuit);
			}
			p_writer.WriteByte(PROPERTY_TRACK_ID);
			p_writer.WriteIntWithHeader(p_value.NameIndex);
			if (p_value.Mirror)
			{
				p_writer.WriteByte(PROPERTY_MIRROR_FLAG);
			}
			if (p_value.ThemeStr != "")
			{
				p_writer.WriteByte(PROPERTY_THEME_STRING);
				p_writer.WriteStringWithHeader(p_value.ThemeStr);
			}
			if (p_value.Mascot != "")
			{
				p_writer.WriteByte(PROPERTY_MASCOT);
				p_writer.WriteStringWithHeader(p_value.Mascot);
			}
		}
	}
}