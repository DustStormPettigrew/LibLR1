using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.IO;

namespace LibLR1
{

	/// <summary>
	/// "AI" path format
	/// </summary>
	public class RRB
	{
		private const byte
			ID_NODES = 0x27,
			ID_UNKNOWN_28 = 0x28,
			ID_UNKNOWN_29 = 0x29,
			ID_UNKNOWN_2A = 0x2A,
			ID_UNKNOWN_2B = 0x2B,
			ID_UNKNOWN_2C = 0x2C,
			ID_UNKNOWN_2D = 0x2D;

		private RRB_Node[] m_nodes;
		private LRQuaternion m_unknown28;
		private LRVector3 m_unknown29;   // probably initial position, need to check this.
		private LRVector3 m_unknown2A;
		private LRQuaternion m_unknown2B;
		private int m_unknown2C;
		private int m_unknown2D;

		public RRB_Node[] Nodes
		{
			get { return m_nodes; }
			set { m_nodes = value; }
		}
		public LRQuaternion Unknown28
		{
			get { return m_unknown28; }
			set { m_unknown28 = value; }
		}
		public LRVector3 Unknown29
		{
			get { return m_unknown29; }
			set { m_unknown29 = value; }
		}
		public LRVector3 Unknown2A
		{
			get { return m_unknown2A; }
			set { m_unknown2A = value; }
		}
		public LRQuaternion Unknown2B
		{
			get { return m_unknown2B; }
			set { m_unknown2B = value; }
		}
		public int Unknown2C
		{
			get { return m_unknown2C; }
			set { m_unknown2C = value; }
		}
		public int Unknown2D
		{
			get { return m_unknown2D; }
			set { m_unknown2D = value; }
		}

		public RRB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public RRB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_NODES:
					{
						m_nodes = p_reader.ReadArrayBlock<RRB_Node>(
							RRB_Node.Read
						);
					}
					break;
					case ID_UNKNOWN_28:
					{
						m_unknown28 = LRQuaternion.Read(p_reader);
						break;
					}
					case ID_UNKNOWN_29:
					{
						m_unknown29 = LRVector3.Read(p_reader);
						break;
					}
					case ID_UNKNOWN_2A:
					{
						m_unknown2A = LRVector3.Read(p_reader);
						break;
					}
					case ID_UNKNOWN_2B:
					{
						m_unknown2B = LRQuaternion.Read(p_reader);
						break;
					}
					case ID_UNKNOWN_2C:
					{
						m_unknown2C = p_reader.ReadIntWithHeader();
						break;
					}
					case ID_UNKNOWN_2D:
					{
						m_unknown2D = p_reader.ReadIntWithHeader();
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
			p_writer.WriteByte(ID_UNKNOWN_28);
			LRQuaternion.Write(p_writer, m_unknown28);
			p_writer.WriteByte(ID_UNKNOWN_29);
			LRVector3.Write(p_writer, m_unknown29);
			p_writer.WriteByte(ID_UNKNOWN_2A);
			LRVector3.Write(p_writer, m_unknown2A);
			p_writer.WriteByte(ID_UNKNOWN_2B);
			LRQuaternion.Write(p_writer, m_unknown2B);
			p_writer.WriteByte(ID_UNKNOWN_2C);
			p_writer.WriteIntWithHeader(m_unknown2C);
			p_writer.WriteByte(ID_UNKNOWN_2D);
			p_writer.WriteIntWithHeader(m_unknown2D);
			p_writer.WriteByte(ID_NODES);
			p_writer.WriteArrayBlock<RRB_Node>(
				RRB_Node.Write,
				m_nodes
			);
		}
	}

	public class RRB_Node
	{
		public Fract16Bit DeltaX;
		public Fract16Bit DeltaY;
		public Fract8Bit DeltaZ;
		public Fract8Bit RotX;
		public Fract8Bit RotY;
		public Fract8Bit RotZ;
		public Fract8Bit RotW;
		public Fract8Bit Fract1;
		public Fract8Bit Fract2;
		public byte UnknownTiming;

		public static RRB_Node Read(LRBinaryReader p_reader)
		{
			RRB_Node val = new RRB_Node();
			val.DeltaX = p_reader.ReadFract16BitWithHeader();
			val.DeltaY = p_reader.ReadFract16BitWithHeader();
			val.DeltaZ = p_reader.ReadFract8BitWithHeader();
			val.RotX = p_reader.ReadFract8BitWithHeader();
			val.RotY = p_reader.ReadFract8BitWithHeader();
			val.RotZ = p_reader.ReadFract8BitWithHeader();
			val.RotW = p_reader.ReadFract8BitWithHeader();
			val.Fract1 = p_reader.ReadFract8BitWithHeader();
			val.Fract2 = p_reader.ReadFract8BitWithHeader();
			val.UnknownTiming = p_reader.ReadByteWithHeader();
			return val;
		}

		public static void Write(LRBinaryWriter p_writer, RRB_Node p_value)
		{
			p_writer.WriteFract16BitWithHeader(p_value.DeltaX);
			p_writer.WriteFract16BitWithHeader(p_value.DeltaY);
			p_writer.WriteFract8BitWithHeader(p_value.DeltaZ);
			p_writer.WriteFract8BitWithHeader(p_value.RotX);
			p_writer.WriteFract8BitWithHeader(p_value.RotY);
			p_writer.WriteFract8BitWithHeader(p_value.RotZ);
			p_writer.WriteFract8BitWithHeader(p_value.RotW);
			p_writer.WriteFract8BitWithHeader(p_value.Fract1);
			p_writer.WriteFract8BitWithHeader(p_value.Fract2);
			p_writer.WriteByteWithHeader(p_value.UnknownTiming);
		}
	}
}