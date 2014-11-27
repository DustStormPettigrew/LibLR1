using LibLR1.Exceptions;
using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibLR1.IO
{
	public class LRBinaryReader : IDisposable
	{
		private BinaryReader m_baseReader;
		private bool m_closeOnDispose;

		public Stream BaseStream { get { return m_baseReader.BaseStream; } }

		public LRBinaryReader(Stream p_stream, bool p_closeOnDispose = true)
		{
			m_baseReader = new BinaryReader(p_stream);
			m_closeOnDispose = p_closeOnDispose;
		}

		public void Dispose()
		{
			if (m_closeOnDispose)
			{
				m_baseReader.BaseStream.Close();
			}
		}

		public void Expect(Token p_expected)
		{
			Expect((byte)p_expected);
		}

		public void Expect(byte p_expected)
		{
			byte actual = m_baseReader.ReadByte();
			if (actual != p_expected)
			{
				throw new Exception(string.Format("Invalid data. Expected 0x{0:X2}, got 0x{1:X2}.", p_expected, actual));
			}
		}

		public Token Expect(Token[] p_expected)
		{
			byte[] buffer = new byte[p_expected.Length];
			for (int i = 0; i < buffer.Length; i++)
			{
				buffer[i] = (byte)p_expected[i];
			}
			return (Token)Expect(buffer);
		}

		public byte Expect(byte[] p_expected)
		{
			byte actual = m_baseReader.ReadByte();
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

		public bool Next(Token p_expected)
		{
			return Next((byte)p_expected);
		}

		public bool Next(byte p_expected)
		{
			byte actual = m_baseReader.ReadByte();
			m_baseReader.BaseStream.Position--;
			return actual == p_expected;
		}

		public bool Next(byte[] p_expected)
		{
			byte actual = m_baseReader.ReadByte();
			m_baseReader.BaseStream.Position--;
			return p_expected.Contains(actual);
		}

		public byte ReadByte()
		{
			return m_baseReader.ReadByte();
		}

		public sbyte ReadSByte()
		{
			return m_baseReader.ReadSByte();
		}

		public byte ReadByteWithHeader()
		{
			Expect(Token.BYTE);
			return ReadByte();
		}

		public sbyte ReadSByteWithHeader()
		{
			Expect(Token.SBYTE);
			return ReadSByte();
		}

		public Fract8Bit ReadFract8BitWithHeader()
		{
			Expect(Token.FRACT8);
			return Fract8Bit.Read(this);
		}

		public ushort ReadUShort()
		{
			return m_baseReader.ReadUInt16();
		}

		public short ReadShort()
		{
			return m_baseReader.ReadInt16();
		}

		public ushort ReadUShortWithHeader()
		{
			Expect(Token.USHORT);
			return ReadUShort();
		}

		public short ReadShortWithHeader()
		{
			Expect(Token.SHORT);
			return ReadShort();
		}

		public Fract16Bit ReadFract16BitWithHeader()
		{
			Expect(Token.FRACT16);
			return Fract16Bit.Read(this);
		}

		public uint ReadUInt()
		{
			return m_baseReader.ReadUInt32();
		}

		public int ReadInt()
		{
			return m_baseReader.ReadInt32();
		}

		public int ReadIntWithHeader()
		{
			Expect(Token.INT32);
			return ReadInt();
		}

		public int ReadIntegralWithHeader()
		{
			Token type = Expect(new Token[] { Token.SBYTE, Token.BYTE, Token.INT32, Token.USHORT, Token.SHORT });
			switch (type)
			{
				case Token.SBYTE: return ReadSByte();
				case Token.BYTE: return ReadByte();
				case Token.INT32: return ReadInt();
				case Token.USHORT: return ReadUShort();
				case Token.SHORT: return ReadShort();
			}
			throw new UnexpectedTypeException(type, m_baseReader.BaseStream.Position - 1);
		}

		public float ReadFloat()
		{
			return m_baseReader.ReadSingle();
		}

		public float ReadFloatWithHeader()
		{
			Expect(Token.FLOAT);
			return ReadFloat();
		}

		public string ReadString()
		{
			string s_val = "";
			byte b;
			while ((b = ReadByte()) != 0x00)
			{
				s_val += (char)b;
			}
			return s_val;
		}

		public string ReadStringWithHeader()
		{
			Expect(Token.STRING);
			return ReadString();
		}

		public delegate T ReadObject<T>(LRBinaryReader p_reader);

		public T[] ReadArrayBlock<T>(ReadObject<T> p_readFunc)
		{
			// [array_len]
			// {
			//     output[0],
			//     output[1],
			//     :
			//     output[array_len - 1]
			// }
			Expect(Token.LEFT_BRACKET);
			int array_len = ReadIntWithHeader();
			T[] output = new T[array_len];
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				output[i] = p_readFunc(this);
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public List<T> ReadListBlock<T>(ReadObject<T> p_readFunc)
		{
			Expect(Token.LEFT_BRACKET);
			int array_len = ReadIntWithHeader();
			List<T> output = new List<T>();
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				output.Add(p_readFunc(this));
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public T[] ReadStructArrayBlock<T>(ReadObject<T> p_readFunc, byte p_typeByte)
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
			Expect(Token.LEFT_BRACKET);
			int array_len = ReadIntWithHeader();
			T[] output = new T[array_len];
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_typeByte);
				output[i] = ReadStruct<T>(p_readFunc);
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public List<T> ReadStructListBlock<T>(ReadObject<T> p_readFunc, byte p_typeByte)
		{
			Expect(Token.LEFT_BRACKET);
			int array_len = ReadIntWithHeader();
			List<T> output = new List<T>();
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_typeByte);
				output.Add(ReadStruct<T>(p_readFunc));
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public Dictionary<string, T> ReadDictionaryBlock<T>(ReadObject<T> p_readFunc, byte p_typeByte)
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
			Expect(Token.LEFT_BRACKET);
			int dict_len = ReadIntWithHeader();
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_typeByte);
				string item_key = i.ToString();
				if (Next(Token.STRING))
				{
					item_key = ReadStringWithHeader();
				}
				T item_value = ReadStruct<T>(p_readFunc);
				output.Add(item_key, item_value);
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public KeyValuePair<string, T>[] ReadCollidableDictionaryBlock<T>(ReadObject<T> p_readFunc, byte p_typeByte)
		{
			// not a fucking clue what this is about.
			Expect(Token.LEFT_BRACKET);
			int dict_len = ReadIntWithHeader();
			KeyValuePair<string, T>[] output = new KeyValuePair<string, T>[dict_len];
			Expect(Token.RIGHT_BRACKET);
			Expect(Token.LEFT_CURLY);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_typeByte);
				string item_key = i.ToString();
				if (Next(Token.STRING))
				{
					item_key = ReadStringWithHeader();
				}
				T item_value = ReadStruct<T>(p_readFunc);
				output[i] = new KeyValuePair<string, T>(item_key, item_value);
			}
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public T ReadStruct<T>(ReadObject<T> p_readFunc)
		{
			Expect(Token.LEFT_CURLY);
			T output = p_readFunc(this);
			Expect(Token.RIGHT_CURLY);
			return output;
		}

		public byte[] ReadByteArrayBlock()
		{
			return ReadArrayBlock<byte>(new ReadObject<byte>((br) => br.ReadByteWithHeader()));
		}

		public float[] ReadFloatArrayBlock()
		{
			return ReadArrayBlock<float>(new ReadObject<float>((br) => br.ReadFloatWithHeader()));
		}

		public int[] ReadIntArrayBlock()
		{
			return ReadArrayBlock<int>(new ReadObject<int>((br) => br.ReadIntWithHeader()));
		}

		public LRQuaternion[] ReadQuaternionArrayBlock()
		{
			return ReadArrayBlock<LRQuaternion>(new ReadObject<LRQuaternion>((br) => LRQuaternion.Read(br)));
		}

		public string[] ReadStringArrayBlock()
		{
			return ReadArrayBlock<string>(new ReadObject<string>((br) => br.ReadStringWithHeader()));
		}

		public LRVector2[] ReadVector2fArrayBlock()
		{
			return ReadArrayBlock<LRVector2>(new ReadObject<LRVector2>((br) => LRVector2.Read(br)));
		}

		public LRVector3[] ReadVector3fArrayBlock()
		{
			return ReadArrayBlock<LRVector3>(new ReadObject<LRVector3>((br) => LRVector3.Read(br)));
		}
	}
}