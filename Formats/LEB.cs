using LibLR1.Exceptions;
using LibLR1.IO;
using LibLR1.Utils;
using System.Collections.Generic;
using System.IO;

namespace LibLR1
{
	/// <summary>
	/// LEGO Brick List format. Defines brick piece sets, brick geometry data, color palettes, and crest manager file lists.
	/// </summary>
	public class LEB
	{
		private const byte
			ID_PIECE_GEOMETRY = 0x28,
			ID_PIECE_NORMALS = 0x29,
			ID_PIECE_COLORS = 0x2A,
			ID_PIECE_UVS = 0x2B,
			ID_COLORS = 0x2D,
			ID_PIECE_COUNT = 0x2E,
			ID_CREST_MANAGER = 0x2F,
			ID_PIECES = 0x30,
			ID_CHASSIS_NAME = 0x31;

		private object[] m_pieceGeometry;
		private int m_pieceGeometryDeclaredCount;
		private object[] m_pieceNormals;
		private int m_pieceNormalsDeclaredCount;
		private object[] m_pieceColors;
		private int m_pieceColorsDeclaredCount;
		private object[] m_pieceUVs;
		private int m_pieceUVsDeclaredCount;
		private string[] m_colors;
		private int m_pieceCount;
		private int m_pieceDeclaredCount;
		private string[] m_crestManagerFiles;
		private string[] m_pieces;
		private string m_chassisName;
		private Dictionary<byte, KeyValuePair<int, object[]>> m_extraBlocks;

		public object[] PieceGeometry
		{
			get { return m_pieceGeometry; }
			set { m_pieceGeometry = value; }
		}

		public object[] PieceNormals
		{
			get { return m_pieceNormals; }
			set { m_pieceNormals = value; }
		}

		public object[] PieceColors
		{
			get { return m_pieceColors; }
			set { m_pieceColors = value; }
		}

		public object[] PieceUVs
		{
			get { return m_pieceUVs; }
			set { m_pieceUVs = value; }
		}

		public string[] Colors
		{
			get { return m_colors; }
			set { m_colors = value; }
		}

		public int PieceCount
		{
			get { return m_pieceCount; }
			set { m_pieceCount = value; }
		}

		public string[] CrestManagerFiles
		{
			get { return m_crestManagerFiles; }
			set { m_crestManagerFiles = value; }
		}

		public string[] Pieces
		{
			get { return m_pieces; }
			set { m_pieces = value; }
		}

		public string ChassisName
		{
			get { return m_chassisName; }
			set { m_chassisName = value; }
		}

		public Dictionary<byte, KeyValuePair<int, object[]>> ExtraBlocks
		{
			get { return m_extraBlocks; }
			set { m_extraBlocks = value; }
		}

		public LEB(string p_filepath)
			: this(BinaryFileHelper.Decompress(p_filepath))
		{
		}

		public LEB(LRBinaryReader p_reader)
		{
			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				byte blockId = p_reader.ReadByte();
				switch (blockId)
				{
					case ID_PIECE_GEOMETRY:
					{
						m_pieceGeometry = ReadMixedArrayBlock(p_reader, out m_pieceGeometryDeclaredCount);
						break;
					}
					case ID_PIECE_NORMALS:
					{
						m_pieceNormals = ReadMixedArrayBlock(p_reader, out m_pieceNormalsDeclaredCount);
						break;
					}
					case ID_PIECE_COLORS:
					{
						m_pieceColors = ReadMixedArrayBlock(p_reader, out m_pieceColorsDeclaredCount);
						break;
					}
					case ID_PIECE_UVS:
					{
						m_pieceUVs = ReadMixedArrayBlock(p_reader, out m_pieceUVsDeclaredCount);
						break;
					}
					case ID_COLORS:
					{
						m_colors = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_PIECE_COUNT:
					{
						m_pieceCount = p_reader.ReadIntWithHeader();
						break;
					}
					case ID_CREST_MANAGER:
					{
						m_crestManagerFiles = p_reader.ReadStringArrayBlock();
						break;
					}
					case ID_PIECES:
					{
						p_reader.Expect(Token.LeftBracket);
						m_pieceDeclaredCount = p_reader.ReadIntWithHeader();
						p_reader.Expect(Token.RightBracket);
						p_reader.Expect(Token.LeftCurly);
						List<string> pieces = new List<string>();
						while (!p_reader.Next(Token.RightCurly))
						{
							pieces.Add(p_reader.ReadStringWithHeader());
						}
						p_reader.Expect(Token.RightCurly);
						m_pieces = pieces.ToArray();
						break;
					}
					case ID_CHASSIS_NAME:
					{
						m_chassisName = p_reader.ReadStringWithHeader();
						break;
					}
					default:
					{
						if (m_extraBlocks == null)
							m_extraBlocks = new Dictionary<byte, KeyValuePair<int, object[]>>();
						int declaredCount;
						object[] data = ReadMixedArrayBlock(p_reader, out declaredCount);
						m_extraBlocks[blockId] = new KeyValuePair<int, object[]>(declaredCount, data);
						break;
					}
				}
			}
		}

