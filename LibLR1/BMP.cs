#define FAIL_ON_COMPRESSED_BLOCK

using System;
using System.IO;
using LibLR1.Utils;

namespace LibLR1
{
	public class BMP
	{
		private const byte
			ENCODING_4BPP = 0x04,
			ENCODING_8BPP = 0x08,
			ENCODING_RGB  = 0x98;
		
		private BMP_Header m_Header;
		private BMP_Color[] m_Palette;
		
		public BMP_Header Header
		{
			get { return m_Header; }
			set { m_Header = value; }
		}
		public BMP_Color[] Palette
		{
			get { return m_Palette; }
			set { m_Palette = value; }
		}
		
		public BMP(Stream stream)
		{
			m_Header = BMP_Header.FromStream(stream);
			if (m_Header.Encoding == ENCODING_RGB)
			{
				throw new Exception("Unimplemented encoding 0x" + m_Header.Encoding.ToString("X2"));
			}
			
			m_Palette = new BMP_Color[m_Header.PaletteLength + 1];
			for (int i = 0; i < m_Palette.Length; i++)
			{
				m_Palette[i] = BMP_Color.FromStream(stream);
			}
			
			while (stream.Position < stream.Length)
			{
				ushort block_len_raw = BinaryFileHelper.ReadUShort(stream);
				ushort block_len_com = BinaryFileHelper.ReadUShort(stream);
				byte[] data_com = new byte[block_len_com];
				byte[] data_raw = new byte[block_len_raw];
				stream.Read(data_com, 0, block_len_com);
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
		
		public BMP(string path)
			: this(new FileStream(path, FileMode.Open, FileAccess.Read))
		{
		}
	}
	
	public class BMP_Header
	{
		public byte   Encoding;
		public byte   PaletteLength;
		public ushort Width;
		public ushort Height;
		
		public static BMP_Header FromStream(Stream stream)
		{
			BMP_Header val = new BMP_Header();
			val.Encoding = BinaryFileHelper.ReadByte(stream);
			val.PaletteLength = BinaryFileHelper.ReadByte(stream);
			val.Width = BinaryFileHelper.ReadUShort(stream);
			val.Height = BinaryFileHelper.ReadUShort(stream);
			return val;
		}
	}
	
	public class BMP_Color
	{
		public byte R, G, B;
		
		public static BMP_Color FromStream(Stream stream)
		{
			BMP_Color val = new BMP_Color();
			val.B = BinaryFileHelper.ReadByte(stream);
			val.G = BinaryFileHelper.ReadByte(stream);
			val.R = BinaryFileHelper.ReadByte(stream);
			return val;
		}
	}
}