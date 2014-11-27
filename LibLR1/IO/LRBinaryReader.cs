using LibLR1.Exceptions;
using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
			Token actual = ReadToken();
			if (actual != p_expected)
			{
				throw new Exception(string.Format("Invalid data. Expected {0}, got {1}.", p_expected, actual));
			}
		}

		[Obsolete] // should I remove this?
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
			Token actual = ReadToken();
			if (p_expected.Contains(actual) == false)
			{
				throw new Exception(string.Format("Invalid data. Expected [{0}], got {1}.", string.Join(", ", p_expected), actual));
			}
			return actual;
		}

		public bool Next(Token p_expected)
		{
			Token actual = ReadToken();
			m_baseReader.BaseStream.Position--;
			return actual == p_expected;
		}

		public Token ReadToken()
		{
			return (Token)ReadByte();
		}

		public byte ReadByte()
		{
			return m_baseReader.ReadByte();
		}

		public byte[] ReadBytes(int p_count)
		{
			return m_baseReader.ReadBytes(p_count);
		}

		public sbyte ReadSByte()
		{
			return m_baseReader.ReadSByte();
		}

		public byte ReadByteWithHeader()
		{
			Expect(Token.Byte);
			return ReadByte();
		}

		public sbyte ReadSByteWithHeader()
		{
			Expect(Token.SByte);
			return ReadSByte();
		}

		public Fract8Bit ReadFract8BitWithHeader()
		{
			Expect(Token.Fract8);
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
			Expect(Token.UShort);
			return ReadUShort();
		}

		public short ReadShortWithHeader()
		{
			Expect(Token.Short);
			return ReadShort();
		}

		public Fract16Bit ReadFract16BitWithHeader()
		{
			Expect(Token.Fract16);
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
			Expect(Token.Int32);
			return ReadInt();
		}

		public int ReadIntegralWithHeader()
		{
			Token type = Expect(new Token[] { Token.SByte, Token.Byte, Token.Int32, Token.UShort, Token.Short });
			switch (type)
			{
				case Token.SByte:  return ReadSByte();
				case Token.Byte:   return ReadByte();
				case Token.Int32:  return ReadInt();
				case Token.UShort: return ReadUShort();
				case Token.Short:  return ReadShort();
			}
			throw new UnexpectedTypeException(type, m_baseReader.BaseStream.Position - 1);
		}

		public float ReadFloat()
		{
			return m_baseReader.ReadSingle();
		}

		public float ReadFloatWithHeader()
		{
			Expect(Token.Float);
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
			Expect(Token.String);
			return ReadString();
		}

		public T[] ReadArrayBlock<T>(Func<LRBinaryReader, T> p_readFunc)
		{
			// [array_len]
			// {
			//     output[0],
			//     output[1],
			//     :
			//     output[array_len - 1]
			// }
			Expect(Token.LeftBracket);
			int array_len = ReadIntWithHeader();
			T[] output = new T[array_len];
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < array_len; i++)
			{
				output[i] = p_readFunc(this);
			}
			Expect(Token.RightCurly);
			return output;
		}

		public List<T> ReadListBlock<T>(Func<LRBinaryReader, T> p_readFunc)
		{
			Expect(Token.LeftBracket);
			int array_len = ReadIntWithHeader();
			List<T> output = new List<T>();
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < array_len; i++)
			{
				output.Add(p_readFunc(this));
			}
			Expect(Token.RightCurly);
			return output;
		}

		public T[] ReadStructArrayBlock<T>(Func<LRBinaryReader, T> p_readFunc, byte p_typeByte)
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
			Expect(Token.LeftBracket);
			int array_len = ReadIntWithHeader();
			T[] output = new T[array_len];
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_typeByte);
				output[i] = ReadStruct<T>(p_readFunc);
			}
			Expect(Token.RightCurly);
			return output;
		}

		public List<T> ReadStructListBlock<T>(Func<LRBinaryReader, T> p_readFunc, byte p_typeByte)
		{
			Expect(Token.LeftBracket);
			int array_len = ReadIntWithHeader();
			List<T> output = new List<T>();
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < array_len; i++)
			{
				Expect(p_typeByte);
				output.Add(ReadStruct<T>(p_readFunc));
			}
			Expect(Token.RightCurly);
			return output;
		}

		public Dictionary<string, T> ReadDictionaryBlock<T>(Func<LRBinaryReader, T> p_readFunc, byte p_typeByte)
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
			Expect(Token.LeftBracket);
			int dict_len = ReadIntWithHeader();
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_typeByte);
				string item_key = i.ToString();
				if (Next(Token.String))
				{
					item_key = ReadStringWithHeader();
				}
				T item_value = ReadStruct<T>(p_readFunc);
				output.Add(item_key, item_value);
			}
			Expect(Token.RightCurly);
			return output;
		}

		public KeyValuePair<string, T>[] ReadCollidableDictionaryBlock<T>(Func<LRBinaryReader, T> p_readFunc, byte p_typeByte)
		{
			// not a fucking clue what this is about.
			Expect(Token.LeftBracket);
			int dict_len = ReadIntWithHeader();
			KeyValuePair<string, T>[] output = new KeyValuePair<string, T>[dict_len];
			Expect(Token.RightBracket);
			Expect(Token.LeftCurly);
			for (int i = 0; i < dict_len; i++)
			{
				Expect(p_typeByte);
				string item_key = i.ToString();
				if (Next(Token.String))
				{
					item_key = ReadStringWithHeader();
				}
				T item_value = ReadStruct<T>(p_readFunc);
				output[i] = new KeyValuePair<string, T>(item_key, item_value);
			}
			Expect(Token.RightCurly);
			return output;
		}

		public T ReadStruct<T>(Func<LRBinaryReader, T> p_readFunc)
		{
			Expect(Token.LeftCurly);
			T output = p_readFunc(this);
			Expect(Token.RightCurly);
			return output;
		}

		public byte[] ReadByteArrayBlock()
		{
			return ReadArrayBlock<byte>((br) => br.ReadByteWithHeader());
		}

		public float[] ReadFloatArrayBlock()
		{
			return ReadArrayBlock<float>((br) => br.ReadFloatWithHeader());
		}

		public int[] ReadIntArrayBlock()
		{
			return ReadArrayBlock<int>((br) => br.ReadIntWithHeader());
		}

		public LRQuaternion[] ReadQuaternionArrayBlock()
		{
			return ReadArrayBlock<LRQuaternion>(LRQuaternion.Read);
		}

		public string[] ReadStringArrayBlock()
		{
			return ReadArrayBlock<string>((br) => br.ReadStringWithHeader());
		}

		public LRVector2[] ReadVector2fArrayBlock()
		{
			return ReadArrayBlock<LRVector2>(LRVector2.Read);
		}

		public LRVector3[] ReadVector3fArrayBlock()
		{
			return ReadArrayBlock<LRVector3>(LRVector3.Read);
		}
	}
}