		private static object[] ReadMixedArrayBlock(LRBinaryReader p_reader, out int p_declaredCount)
		{
			p_reader.Expect(Token.LeftBracket);
			p_declaredCount = p_reader.ReadIntWithHeader();
			p_reader.Expect(Token.RightBracket);
			p_reader.Expect(Token.LeftCurly);
			List<object> items = new List<object>();
			while (!p_reader.Next(Token.RightCurly))
			{
				if (p_reader.Next(Token.Int32))
				{
					items.Add(p_reader.ReadIntWithHeader());
				}
				else if (p_reader.Next(Token.String))
				{
					items.Add(p_reader.ReadStringWithHeader());
				}
				else
				{
					Token actual = p_reader.ReadToken();
					throw new UnexpectedTypeException(
						actual,
						p_reader.BaseStream.Position - 1
					);
				}
			}
			p_reader.Expect(Token.RightCurly);
			return items.ToArray();
		}

		private static void WriteMixedArrayBlock(LRBinaryWriter p_writer, object[] p_values, int p_declaredCount)
		{
			p_writer.WriteToken(Token.LeftBracket);
			p_writer.WriteIntWithHeader(p_declaredCount);
			p_writer.WriteToken(Token.RightBracket);
			p_writer.WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_values.Length; i++)
			{
				if (p_values[i] is int n)
				{
					p_writer.WriteIntWithHeader(n);
				}
				else if (p_values[i] is string s)
				{
					p_writer.WriteStringWithHeader(s);
				}
			}
			p_writer.WriteToken(Token.RightCurly);
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
			if (m_pieceGeometry != null)
			{
				p_writer.WriteByte(ID_PIECE_GEOMETRY);
				WriteMixedArrayBlock(p_writer, m_pieceGeometry, m_pieceGeometryDeclaredCount);
			}

			if (m_pieceNormals != null)
			{
				p_writer.WriteByte(ID_PIECE_NORMALS);
				WriteMixedArrayBlock(p_writer, m_pieceNormals, m_pieceNormalsDeclaredCount);
			}

			if (m_pieceColors != null)
			{
				p_writer.WriteByte(ID_PIECE_COLORS);
				WriteMixedArrayBlock(p_writer, m_pieceColors, m_pieceColorsDeclaredCount);
			}

			if (m_pieceUVs != null)
			{
				p_writer.WriteByte(ID_PIECE_UVS);
				WriteMixedArrayBlock(p_writer, m_pieceUVs, m_pieceUVsDeclaredCount);
			}

			if (m_colors != null)
			{
				p_writer.WriteByte(ID_COLORS);
				p_writer.WriteStringArrayBlock(m_colors);
			}

			if (m_pieces != null)
			{
				p_writer.WriteByte(ID_PIECE_COUNT);
				p_writer.WriteIntWithHeader(m_pieceCount);

				p_writer.WriteByte(ID_CHASSIS_NAME);
				p_writer.WriteStringWithHeader(m_chassisName);

				p_writer.WriteByte(ID_PIECES);
				p_writer.WriteToken(Token.LeftBracket);
				p_writer.WriteIntWithHeader(m_pieceDeclaredCount);
				p_writer.WriteToken(Token.RightBracket);
				p_writer.WriteToken(Token.LeftCurly);
				for (int i = 0; i < m_pieces.Length; i++)
				{
					p_writer.WriteStringWithHeader(m_pieces[i]);
				}
				p_writer.WriteToken(Token.RightCurly);
			}

			if (m_crestManagerFiles != null)
			{
				p_writer.WriteByte(ID_CREST_MANAGER);
				p_writer.WriteStringArrayBlock(m_crestManagerFiles);
			}

			if (m_extraBlocks != null)
			{
				foreach (KeyValuePair<byte, KeyValuePair<int, object[]>> kvp in m_extraBlocks)
				{
					p_writer.WriteByte(kvp.Key);
					WriteMixedArrayBlock(p_writer, kvp.Value.Value, kvp.Value.Key);
				}
			}
		}
	}
}
