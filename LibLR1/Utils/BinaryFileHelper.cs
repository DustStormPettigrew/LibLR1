using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LibLR1.Exceptions;

namespace LibLR1.Utils
{
	public static class BinaryFileHelper
	{
		public const byte
			TYPE_STRING        = 0x02,
			TYPE_FLOAT         = 0x03,
			TYPE_INT32         = 0x04,
			TYPE_LEFT_CURLY    = 0x05,
			TYPE_RIGHT_CURLY   = 0x06,
			TYPE_LEFT_BRACKET  = 0x07,
			TYPE_RIGHT_BRACKET = 0x08,
			TYPE_8BIT_FRACT    = 0x0B,
			TYPE_SBYTE         = 0x0B,
			TYPE_BYTE          = 0x0C,
			TYPE_16BIT_FRACT   = 0x0D,
			TYPE_SHORT         = 0x0D,
			TYPE_USHORT        = 0x0E,
			TYPE_ARRAY         = 0x14,  // compression pass only
			TYPE_STRUCT        = 0x16;  // compression pass only
		
		private const int
			SIZE_32 = 4,
			SIZE_16 = 2;
		
		#region Compression
		
		public static MemoryStream Decompress(string p_filepath)
		{
			using (FileStream fs = new FileStream(p_filepath, FileMode.Open, FileAccess.Read))
			{
				return Decompress(fs);
			}
		}
		
		public static MemoryStream Decompress(Stream p_streamIn)
		{
			Dictionary<byte, byte[]> structs = new Dictionary<byte, byte[]>();
			MemoryStream msOut = new MemoryStream();
			while (p_streamIn.Position < p_streamIn.Length)
			{
				byte block_id = ReadByte(p_streamIn);
				RecursiveDecompress(block_id, p_streamIn, msOut, structs);
			}
			msOut.Position = 0;
			return msOut;
		}
		
		private static void RecursiveDecompress(byte p_blockId, Stream p_streamIn, Stream p_streamOut, Dictionary<byte, byte[]> p_structs)
		{
			switch (p_blockId)
			{
				case TYPE_STRING:
				{  // block_id followed by a null-terminated ascii string
					p_streamOut.WriteByte(p_blockId);
					while (true)
					{
						byte buffer = ReadByte(p_streamIn);
						p_streamOut.WriteByte(buffer);
						if (buffer == 0)
						{
							break;
						}
					}
					break;
				}
				case TYPE_FLOAT:
				case TYPE_INT32:
				{  // 32-bit little-endian IEEE float or int32
					p_streamOut.WriteByte(p_blockId);
					byte[] buffer = new byte[SIZE_32];
					p_streamIn.Read(buffer, 0, buffer.Length);
					p_streamOut.Write(buffer, 0, buffer.Length);
					break;
				}
				case TYPE_LEFT_CURLY:
				case TYPE_RIGHT_CURLY:
				case TYPE_LEFT_BRACKET:
				case TYPE_RIGHT_BRACKET:
				{  // just copy the token, nothing else
					p_streamOut.WriteByte(p_blockId);
					break;
				}
				case TYPE_SBYTE:
				case TYPE_BYTE:
				{  // copy a single byte from the stream
					p_streamOut.WriteByte(p_blockId);
					byte buffer = ReadByte(p_streamIn);
					p_streamOut.WriteByte(buffer);
					break;
				}
				case TYPE_SHORT:
				case TYPE_USHORT:
				{  // copy 16 bits (2 bytes) from the stream
					p_streamOut.WriteByte(p_blockId);
					byte[] buffer = new byte[SIZE_16];
					p_streamIn.Read(buffer, 0, buffer.Length);
					p_streamOut.Write(buffer, 0, buffer.Length);
					break;
				}
				case TYPE_ARRAY:
				{  // decompression pass.
					short arraylen = ReadShort(p_streamIn);
					byte arraytype = ReadByte(p_streamIn);
					for (int i = 0; i < arraylen; i++)
					{
						RecursiveDecompress(arraytype, p_streamIn, p_streamOut, p_structs);
					}
					break;
				}
				case TYPE_STRUCT:
				{  // decompression pass
					byte structid = ReadByte(p_streamIn);
					byte structlen = ReadByte(p_streamIn);
					byte[] structdef = new byte[structlen];
					p_streamIn.Read(structdef, 0, structlen);
					p_structs.Add(structid, structdef);
					break;
				}
				default:
				{
					if (p_structs.ContainsKey(p_blockId))
					{  // it's a struct
						for (int i = 0; i < p_structs[p_blockId].Length; i++)
						{
							RecursiveDecompress(p_structs[p_blockId][i], p_streamIn, p_streamOut, p_structs);
						}
					}
					else
					{  // it's a file-specific block token
						p_streamOut.WriteByte(p_blockId);
					}
					break;
				}
			}
		}
		
