using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LibLR1
{
	public class BMP
	{
		private int m_width, m_height;
		private BitmapColor[] m_image;

		public int Width  { get { return m_width;  } }
		public int Height { get { return m_height; } }

		public BitmapColor GetPixel(int p_x, int p_y)
		{
			if (p_x < 0 || p_y < 0 || p_x >= m_width || p_y >= m_height)
			{
				throw new IndexOutOfRangeException();
			}
			return m_image[p_y * m_width + p_x];
		}

		private enum ImageEncoding : byte
		{
			Palette4Bit = 0x04,
			Palette8Bit = 0x08,
			RGB         = 0x98,
		}

		public BMP(string p_filepath)
		{
			using (BinaryReader br = new BinaryReader(File.OpenRead(p_filepath)))
			{
				ImageEncoding encoding = (ImageEncoding)br.ReadByte();

				// check if it's a valid encoding
				if (((ImageEncoding[])Enum.GetValues(typeof(ImageEncoding))).Contains(encoding) == false)
				{
					throw new InvalidDataException(string.Format("Unexpected bitmap encoding 0x{0:X2}", (byte)encoding));
				}

				byte paletteSize = br.ReadByte();

				m_width  = br.ReadInt16();
				m_height = br.ReadInt16();

				BitmapColor[] palette;
				if (encoding == ImageEncoding.RGB)
				{
					palette = new BitmapColor[0];
				}
				else
				{
					palette = new BitmapColor[paletteSize + 1];
					for (int i = 0; i < palette.Length; ++i)
					{
						palette[i] = new BitmapColor(br);
					}
				}

				int bufferLength = 0;
				switch (encoding)
				{
					case ImageEncoding.Palette4Bit:
					{
						//TODO: check the +1
						bufferLength = (m_width * m_height + 1) / 2;
						break;
					}
					case ImageEncoding.Palette8Bit:
					{
						bufferLength = m_width * m_height;
						break;
					}
					case ImageEncoding.RGB:
					{
						bufferLength = m_width * m_height * 3;
						break;
					}
				}

				byte[] imageBuffer = new byte[bufferLength];
				int bufferPos = 0;
				while (bufferPos < bufferLength)
				{
					byte[] block = ReadBlock(br);
					Array.Copy(block, 0, imageBuffer, bufferPos, block.Length);
					bufferPos += block.Length;
				}

				m_image = new BitmapColor[m_width * m_height];
				switch (encoding)
				{
					case ImageEncoding.RGB:
					{
						for (int i = 0; i < m_width * m_height; ++i)
						{
							//TODO: possibly the other way round? (bgr vs rgb)
							m_image[i] = new BitmapColor(
								imageBuffer[i * 3 + 0],
								imageBuffer[i * 3 + 1],
								imageBuffer[i * 3 + 2]
							);
						}
						break;
					}
					case ImageEncoding.Palette4Bit:
					{
						for (int i = 0; i < m_width * m_height; ++i)
						{
							byte index = imageBuffer[i / 2];
							index >>= 4 * (1 - (i % 2));
							index &= 0xF;
							m_image[i] = palette[index];
						}
						break;
					}
					case ImageEncoding.Palette8Bit:
					{
						for (int i = 0; i < m_width * m_height; ++i)
						{
							byte index = imageBuffer[i];
							m_image[i] = palette[index];
						}
						break;
					}
				}
			}
		}

		private byte[] ReadBlock(BinaryReader p_reader)
		{
			short blockLengthDecompressed = p_reader.ReadInt16();
			short blockLengthCompressed = p_reader.ReadInt16();

			if (blockLengthCompressed == blockLengthDecompressed)
			{
				return p_reader.ReadBytes(blockLengthCompressed);
			}

			List<byte> buffer = new List<byte>(blockLengthDecompressed); // allocate a buffer for the block
			buffer.Add(p_reader.ReadByte()); // always copy the first byte

			while (ReadSubBlock(p_reader, buffer)) ;

			Debug.Assert(buffer.Count == blockLengthDecompressed);
			return buffer.ToArray();
		}

		private bool ReadSubBlock(BinaryReader p_reader, List<byte> p_buffer)
		{
			// mad thanks to Sluicer for this <3

			byte blockMap = p_reader.ReadByte();
			for (int i = 0; i < 8; i++) // for all eight bits in blockMap
			{
				if ((blockMap & 0x80) != 0) // if highest bit is set - RLE
				{
					byte foo = p_reader.ReadByte();
					int repeat = foo & 0x0F;
					int goback = (foo & 0xF0) << 4;

					goback += p_reader.ReadByte();
					if (repeat != 0)
					{
						repeat = -(repeat - 0x12);
					}
					else
					{
						if (repeat == 0 && goback == 0)
						{
							return false; // section is finished
						}

						repeat = p_reader.ReadByte() + 0x12; // override
					}

					for (int j = 0; j < repeat; j++)
					{
						p_buffer.Add(p_buffer[p_buffer.Count - goback]);
					}
				}
				else // if highest bit is not set - copy byte
				{
					p_buffer.Add(p_reader.ReadByte());
				}
				blockMap <<= 1; // move to next bit
			}
			return true;
		}
	}

	public struct BitmapColor
	{
		public byte r, g, b;

		public BitmapColor(BinaryReader p_reader)
		{
			b = p_reader.ReadByte();
			g = p_reader.ReadByte();
			r = p_reader.ReadByte();
		}

		public BitmapColor(byte p_r, byte p_g, byte p_b)
		{
			r = p_r;
			g = p_g;
			b = p_b;
		}

		public override string ToString()
		{
			return string.Format("BitmapColor({0}, {1}, {2})", r, g, b);
		}
	}
}