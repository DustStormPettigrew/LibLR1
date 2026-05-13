using LibLR1.Utils;
using System;
using System.Collections.Generic;
using System.IO;
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

		public void WriteBytes(byte[] p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteSByte(sbyte p_value)
		{
			m_baseWriter.Write(p_value);
		}

		public void WriteByteWithHeader(byte p_value)
		{
			WriteToken(Token.Byte);
			WriteByte(p_value);
		}

		public void WriteFract8BitWithHeader(Fract8Bit p_value)
		{
			WriteToken(Token.Fract8);
			Fract8Bit.Write(this, p_value);
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
			WriteToken(Token.Fract16);
			Fract16Bit.Write(this, p_value);
		}

		public void WriteUShortWithHeader(ushort p_value)
		{
			WriteToken(Token.UShort);
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
			WriteToken(Token.Int32);
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
			WriteToken(Token.Float);
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
			WriteToken(Token.String);
			WriteString(p_value);
		}

		public void WriteArrayBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, T[] p_values)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_values.Length; i++)
			{
				p_writeFunc(this, p_values[i]);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteListBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, List<T> p_values)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_values.Count; i++)
			{
				p_writeFunc(this, p_values[i]);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteStructArrayBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, T[] p_values, byte p_typeByte)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_values.Length; i++)
			{
				WriteByte(p_typeByte);
				WriteStruct<T>(p_writeFunc, p_values[i]);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteStructListBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, List<T> p_values, byte p_typeByte)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			for (int i = 0; i < p_values.Count; i++)
			{
				WriteByte(p_typeByte);
				WriteStruct<T>(p_writeFunc, p_values[i]);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteDictionaryBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, Dictionary<string, T> p_values, byte p_typeByte)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Count);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				WriteByte(p_typeByte);
				WriteStringWithHeader(kvp.Key);
				WriteStruct<T>(p_writeFunc, kvp.Value);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteCollidableDictionaryBlock<T>(Action<LRBinaryWriter, T> p_writeFunc, KeyValuePair<string, T>[] p_values, byte p_typeByte)
		{
			WriteToken(Token.LeftBracket);
			WriteIntWithHeader(p_values.Length);
			WriteToken(Token.RightBracket);
			WriteToken(Token.LeftCurly);
			foreach (KeyValuePair<string, T> kvp in p_values)
			{
				WriteByte(p_typeByte);
				WriteStringWithHeader(kvp.Key);
				WriteStruct<T>(p_writeFunc, kvp.Value);
			}
			WriteToken(Token.RightCurly);
		}

		public void WriteStruct<T>(Action<LRBinaryWriter, T> p_writeFunc, T p_value)
		{
			WriteToken(Token.LeftCurly);
			p_writeFunc(this, p_value);
			WriteToken(Token.RightCurly);
		}

		public void WriteByteArrayBlock(byte[] p_values)
		{
			WriteArrayBlock<byte>((bw, val) => WriteByteWithHeader(val), p_values);
		}

		public void WriteFloatArrayBlock(float[] p_values)
		{
			WriteArrayBlock<float>((bw, val) => WriteFloatWithHeader(val), p_values);
		}

		public void WriteIntArrayBlock(int[] p_values)
		{
			WriteArrayBlock<int>((bw, val) => WriteIntWithHeader(val), p_values);
		}

		public void WriteQuaternionArrayBlock(LRQuaternion[] p_values)
		{
			WriteArrayBlock<LRQuaternion>(LRQuaternion.Write, p_values);
		}

		public void WriteStringArrayBlock(string[] p_values)
		{
			WriteArrayBlock<string>((bw, val) => bw.WriteStringWithHeader(val), p_values);
		}

		public void WriteVector2fArrayBlock(LRVector2[] p_values)
		{
			WriteArrayBlock<LRVector2>(LRVector2.Write, p_values);
		}

		public void WriteVector3fArrayBlock(LRVector3[] p_values)
		{
			WriteArrayBlock<LRVector3>(LRVector3.Write, p_values);
		}
	}
}