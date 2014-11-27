#define FAIL_ON_COMPRESSED_BLOCK

using System;
using System.IO;
using LibLR1.Utils;
using LibLR1.IO;

namespace LibLR1
{
	public class BMP
	{
		private const byte
			ENCODING_4BPP = 0x04,
			ENCODING_8BPP = 0x08,
			ENCODING_RGB  = 0x98;
		
		private BMP_Header m_header;
		private BMP_Color[] m_palette;
		
		public BMP_Header Header
		{
			get { return m_header; }
			set { m_header = value; }
		}
		public BMP_Color[] Palette
		{
			get { return m_palette; }
			set { m_palette = value; }
		}

		public BMP(string p_filepath)
			: this(new LRBinaryReader(File.OpenRead(p_filepath)))
		{
		}

		public BMP(LRBinaryReader p_reader)
		{
			m_header = BMP_Header.Read(p_reader);
			if (m_header.Encoding == ENCODING_RGB)
			{
				throw new Exception("Unimplemented encoding 0x" + m_header.Encoding.ToString("X2"));
			}
			
			m_palette = new BMP_Color[m_header.PaletteLength + 1];
			for (int i = 0; i < m_palette.Length; i++)
			{
				m_palette[i] = BMP_Color.Read(p_reader);
			}

			while (p_reader.BaseStream.Position < p_reader.BaseStream.Length)
			{
				ushort block_len_raw = p_reader.ReadUShort();
				ushort block_len_com = p_reader.ReadUShort();
				byte[] data_com = new byte[block_len_com];
				byte[] data_raw = new byte[block_len_raw];
				p_reader.BaseStream.Read(data_com, 0, block_len_com);
				if (block_len_com == block_len_raw)
				{
					data_com.CopyTo(data_raw, 0);
				}
				else
				{
#if FAIL_ON_COMPRESSED_BLOCK
					throw new Exception("BMP: FAIL_ON_COMPRESSED_BLOCK");
#endif
				}
			}
		}
	}
	
	public class BMP_Header
	{
		public byte   Encoding;
		public byte   PaletteLength;
		public ushort Width;
		public ushort Height;

		public static BMP_Header Read(LRBinaryReader p_reader)
		{
			BMP_Header val = new BMP_Header();
			val.Encoding      = p_reader.ReadByte();
			val.PaletteLength = p_reader.ReadByte();
			val.Width         = p_reader.ReadUShort();
			val.Height        = p_reader.ReadUShort();
			return val;
		}
	}
	
	public class BMP_Color
	{
		public byte R, G, B;

		public static BMP_Color Read(LRBinaryReader p_reader)
		{
			BMP_Color val = new BMP_Color();
			val.B = p_reader.ReadByte();
			val.G = p_reader.ReadByte();
			val.R = p_reader.ReadByte();
			return val;
		}
	}
}