		#endregion
		
		#region Basic Read
		
		public static byte ReadByte(Stream p_stream)
		{
			return (byte)p_stream.ReadByte();
		}
		
		public static sbyte ReadSByte(Stream p_stream)
		{
			return (sbyte)p_stream.ReadByte();
		}
		
		public static byte ReadByteWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_BYTE);
			return ReadByte(p_stream);
		}
		
		public static sbyte ReadSByteWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_SBYTE);
			return ReadSByte(p_stream);
		}
		
		public static Fract8Bit ReadFract8BitWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_8BIT_FRACT);
			return Fract8Bit.FromStream(p_stream);
		}
		
		public static ushort ReadUShort(Stream p_stream)
		{
			byte[] b_val = new byte[sizeof(ushort)];
			p_stream.Read(b_val, 0, b_val.Length);
			return BitConverter.ToUInt16(b_val, 0);
		}
		
		public static short ReadShort(Stream p_stream)
		{
			byte[] b_val = new byte[sizeof(short)];
			p_stream.Read(b_val, 0, b_val.Length);
			return BitConverter.ToInt16(b_val, 0);
		}
		
		public static ushort ReadUShortWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_USHORT);
			return ReadUShort(p_stream);
		}
		
		public static short ReadShortWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_SHORT);
			return ReadShort(p_stream);
		}
		
		public static Fract16Bit ReadFract16BitWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_16BIT_FRACT);
			return Fract16Bit.FromStream(p_stream);
		}
		
		public static uint ReadUInt(Stream p_stream)
		{
			byte[] b_val = new byte[sizeof(uint)];
			p_stream.Read(b_val, 0, b_val.Length);
			return BitConverter.ToUInt32(b_val, 0);
		}
		
		public static int ReadInt(Stream p_stream)
		{
			byte[] b_val = new byte[sizeof(int)];
			p_stream.Read(b_val, 0, b_val.Length);
			return BitConverter.ToInt32(b_val, 0);
		}
		
		public static int ReadIntWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_INT32);
			return ReadInt(p_stream);
		}
		
		public static int ReadIntegralWithHeader(Stream p_stream)
		{
			byte type = Expect(p_stream, new byte[] { TYPE_SBYTE, TYPE_BYTE, TYPE_INT32, TYPE_USHORT, TYPE_SHORT });
			switch (type)
			{
				case TYPE_SBYTE:  return ReadSByte(p_stream);
				case TYPE_BYTE:   return ReadByte(p_stream);
				case TYPE_INT32:  return ReadInt(p_stream);
				case TYPE_USHORT: return ReadUShort(p_stream);
				case TYPE_SHORT:  return ReadShort(p_stream);
			}
			throw new UnexpectedTypeException((Token)type, p_stream.Position - 1);
		}
		
		public static float ReadFloat(Stream p_stream)
		{
			byte[] b_val = new byte[sizeof(float)];
			p_stream.Read(b_val, 0, b_val.Length);
			return BitConverter.ToSingle(b_val, 0);
		}
		
		public static float ReadFloatWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_FLOAT);
			return ReadFloat(p_stream);
		}
		
		public static string ReadString(Stream p_stream)
		{
			string s_val = "";
			byte b;
			while ((b = ReadByte(p_stream)) != 0x00)
			{
				s_val += (char)b;
			}
			return s_val;
		}
		
		public static string ReadStringWithHeader(Stream p_stream)
		{
			Expect(p_stream, TYPE_STRING);
			return ReadString(p_stream);
		}
		
		#endregion
		
		#region Complex Read
		
		public delegate T ReadObject<T>(Stream p_stream);
		
		public static T[] ReadArrayBlock<T>(Stream p_stream, ReadObject<T> p_readFunc)
		{
			// [array_len]
			// {
			//     output[0],
			//     output[1],
			//     :
			//     output[array_len - 1]
			// }
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int array_len = ReadIntWithHeader(p_stream);
			T[] output = new T[array_len];
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				output[i] = p_readFunc(p_stream);
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static List<T> ReadListBlock<T>(Stream p_stream, ReadObject<T> p_readFunc)
		{
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int array_len = ReadIntWithHeader(p_stream);
			List<T> output = new List<T>();
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				output.Add(p_readFunc(p_stream));
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static T[] ReadStructArrayBlock<T>(Stream p_stream, ReadObject<T> p_readFunc, byte p_typeByte)
		{
			// [array_len]
			// {
			//     0xtypeByte
			//     {
			//         output[0],
			//     }
			//     :
			//     0xtypeByte
			//     {
			//         output[array_len - 1],
			//     }
			// }
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int array_len = ReadIntWithHeader(p_stream);
			T[] output = new T[array_len];
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_stream, p_typeByte);
				output[i] = ReadStruct<T>(p_stream, p_readFunc);
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static List<T> ReadStructListBlock<T>(Stream p_stream, ReadObject<T> p_readFunc, byte p_typeByte)
		{
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int array_len = ReadIntWithHeader(p_stream);
			List<T> output = new List<T>();
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_stream, p_typeByte);
				output.Add(ReadStruct<T>(p_stream, p_readFunc));
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static Dictionary<string, T> ReadDictionaryBlock<T>(Stream p_stream, ReadObject<T> p_readFunc, byte p_typeByte)
		{
			// [array_len]
			// {
			//     0xtypeByte
			//     "item_key"
			//     {
			//         output[0],
			//     }
			//     :
			//     0xtypeByte
			//     "item_key"
			//     {
			//         output[array_len - 1],
			//     }
			// }
			Dictionary<string, T> output = new Dictionary<string, T>();
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int dict_len = ReadIntWithHeader(p_stream);
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_stream, p_typeByte);
				string item_key = i.ToString();
				if (Next(p_stream, TYPE_STRING))
				{
					item_key = ReadStringWithHeader(p_stream);
				}
				T item_value = ReadStruct<T>(p_stream, p_readFunc);
				output.Add(item_key, item_value);
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static KeyValuePair<string, T>[] ReadCollidableDictionaryBlock<T>(Stream p_stream, ReadObject<T> p_readFunc, byte p_typeByte)
		{
			// not a fucking clue what this is about.
			Expect(p_stream, TYPE_LEFT_BRACKET);
			int dict_len = ReadIntWithHeader(p_stream);
			KeyValuePair<string, T>[] output = new KeyValuePair<string, T>[dict_len];
			Expect(p_stream, TYPE_RIGHT_BRACKET);
			Expect(p_stream, TYPE_LEFT_CURLY);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_stream, p_typeByte);
				string item_key = i.ToString();
				if (Next(p_stream, TYPE_STRING))
				{
					item_key = ReadStringWithHeader(p_stream);
				}
				T item_value = ReadStruct<T>(p_stream, p_readFunc);
				output[i] = new KeyValuePair<string, T>(item_key, item_value);
			}
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static T ReadStruct<T>(Stream p_stream, ReadObject<T> p_readFunc)
		{
			Expect(p_stream, TYPE_LEFT_CURLY);
			T output = p_readFunc(p_stream);
			Expect(p_stream, TYPE_RIGHT_CURLY);
			return output;
		}
		
		public static byte[] ReadByteArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<byte>(p_stream, new ReadObject<byte>(ReadByteWithHeader));
		}
		
		public static float[] ReadFloatArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<float>(p_stream, new ReadObject<float>(ReadFloatWithHeader));
		}
		
		public static int[] ReadIntArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<int>(p_stream, new ReadObject<int>(ReadIntWithHeader));
		}
		
		public static LRQuaternion[] ReadQuaternionArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<LRQuaternion>(p_stream, new ReadObject<LRQuaternion>(LRQuaternion.FromStream));
		}
		
		public static string[] ReadStringArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<string>(p_stream, new ReadObject<string>(ReadStringWithHeader));
		}
		
		public static LRVector2[] ReadVector2fArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<LRVector2>(p_stream, new ReadObject<LRVector2>(LRVector2.FromStream));
		}
		
		public static LRVector3[] ReadVector3fArrayBlock(Stream p_stream)
		{
			return ReadArrayBlock<LRVector3>(p_stream, new ReadObject<LRVector3>(LRVector3.FromStream));
		}
		
		#endregion
		
		#region Basic Write

		public static void WriteByte(Stream p_stream, byte p_value)
		{
			p_stream.WriteByte(p_value);
		}

		public static void WriteSByte(Stream p_stream, sbyte p_value)
		{
			p_stream.WriteByte((byte)p_value);
		}

		public static void WriteByteWithHeader(Stream p_stream, byte p_value)
		{
			p_stream.WriteByte(TYPE_BYTE);
			WriteByte(p_stream, p_value);
		}

		public static void WriteFract8BitWithHeader(Stream p_stream, Fract8Bit p_value)
		{
			p_stream.WriteByte(TYPE_8BIT_FRACT);
			WriteSByte(p_stream, p_value.Value);
		}

		public static void WriteUShort(Stream p_stream, ushort p_value)
		{
			byte[] b_val = BitConverter.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
		}

		public static void WriteShort(Stream p_stream, short p_value)
		{
			byte[] b_val = BitConverter.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
		}

		public static void WriteFract16BitWithHeader(Stream p_stream, Fract16Bit p_value)
		{
			p_stream.WriteByte(TYPE_16BIT_FRACT);
			WriteShort(p_stream, p_value.Value);
		}

		public static void WriteUShortWithHeader(Stream p_stream, ushort p_value)
		{
			p_stream.WriteByte(TYPE_USHORT);
			WriteUShort(p_stream, p_value);
		}

		public static void WriteUInt(Stream p_stream, uint p_value)
		{
			byte[] b_val = BitConverter.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
		}

		public static void WriteInt(Stream p_stream, int p_value)
		{
			byte[] b_val = BitConverter.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
		}

		public static void WriteIntWithHeader(Stream p_stream, int p_value)
		{
			p_stream.WriteByte(TYPE_INT32);
			WriteInt(p_stream, p_value);
		}

		public static void WriteIntegralWithHeader(Stream p_stream, int p_value)
		{
			if (p_value >= 0)
			{
				if (p_value < 256)
				{
					WriteByteWithHeader(p_stream, (byte)p_value);
				}
				else if (p_value < 65536)
				{
					WriteUShortWithHeader(p_stream, (ushort)p_value);
				}
				else
				{
					WriteIntWithHeader(p_stream, p_value);
				}
			}
			else
			{
				WriteIntWithHeader(p_stream, p_value);
			}
		}

		public static void WriteFloat(Stream p_stream, float p_value)
		{
			byte[] b_val = BitConverter.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
		}

		public static void WriteFloatWithHeader(Stream p_stream, float p_value)
		{
			p_stream.WriteByte(TYPE_FLOAT);
			WriteFloat(p_stream, p_value);
		}

		public static void WriteString(Stream p_stream, string p_value)
		{
			byte[] b_val = Encoding.ASCII.GetBytes(p_value);
			p_stream.Write(b_val, 0, b_val.Length);
			p_stream.WriteByte(0x00);
		}

		public static void WriteStringWithHeader(Stream p_stream, string p_value)
		{
			p_stream.WriteByte(TYPE_STRING);
			WriteString(p_stream, p_value);
		}
		
		#endregion
		
		#region Complex Write
		
		public delegate void WriteObject<T>(Stream p_stream, T p_value);
		
		public static void WriteArrayBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, T[] p_values)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Length);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			for (int i = 0; i < p_values.Length; i++)
			{
				p_writeFunc(p_stream, p_values[i]);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}
		
		public static void WriteListBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, List<T> p_values)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Count);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writeFunc(p_stream, p_values[i]);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}
		
		public static void WriteStructArrayBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, T[] p_values, byte p_typeByte)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Length);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			for (int i = 0; i < p_values.Length; i++)
			{
				p_stream.WriteByte(p_typeByte);
				WriteStruct<T>(p_stream, p_writeFunc, p_values[i]);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}
		
		public static void WriteStructListBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, List<T> p_values, byte p_typeByte)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Count);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			for (int i = 0; i < p_values.Count; i++)
			{
				p_stream.WriteByte(p_typeByte);
				WriteStruct<T>(p_stream, p_writeFunc, p_values[i]);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}

		public static void WriteDictionaryBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, Dictionary<string, T> p_values, byte p_typeByte)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Count);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				p_stream.WriteByte(p_typeByte);
				WriteStringWithHeader(p_stream, kvp.Key);
				WriteStruct<T>(p_stream, p_writeFunc, kvp.Value);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}

		public static void WriteCollidableDictionaryBlock<T>(Stream p_stream, WriteObject<T> p_writeFunc, KeyValuePair<string, T>[] p_values, byte p_typeByte)
		{
			p_stream.WriteByte(TYPE_LEFT_BRACKET);
			WriteIntWithHeader(p_stream, p_values.Length);
			p_stream.WriteByte(TYPE_RIGHT_BRACKET);
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				p_stream.WriteByte(p_typeByte);
				WriteStringWithHeader(p_stream, kvp.Key);
				WriteStruct<T>(p_stream, p_writeFunc, kvp.Value);
			}
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}

		public static void WriteStruct<T>(Stream p_stream, WriteObject<T> p_writeFunc, T p_value)
		{
			p_stream.WriteByte(TYPE_LEFT_CURLY);
			p_writeFunc(p_stream, p_value);
			p_stream.WriteByte(TYPE_RIGHT_CURLY);
		}

		public static void WriteByteArrayBlock(Stream p_stream, byte[] p_values)
		{
			WriteArrayBlock<byte>(p_stream, new WriteObject<byte>(WriteByteWithHeader), p_values);
		}

		public static void WriteFloatArrayBlock(Stream p_stream, float[] p_values)
		{
			WriteArrayBlock<float>(p_stream, new WriteObject<float>(WriteFloatWithHeader), p_values);
		}

		public static void WriteIntArrayBlock(Stream p_stream, int[] p_values)
		{
			WriteArrayBlock<int>(p_stream, new WriteObject<int>(WriteIntWithHeader), p_values);
		}

		public static void WriteQuaternionArrayBlock(Stream p_stream, LRQuaternion[] p_values)
		{
			WriteArrayBlock<LRQuaternion>(p_stream, new WriteObject<LRQuaternion>(LRQuaternion.ToStream), p_values);
		}

		public static void WriteStringArrayBlock(Stream p_stream, string[] p_values)
		{
			WriteArrayBlock<string>(p_stream, new WriteObject<string>(WriteStringWithHeader), p_values);
		}

		public static void WriteVector2fArrayBlock(Stream p_stream, LRVector2[] p_values)
		{
			WriteArrayBlock<LRVector2>(p_stream, new WriteObject<LRVector2>(LRVector2.ToStream), p_values);
		}

		public static void WriteVector3fArrayBlock(Stream p_stream, LRVector3[] p_values)
		{
			WriteArrayBlock<LRVector3>(p_stream, new WriteObject<LRVector3>(LRVector3.ToStream), p_values);
		}
		
		#endregion
		
		#region Expect/Next

		public static void Expect(Stream p_stream, byte p_expected)
		{
			byte actual = ReadByte(p_stream);
			if (actual != p_expected)
			{
				throw new Exception(string.Format("Invalid data. Expected 0x{0:X2}, got 0x{1:X2}.", p_expected, actual));
			}
		}
		
		public static byte Expect(Stream p_stream, byte[] p_expected)
		{
			byte actual = ReadByte(p_stream);
			if (p_expected.Contains(actual) == false)
			{
				string s_expected = "{ ";
				for (int i = 0; i < p_expected.Length; i++)
				{
					s_expected += (i == 0 ? "" : ", ") + "0x" + p_expected[i].ToString("X2");
				}
				s_expected += " }";
				throw new Exception(string.Format("Invalid data. Expected {0}, got 0x{1:X2}.", s_expected, actual));
			}
			return actual;
		}
		
		public static bool Next(Stream p_stream, byte p_expected)
		{
			byte actual = ReadByte(p_stream);
			p_stream.Position--;
			return actual == p_expected;
		}
		
		public static bool Next(Stream p_stream, byte[] p_expected)
		{
			byte actual = ReadByte(p_stream);
			p_stream.Position--;
			return p_expected.Contains(actual);
		}
		
		#endregion
	}
}