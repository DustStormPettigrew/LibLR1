using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// Hazard List format. Defines track hazards including rotating objects, destructibles, surface effects, water zones, and animated obstacles.
	/// </summary>
	public class HZB
	{
		private const byte
			ID_HAZARDS = 0x27;

		private List<HZB_Entry> m_entries;

		public List<HZB_Entry> Entries
		{
			get { return m_entries; }
			set { m_entries = value; }
		}

		public HZB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public HZB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_HAZARDS:
					{
						p_reader.Expect(Token.LeftBracket);
						int count = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						m_entries = new List<HZB_Entry>();
						while (!p_reader.Next(Token.RightCurly))
						{
							m_entries.Add(HZB_Entry.Read(p_reader));
						}
						p_reader.Expect(Token.RightCurly);
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
			if (m_entries != null)
			{
				p_writer.WriteByte(ID_HAZARDS);
				p_writer.WriteToken(Token.LeftBracket);
				int count = 0;
				foreach (var entry in m_entries)
					count += entry.CountItems();
				p_writer.WriteIntWithHeader(count);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				foreach (var entry in m_entries)
				{
					HZB_Entry.Write(p_writer, entry);
				}
				p_writer.WriteToken(Token.RightCurly);
			}
		}
	}

	public class HZB_Entry
	{
		private const byte
			FLAG_UNKNOWN_28 = 0x28,
			FLAG_UNKNOWN_29 = 0x29,
			FLAG_UNKNOWN_2A = 0x2A,
			FLAG_UNKNOWN_2B = 0x2B,
			FLAG_UNKNOWN_2C = 0x2C,
			FLAG_UNKNOWN_2D = 0x2D,
			FLAG_UNKNOWN_2E = 0x2E,
			FLAG_UNKNOWN_2F = 0x2F,
			FLAG_UNKNOWN_30 = 0x30,
			FLAG_UNKNOWN_32 = 0x32,
			FLAG_UNKNOWN_3F = 0x3F,
			FLAG_UNKNOWN_49 = 0x49,
			TYPE_PATH = 0x33,
			TYPE_DESTRUCTIBLE = 0x34,
			TYPE_SURFACE = 0x36,
			TYPE_WATER_ZONE = 0x3D,
			TYPE_ROTATING = 0x3E,
			TYPE_SPINNING = 0x40,
			TYPE_ANIMATED = 0x43,
			TYPE_FLYING = 0x48;

		public byte Type;

		// Flag entries (bare byte, no data)

		// Surface (0x36): inline string + 2 floats
		public string SurfaceModel;
		public float SurfaceParam1;
		public float SurfaceParam2;

		// Rotating (0x3E): struct { string, int, 4 floats }
		public string RotatingModel;
		public int RotatingCheckpoint;
		public float RotatingParam1;
		public float RotatingParam2;
		public float RotatingParam3;
		public float RotatingParam4;

		// Destructible (0x34): struct { 0x41 string, multiple 0x42 strings, 0x3B int }
		public string DestructibleCollision;
		public List<string> DestructibleModels;
		public int DestructibleCheckpoint;

		// Path (0x33): struct { 0x37 vec3, 0x38 vec3, 0x39 vec3, 0x3A float, 0x3B int }
		public HZB_PathHazard PathData;

		// Water Zone (0x3D): struct { 0x37 vertex array, 0x38 vertex array, 0x33 path sub-struct }
		public HZB_WaterZone WaterZoneData;

		// Spinning (0x40): struct { 0x3B int, opt 0x37 vec3, opt 0x42 string, 0x46 int, 0x47 3 floats }
		public HZB_SpinningHazard SpinningData;

		// Animated (0x43): struct { 0x42 string, 0x3B int, 0x44 int }
		public string AnimatedModel;
		public int AnimatedCheckpoint;
		public int AnimatedDuration;

		// Flying (0x48): struct { 0x3B int, 0x42 string, 3 floats }
		public int FlyingCheckpoint;
		public string FlyingModel;
		public float FlyingParam1;
		public float FlyingParam2;
		public float FlyingParam3;

		public int CountItems()
		{
			return 1;
		}

		public static HZB_Entry Read(LRBinaryReader p_reader)
		{
			HZB_Entry val = new HZB_Entry();
			val.Type = p_reader.ReadByte();

			switch (val.Type)
			{
				case FLAG_UNKNOWN_28:
				case FLAG_UNKNOWN_29:
				case FLAG_UNKNOWN_2A:
				case FLAG_UNKNOWN_2B:
				case FLAG_UNKNOWN_2C:
				case FLAG_UNKNOWN_2D:
				case FLAG_UNKNOWN_2E:
				case FLAG_UNKNOWN_2F:
				case FLAG_UNKNOWN_30:
				case FLAG_UNKNOWN_32:
				case FLAG_UNKNOWN_3F:
				case FLAG_UNKNOWN_49:
					break;

				case TYPE_SURFACE:
				{
					val.SurfaceModel = p_reader.ReadStringWithHeader();
					val.SurfaceParam1 = p_reader.ReadFloatWithHeader();
					val.SurfaceParam2 = p_reader.ReadFloatWithHeader();
					break;
				}

				case TYPE_ROTATING:
				{
					p_reader.Expect(Token.LeftCurly);
					val.RotatingModel = p_reader.ReadStringWithHeader();
					val.RotatingCheckpoint = p_reader.ReadIntWithHeader();
					val.RotatingParam1 = p_reader.ReadFloatWithHeader();
					val.RotatingParam2 = p_reader.ReadFloatWithHeader();
					val.RotatingParam3 = p_reader.ReadFloatWithHeader();
					val.RotatingParam4 = p_reader.ReadFloatWithHeader();
					p_reader.Expect(Token.RightCurly);
					break;
				}

				case TYPE_DESTRUCTIBLE:
				{
					p_reader.Expect(Token.LeftCurly);
					val.DestructibleModels = new List<string>();
					while (!p_reader.Next(Token.RightCurly))
					{
						byte propId = p_reader.ReadByte();
						switch (propId)
						{
							case 0x41:
								val.DestructibleCollision = p_reader.ReadStringWithHeader();
								break;
							case 0x42:
								val.DestructibleModels.Add(p_reader.ReadStringWithHeader());
								break;
							case 0x3B:
								val.DestructibleCheckpoint = p_reader.ReadIntWithHeader();
								break;
							default:
								throw new UnexpectedPropertyException(
									propId,
									p_reader.BaseStream.Position - 1
								);
						}
					}
					p_reader.Expect(Token.RightCurly);
					break;
				}

				case TYPE_PATH:
				{
					val.PathData = p_reader.ReadStruct<HZB_PathHazard>(HZB_PathHazard.Read);
					break;
				}

				case TYPE_WATER_ZONE:
				{
					val.WaterZoneData = p_reader.ReadStruct<HZB_WaterZone>(HZB_WaterZone.Read);
					break;
				}

				case TYPE_SPINNING:
				{
					val.SpinningData = p_reader.ReadStruct<HZB_SpinningHazard>(HZB_SpinningHazard.Read);
					break;
				}

				case TYPE_ANIMATED:
				{
					p_reader.Expect(Token.LeftCurly);
					while (!p_reader.Next(Token.RightCurly))
					{
						byte propId = p_reader.ReadByte();
						switch (propId)
						{
							case 0x42:
								val.AnimatedModel = p_reader.ReadStringWithHeader();
								break;
							case 0x3B:
								val.AnimatedCheckpoint = p_reader.ReadIntWithHeader();
								break;
							case 0x44:
								val.AnimatedDuration = p_reader.ReadIntWithHeader();
								break;
							default:
								throw new UnexpectedPropertyException(
									propId,
									p_reader.BaseStream.Position - 1
								);
						}
					}
					p_reader.Expect(Token.RightCurly);
					break;
				}

				case TYPE_FLYING:
				{
					p_reader.Expect(Token.LeftCurly);
					while (!p_reader.Next(Token.RightCurly))
					{
						byte propId = p_reader.ReadByte();
						switch (propId)
						{
							case 0x3B:
								val.FlyingCheckpoint = p_reader.ReadIntWithHeader();
								break;
							case 0x42:
								val.FlyingModel = p_reader.ReadStringWithHeader();
								val.FlyingParam1 = p_reader.ReadFloatWithHeader();
								val.FlyingParam2 = p_reader.ReadFloatWithHeader();
								val.FlyingParam3 = p_reader.ReadFloatWithHeader();
								break;
							default:
								throw new UnexpectedPropertyException(
									propId,
									p_reader.BaseStream.Position - 1
								);
						}
					}
					p_reader.Expect(Token.RightCurly);
					break;
				}

				default:
				{
					throw new UnexpectedBlockException(
						val.Type,
						p_reader.BaseStream.Position - 1
					);
				}
			}

			return val;
		}

		public static void Write(LRBinaryWriter p_writer, HZB_Entry p_value)
		{
			p_writer.WriteByte(p_value.Type);

			switch (p_value.Type)
			{
				case FLAG_UNKNOWN_28:
				case FLAG_UNKNOWN_29:
				case FLAG_UNKNOWN_2A:
				case FLAG_UNKNOWN_2B:
				case FLAG_UNKNOWN_2C:
				case FLAG_UNKNOWN_2D:
				case FLAG_UNKNOWN_2E:
				case FLAG_UNKNOWN_2F:
				case FLAG_UNKNOWN_30:
				case FLAG_UNKNOWN_32:
				case FLAG_UNKNOWN_3F:
				case FLAG_UNKNOWN_49:
					break;

				case TYPE_SURFACE:
					p_writer.WriteStringWithHeader(p_value.SurfaceModel);
					p_writer.WriteFloatWithHeader(p_value.SurfaceParam1);
					p_writer.WriteFloatWithHeader(p_value.SurfaceParam2);
					break;

				case TYPE_ROTATING:
					p_writer.WriteToken(Token.LeftCurly);
					p_writer.WriteStringWithHeader(p_value.RotatingModel);
					p_writer.WriteIntWithHeader(p_value.RotatingCheckpoint);
					p_writer.WriteFloatWithHeader(p_value.RotatingParam1);
					p_writer.WriteFloatWithHeader(p_value.RotatingParam2);
					p_writer.WriteFloatWithHeader(p_value.RotatingParam3);
					p_writer.WriteFloatWithHeader(p_value.RotatingParam4);
					p_writer.WriteToken(Token.RightCurly);
					break;

				case TYPE_DESTRUCTIBLE:
					p_writer.WriteToken(Token.LeftCurly);
					if (p_value.DestructibleCollision != null)
					{
						p_writer.WriteByte(0x41);
						p_writer.WriteStringWithHeader(p_value.DestructibleCollision);
					}
					if (p_value.DestructibleModels != null)
					{
						foreach (string model in p_value.DestructibleModels)
						{
							p_writer.WriteByte(0x42);
							p_writer.WriteStringWithHeader(model);
						}
					}
					p_writer.WriteByte(0x3B);
					p_writer.WriteIntWithHeader(p_value.DestructibleCheckpoint);
					p_writer.WriteToken(Token.RightCurly);
					break;

				case TYPE_PATH:
					p_writer.WriteStruct<HZB_PathHazard>(HZB_PathHazard.Write, p_value.PathData);
					break;

				case TYPE_WATER_ZONE:
					p_writer.WriteStruct<HZB_WaterZone>(HZB_WaterZone.Write, p_value.WaterZoneData);
					break;

				case TYPE_SPINNING:
					p_writer.WriteStruct<HZB_SpinningHazard>(HZB_SpinningHazard.Write, p_value.SpinningData);
					break;

				case TYPE_ANIMATED:
					p_writer.WriteToken(Token.LeftCurly);
					if (p_value.AnimatedModel != null)
					{
						p_writer.WriteByte(0x42);
						p_writer.WriteStringWithHeader(p_value.AnimatedModel);
					}
					p_writer.WriteByte(0x3B);
					p_writer.WriteIntWithHeader(p_value.AnimatedCheckpoint);
					p_writer.WriteByte(0x44);
					p_writer.WriteIntWithHeader(p_value.AnimatedDuration);
					p_writer.WriteToken(Token.RightCurly);
					break;

				case TYPE_FLYING:
					p_writer.WriteToken(Token.LeftCurly);
					p_writer.WriteByte(0x3B);
					p_writer.WriteIntWithHeader(p_value.FlyingCheckpoint);
					p_writer.WriteByte(0x42);
					p_writer.WriteStringWithHeader(p_value.FlyingModel);
					p_writer.WriteFloatWithHeader(p_value.FlyingParam1);
					p_writer.WriteFloatWithHeader(p_value.FlyingParam2);
					p_writer.WriteFloatWithHeader(p_value.FlyingParam3);
					p_writer.WriteToken(Token.RightCurly);
					break;
			}
		}
	}

	public class HZB_PathHazard
	{
		private const byte
			PROPERTY_POSITION1 = 0x37,
			PROPERTY_POSITION2 = 0x38,
			PROPERTY_POSITION3 = 0x39,
			PROPERTY_RADIUS = 0x3A,
			PROPERTY_CHECKPOINT = 0x3B;

		public LRVector3 Position1;
		public bool HasPosition2;
		public LRVector3 Position2;
		public bool HasPosition3;
		public LRVector3 Position3;
		public float Radius;
		public int Checkpoint;

		public static HZB_PathHazard Read(LRBinaryReader p_reader)
		{
			HZB_PathHazard val = new HZB_PathHazard();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_POSITION1:
						val.Position1 = LRVector3.Read(p_reader);
						break;
					case PROPERTY_POSITION2:
						val.HasPosition2 = true;
						val.Position2 = LRVector3.Read(p_reader);
						break;
					case PROPERTY_POSITION3:
						val.HasPosition3 = true;
						val.Position3 = LRVector3.Read(p_reader);
						break;
					case PROPERTY_RADIUS:
						val.Radius = p_reader.ReadFloatWithHeader();
						break;
					case PROPERTY_CHECKPOINT:
						val.Checkpoint = p_reader.ReadIntWithHeader();
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

		public static void Write(LRBinaryWriter p_writer, HZB_PathHazard p_value)
		{
			p_writer.WriteByte(PROPERTY_POSITION1);
			LRVector3.Write(p_writer, p_value.Position1);

			if (p_value.HasPosition2)
			{
				p_writer.WriteByte(PROPERTY_POSITION2);
				LRVector3.Write(p_writer, p_value.Position2);
			}

			if (p_value.HasPosition3)
			{
				p_writer.WriteByte(PROPERTY_POSITION3);
				LRVector3.Write(p_writer, p_value.Position3);
			}

			p_writer.WriteByte(PROPERTY_RADIUS);
			p_writer.WriteFloatWithHeader(p_value.Radius);

			p_writer.WriteByte(PROPERTY_CHECKPOINT);
			p_writer.WriteIntWithHeader(p_value.Checkpoint);
		}
	}

	public class HZB_WaterZone
	{
		private const byte
			PROPERTY_VERTICES1 = 0x37,
			PROPERTY_VERTICES2 = 0x38,
			PROPERTY_PATH = 0x33;

		public HZB_Vertex[] Vertices1;
		public HZB_Vertex[] Vertices2;
		public HZB_PathHazard Path;

		public static HZB_WaterZone Read(LRBinaryReader p_reader)
		{
			HZB_WaterZone val = new HZB_WaterZone();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_VERTICES1:
						val.Vertices1 = ReadVertexArray(p_reader);
						break;
					case PROPERTY_VERTICES2:
						val.Vertices2 = ReadVertexArray(p_reader);
						break;
					case PROPERTY_PATH:
						val.Path = p_reader.ReadStruct<HZB_PathHazard>(HZB_PathHazard.Read);
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

		public static void Write(LRBinaryWriter p_writer, HZB_WaterZone p_value)
		{
			if (p_value.Vertices1 != null)
			{
				p_writer.WriteByte(PROPERTY_VERTICES1);
				WriteVertexArray(p_writer, p_value.Vertices1);
			}

			if (p_value.Vertices2 != null)
			{
				p_writer.WriteByte(PROPERTY_VERTICES2);
				WriteVertexArray(p_writer, p_value.Vertices2);
			}

			if (p_value.Path != null)
			{
				p_writer.WriteByte(PROPERTY_PATH);
				p_writer.WriteStruct<HZB_PathHazard>(HZB_PathHazard.Write, p_value.Path);
			}
		}

		private static HZB_Vertex[] ReadVertexArray(LRBinaryReader p_reader)
		{
			p_reader.Expect(Token.LeftBracket);
			int count = p_reader.ReadIntWithHeader();
			p_reader.Expect(Token.RightBracket);
			p_reader.Expect(Token.LeftCurly);
			HZB_Vertex[] vertices = new HZB_Vertex[count];
			for (int i = 0; i < count; i++)
			{
				vertices[i] = new HZB_Vertex();
				vertices[i].X = p_reader.ReadFloatWithHeader();
				vertices[i].Y = p_reader.ReadFloatWithHeader();
				vertices[i].Z = p_reader.ReadFloatWithHeader();
				vertices[i].Index = p_reader.ReadIntWithHeader();
			}
			p_reader.Expect(Token.RightCurly);
			return vertices;
		}

		private static void WriteVertexArray(LRBinaryWriter p_writer, HZB_Vertex[] p_vertices)
		{
			p_writer.WriteToken(Token.LeftBracket);
			p_writer.WriteIntWithHeader(p_vertices.Length);
			p_writer.WriteToken(Token.RightBracket);
			p_writer.WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_vertices.Length; i++)
			{
				p_writer.WriteFloatWithHeader(p_vertices[i].X);
				p_writer.WriteFloatWithHeader(p_vertices[i].Y);
				p_writer.WriteFloatWithHeader(p_vertices[i].Z);
				p_writer.WriteIntWithHeader(p_vertices[i].Index);
			}
			p_writer.WriteToken(Token.RightCurly);
		}
	}

	public class HZB_Vertex
	{
		public float X;
		public float Y;
		public float Z;
		public int Index;
	}

	public class HZB_SpinningHazard
	{
		private const byte
			PROPERTY_CHECKPOINT = 0x3B,
			PROPERTY_POSITION = 0x37,
			PROPERTY_MODEL = 0x42,
			PROPERTY_DURATION = 0x46,
			PROPERTY_ROTATION = 0x47;

		public int Checkpoint;
		public bool HasPosition;
		public LRVector3 Position;
		public string Model;
		public int Duration;
		public LRVector3 Rotation;

		public static HZB_SpinningHazard Read(LRBinaryReader p_reader)
		{
			HZB_SpinningHazard val = new HZB_SpinningHazard();
			while (!p_reader.Next(Token.RightCurly))
			{
				byte propId = p_reader.ReadByte();
				switch (propId)
				{
					case PROPERTY_CHECKPOINT:
						val.Checkpoint = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_POSITION:
						val.HasPosition = true;
						val.Position = LRVector3.Read(p_reader);
						break;
					case PROPERTY_MODEL:
						val.Model = p_reader.ReadStringWithHeader();
						break;
					case PROPERTY_DURATION:
						val.Duration = p_reader.ReadIntWithHeader();
						break;
					case PROPERTY_ROTATION:
						val.Rotation = LRVector3.Read(p_reader);
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

		public static void Write(LRBinaryWriter p_writer, HZB_SpinningHazard p_value)
		{
			p_writer.WriteByte(PROPERTY_CHECKPOINT);
			p_writer.WriteIntWithHeader(p_value.Checkpoint);

			if (p_value.HasPosition)
			{
				p_writer.WriteByte(PROPERTY_POSITION);
				LRVector3.Write(p_writer, p_value.Position);
			}

			if (p_value.Model != null)
			{
				p_writer.WriteByte(PROPERTY_MODEL);
				p_writer.WriteStringWithHeader(p_value.Model);
			}

			p_writer.WriteByte(PROPERTY_DURATION);
			p_writer.WriteIntWithHeader(p_value.Duration);

			p_writer.WriteByte(PROPERTY_ROTATION);
			LRVector3.Write(p_writer, p_value.Rotation);
		}
	}
}
