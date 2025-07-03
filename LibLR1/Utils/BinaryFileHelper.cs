using LibLR1.IO;
using System.Collections.Generic;
using System.IO;

namespace LibLR1.Utils
{
	public static class BinaryFileHelper
	{
		public static LRBinaryReader Decompress(string p_filepath)
		{
			using (LRBinaryReader reader = new LRBinaryReader(File.OpenRead(p_filepath)))
			{
				return Decompress(reader);
			}
		}

		private static LRBinaryReader Decompress(LRBinaryReader p_reader)
		{
			Dictionary<Token, Token[]> structs = new Dictionary<Token, Token[]>();
			MemoryStream msOut = new MemoryStream();
			using (LRBinaryWriter writer = new LRBinaryWriter(msOut, false))
			{
				while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
				{
					Token block_id = p_reader.ReadToken();
					RecursiveDecompress(block_id, p_reader, writer, structs);
				}
			}
			msOut.Position = 0;
			return new LRBinaryReader(msOut);
		}

		private static void RecursiveDecompress(Token p_blockId, LRBinaryReader p_reader, LRBinaryWriter p_writer, Dictionary<Token, Token[]> p_structs)
		{
			switch (p_blockId)
			{
				case Token.String:
				{
					p_writer.WriteToken(p_blockId);
					p_writer.WriteString(p_reader.ReadString());
					break;
				}
				case Token.Int32:
				case Token.Float:
				{  // 32-bit little-endian IEEE float or int32
					p_writer.WriteToken(p_blockId);
					p_writer.WriteBytes(p_reader.ReadBytes(4));
					break;
				}
				case Token.LeftCurly:
				case Token.RightCurly:
				case Token.LeftBracket:
				case Token.RightBracket:
				{  // just copy the token, nothing else
					p_writer.WriteToken(p_blockId);
					break;
				}
				case Token.Byte:
				case Token.SByte:
				{  // copy a single byte from the stream
					p_writer.WriteToken(p_blockId);
					p_writer.WriteBytes(p_reader.ReadBytes(1));
					break;
				}
				case Token.Short:
				case Token.UShort:
				{  // copy two bytes from the stream
					p_writer.WriteToken(p_blockId);
					p_writer.WriteBytes(p_reader.ReadBytes(2));
					break;
				}
				case Token.Array:
				{  // decompression pass.
					short arraylen = p_reader.ReadShort();
					Token arraytype = p_reader.ReadToken();
					for (int i = 0; i < arraylen; i++)
					{
						RecursiveDecompress(arraytype, p_reader, p_writer, p_structs);
					}
					break;
				}
				case Token.Struct:
				{  // decompression pass
					Token structid = p_reader.ReadToken();
					byte structlen = p_reader.ReadByte();
					Token[] structdef = new Token[structlen];
					for (int i = 0; i < structlen; i++)
					{
						structdef[i] = p_reader.ReadToken();
					}
					p_structs.Add(structid, structdef);
					break;
				}
				default:
				{
					if (p_structs.ContainsKey(p_blockId))
					{  // it's a struct
						for (int i = 0; i < p_structs[p_blockId].Length; i++)
						{
							RecursiveDecompress(p_structs[p_blockId][i], p_reader, p_writer, p_structs);
						}
					}
					else
					{  // it's a file-specific block token
						p_writer.WriteToken(p_blockId);
					}
					break;
				}
			}
		}
	}
}