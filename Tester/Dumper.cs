using LibLR1.IO;
using LibLR1.Utils;
using System;
using System.IO;

namespace LibLR1.Tester
{
	public static class Dumper
	{
		public static void DumpFileTokens(string filepath, int maxTokens = 200, long startOffset = 0)
		{
			Console.WriteLine($"\n{'=',' '} {Path.GetFileName(filepath)} {'=',' '}");
			Console.WriteLine($"  Path: {filepath}");
			try
			{
				LRBinaryReader reader = BinaryFileHelper.Decompress(filepath);
				long totalLen = reader.BaseStream.Length;
				Console.WriteLine($"  Decompressed size: {totalLen} bytes");
				if (startOffset > 0)
				{
					reader.BaseStream.Position = startOffset;
					Console.WriteLine($"  Skipped to offset: 0x{startOffset:X}");
				}

				int indent = 0;
				int tokenCount = 0;

				while (reader.BaseStream.Position < reader.BaseStream.Length && tokenCount < maxTokens)
				{
					long pos = reader.BaseStream.Position;
					byte raw = reader.ReadByte();
					Token tok = (Token)raw;
					tokenCount++;

					string prefix = new string(' ', Math.Max(0, indent * 2) + 2);

					switch (tok)
					{
						case Token.RightCurly:
							indent--;
							prefix = new string(' ', Math.Max(0, indent * 2) + 2);
							Console.WriteLine($"{prefix}[0x{raw:X2}] RightCurly  (pos={pos})");
							break;
						case Token.RightBracket:
							indent--;
							prefix = new string(' ', Math.Max(0, indent * 2) + 2);
							Console.WriteLine($"{prefix}[0x{raw:X2}] RightBracket  (pos={pos})");
							break;
						case Token.LeftCurly:
							Console.WriteLine($"{prefix}[0x{raw:X2}] LeftCurly  (pos={pos})");
							indent++;
							break;
						case Token.LeftBracket:
							Console.WriteLine($"{prefix}[0x{raw:X2}] LeftBracket  (pos={pos})");
							indent++;
							break;
						case Token.Int32:
						{
							int val = reader.ReadInt();
							Console.WriteLine($"{prefix}[0x{raw:X2}] Int32 = {val}  (pos={pos})");
							break;
						}
						case Token.Float:
						{
							float val = reader.ReadFloat();
							Console.WriteLine($"{prefix}[0x{raw:X2}] Float = {val}  (pos={pos})");
							break;
						}
						case Token.String:
						{
							string val = reader.ReadString();
							Console.WriteLine($"{prefix}[0x{raw:X2}] String = \"{val}\"  (pos={pos})");
							break;
						}
						case Token.Byte:
						{
							byte val = reader.ReadByte();
							Console.WriteLine($"{prefix}[0x{raw:X2}] Byte = {val} (0x{val:X2})  (pos={pos})");
							break;
						}
						case Token.SByte:
						{
							sbyte val = reader.ReadSByte();
							Console.WriteLine($"{prefix}[0x{raw:X2}] SByte/Fract8 = {val} (float={val / 16f})  (pos={pos})");
							break;
						}
						case Token.Short:
						{
							short val = reader.ReadShort();
							Console.WriteLine($"{prefix}[0x{raw:X2}] Short/Fract16 = {val} (float={val / 256f})  (pos={pos})");
							break;
						}
						case Token.UShort:
						{
							ushort val = reader.ReadUShort();
							Console.WriteLine($"{prefix}[0x{raw:X2}] UShort = {val}  (pos={pos})");
							break;
						}
						default:
						{
							Console.WriteLine($"{prefix}[0x{raw:X2}] BlockID  (pos={pos})");
							break;
						}
					}
				}

				if (reader.BaseStream.Position < reader.BaseStream.Length)
				{
					Console.WriteLine($"  ... truncated ({reader.BaseStream.Length - reader.BaseStream.Position} bytes remaining)");
				}
				else
				{
					Console.WriteLine("  [END OF FILE]");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  ERROR: {ex.Message}");
			}
		}

		public static void DumpAllOfType(string gameFolder, string extension, int maxTokens = 200)
		{
			string[] files = Directory.GetFiles(gameFolder, $"*.{extension}", SearchOption.AllDirectories);
			Console.WriteLine($"\n======== {extension} FILES ({files.Length} found) ========");
			foreach (string file in files)
			{
				DumpFileTokens(file, maxTokens);
			}
		}
	}
}
