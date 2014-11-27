using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LibLR1.IO
{
	public class LRBinaryWriter : IDisposable
	{
		private BinaryWriter m_baseWriter;
		private bool m_closeOnDispose;

		public Stream BaseStream { get { return m_baseWriter.BaseStream; } }

		public LRBinaryWriter(Stream p_stream, bool p_closeOnDispose = true)
		{
			m_baseWriter = new BinaryWriter(p_stream);
			m_closeOnDispose = p_closeOnDispose;
		}

		public void Dispose()
		{
			if (m_closeOnDispose)
			{
				m_baseWriter.BaseStream.Close();
			}
		}

		public void WriteToken(Token p_token)
		{
			m_baseWriter.Write((byte)p_token);
		}

		public void WriteByte(byte p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteSByte(sbyte p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteByteWithHeader(byte p_value)
		{
			WriteToken(Token.BYTE);
			WriteByte(p_value);
		}

		public void WriteFract8BitWithHeader(Fract8Bit p_value)
		{
			WriteToken(Token.FRACT8);
			WriteSByte( p_value.Value);
		}

		public void WriteUShort(ushort p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteShort(short p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteFract16BitWithHeader(Fract16Bit p_value)
		{
			WriteToken(Token.FRACT16);
			WriteShort(p_value.Value);
		}

		public void WriteUShortWithHeader(ushort p_value)
		{
			WriteToken(Token.USHORT);
			WriteUShort(p_value);
		}

		public void WriteUInt(uint p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteInt(int p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteIntWithHeader(int p_value)
		{
			WriteToken(Token.INT32);
			WriteInt(p_value);
		}

		public void WriteIntegralWithHeader(int p_value)
		{
			if (p_value >= 0)
			{
				if (p_value < 256)
				{
					WriteByteWithHeader((byte)p_value);
				}
				else if (p_value < 65536)
				{
					WriteUShortWithHeader((ushort)p_value);
				}
				else
				{
					WriteIntWithHeader(p_value);
				}
			}
			else
			{
				WriteIntWithHeader(p_value);
			}
		}

		public void WriteFloat(float p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteFloatWithHeader(float p_value)
		{
			WriteToken(Token.FLOAT);
			WriteFloat(p_value);
		}

		public void WriteString(string p_value)
		{
			byte[] buffer = Encoding.ASCII.GetBytes(p_value);
			m_baseWriter.Write(buffer);
			m_baseWriter.Write((byte)0x00);
		}

		public void WriteStringWithHeader(string p_value)
		{
			WriteToken(Token.STRING);
			WriteString(p_value);
		}

		public delegate void WriteObject<T>(LRBinaryWriter p_writer, T p_value);

		public void WriteArrayBlock<T>(WriteObject<T> p_writeFunc, T[] p_values)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			for (int i = 0; i < p_values.Length; i++)
			{
				p_writeFunc(this, p_values[i]);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteListBlock<T>(WriteObject<T> p_writeFunc, List<T> p_values)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writeFunc(this, p_values[i]);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteStructArrayBlock<T>(WriteObject<T> p_writeFunc, T[] p_values, byte p_typeByte)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			for (int i = 0; i < p_values.Length; i++)
			{
				WriteByte(p_typeByte);
				WriteStruct<T>(p_writeFunc, p_values[i]);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteStructListBlock<T>(WriteObject<T> p_writeFunc, List<T> p_values, byte p_typeByte)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			for (int i = 0; i < p_values.Count; i++)
			{
				WriteByte(p_typeByte);
				WriteStruct<T>(p_writeFunc, p_values[i]);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteDictionaryBlock<T>(WriteObject<T> p_writeFunc, Dictionary<string, T> p_values, byte p_typeByte)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				WriteByte(p_typeByte);
				WriteStringWithHeader(kvp.Key);
				WriteStruct<T>(p_writeFunc, kvp.Value);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteCollidableDictionaryBlock<T>(WriteObject<T> p_writeFunc, KeyValuePair<string, T>[] p_values, byte p_typeByte)
		{
			WriteToken(Token.LEFT_BRACKET);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RIGHT_BRACKET);
			WriteToken(Token.LEFT_CURLY);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				WriteByte(p_typeByte);
				WriteStringWithHeader(kvp.Key);
				WriteStruct<T>(p_writeFunc, kvp.Value);
			}
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteStruct<T>(WriteObject<T> p_writeFunc, T p_value)
		{
			WriteToken(Token.LEFT_CURLY);
			p_writeFunc(this, p_value);
			WriteToken(Token.RIGHT_CURLY);
		}

		public void WriteByteArrayBlock(byte[] p_values)
		{
			WriteArrayBlock<byte>(new WriteObject<byte>((bw, val) => WriteByteWithHeader(val)), p_values);
		}

		public void WriteFloatArrayBlock(float[] p_values)
		{
			WriteArrayBlock<float>(new WriteObject<float>((bw, val) => WriteFloatWithHeader(val)), p_values);
		}

		public void WriteIntArrayBlock(int[] p_values)
		{
			WriteArrayBlock<int>(new WriteObject<int>((bw, val) => WriteIntWithHeader(val)), p_values);
		}

		public void WriteQuaternionArrayBlock(LRQuaternion[] p_values)
		{
			WriteArrayBlock<LRQuaternion>(new WriteObject<LRQuaternion>(LRQuaternion.Write), p_values);
		}

		public void WriteStringArrayBlock(string[] p_values)
		{
			WriteArrayBlock<string>(new WriteObject<string>((bw, val) => bw.WriteStringWithHeader(val)), p_values);
		}

		public void WriteVector2fArrayBlock(LRVector2[] p_values)
		{
			WriteArrayBlock<LRVector2>(new WriteObject<LRVector2>(LRVector2.Write), p_values);
		}

		public void WriteVector3fArrayBlock(LRVector3[] p_values)
		{
			WriteArrayBlock<LRVector3>(new WriteObject<LRVector3>(LRVector3.Write), p_values);
		}
	}
}