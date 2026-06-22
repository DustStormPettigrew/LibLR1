using System;

namespace LibLR1
{
	/// <summary>Indices into the L_COLORS.LEB master palette. The Biege spelling intentionally matches the in-game source file rather than the corrected Beige spelling. Per-piece valid colors are governed by *_CSET.LEB files; this enum is the global palette only.</summary>
	public enum BrickColor : byte
	{
		Black = 0x00,
		White = 0x01,
		Biege = 0x02,
		LtGray = 0x03,
		DkGray = 0x04,
		Red = 0x05,
		Brown = 0x06,
		Yellow = 0x07,
		Green = 0x08,
		Blue = 0x09
	}

	/// <summary>LRS brick-set tags. ChassisBuiltin (0x00) is the sentinel for pieces native to the chassis that have no external CSET file. These values are distinct from CRSTMGR's ordinal registry positions.</summary>
	public enum BrickSetTag : byte
	{
		ChassisBuiltin = 0x00,
		JohnnyThunder = 0x0B,
		GypsyMoth = 0x0C,
		BaronVonBarron = 0x0D,
		Basil = 0x0E,
		CaptainRedbeard = 0x0F,
		KingKahuka = 0x10,
		DefaultSet1 = 0x11,
		DefaultSet2 = 0x12,
		DefaultSet3 = 0x13,
		DefaultSet4 = 0x14,
		RocketRacer = 0x15,
		VeronicaVoltage = 0x16
	}

	public class BrickPlacement
	{
		public const int Size = 8;
		public const byte DefaultBrickTypeToken = 0x13;

		private byte m_rotationVariantByte;

		/// <summary>LR token-class byte. Observed values are 0x0C and 0x10 through 0x16; 0x13 is a regular molded brick. Token 0x14 marks compound or decorated pieces. These are often paired, but single CR-set accessory placements are observed.</summary>
		public byte BrickTypeToken = DefaultBrickTypeToken;
		public byte BrickId;
		/// <summary>Unsigned front/back position. Valid range is chassis-dependent; ddchas0 confirms 0x00 through 0x08.</summary>
		public byte PositionA;
		public byte PositionB;

		/// <summary>Raw byte 4. Bits 0-1 are Rotation and bits 2-7 are Variant.</summary>
		public byte RotationVariantByte
		{
			get { return m_rotationVariantByte; }
			set { m_rotationVariantByte = value; }
		}

		/// <summary>Two-bit rotation: 0, 90, 180, or 270 degrees.</summary>
		public byte Rotation
		{
			get { return (byte)(m_rotationVariantByte & 0x03); }
			set { m_rotationVariantByte = (byte)((m_rotationVariantByte & 0xFC) | (value & 0x03)); }
		}

		/// <summary>Six-bit brick decoration/face variant.</summary>
		public byte Variant
		{
			get { return (byte)(m_rotationVariantByte >> 2); }
			set { m_rotationVariantByte = (byte)((m_rotationVariantByte & 0x03) | ((value & 0x3F) << 2)); }
		}

		/// <summary>Primary L_COLORS.LEB palette index. Validity is per (piece name, SetTag) according to the matching CSET file.</summary>
		public byte Color;
		/// <summary>Secondary L_COLORS.LEB palette index for two-tone bricks; zero means none.</summary>
		public byte ColorSecondary;
		/// <summary>Brick-set namespace: 0x00 is chassis-built-in; 0x0B-0x10 and 0x15-0x16 are character sets; 0x11-0x14 are the default sets.</summary>
		public byte SetTag;

		public static bool IsKnownBrickTypeToken(byte p_value)
		{
			return p_value == 0x0C || (p_value >= 0x10 && p_value <= 0x16);
		}

		public static BrickPlacement Read(ReadOnlySpan<byte> p_bytes)
		{
			if (p_bytes.Length < Size)
			{
				throw new ArgumentException("BrickPlacement requires exactly 8 bytes.", nameof(p_bytes));
			}

			return new BrickPlacement
			{
				BrickTypeToken = p_bytes[0],
				BrickId = p_bytes[1],
				PositionA = p_bytes[2],
				PositionB = p_bytes[3],
				RotationVariantByte = p_bytes[4],
				Color = p_bytes[5],
				ColorSecondary = p_bytes[6],
				SetTag = p_bytes[7]
			};
		}

		public void Write(Span<byte> p_bytes)
		{
			if (p_bytes.Length < Size)
			{
				throw new ArgumentException("BrickPlacement requires exactly 8 bytes.", nameof(p_bytes));
			}

			p_bytes[0] = BrickTypeToken;
			p_bytes[1] = BrickId;
			p_bytes[2] = PositionA;
			p_bytes[3] = PositionB;
			p_bytes[4] = RotationVariantByte;
			p_bytes[5] = Color;
			p_bytes[6] = ColorSecondary;
			p_bytes[7] = SetTag;
		}
	}
}
