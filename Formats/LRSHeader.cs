using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibLR1
{
	public enum BossSet
	{
		CaptainRedbeard = 0,
		GypsyMoth = 1,
		Basil = 2,
		JohnnyThunder = 3,
		KingKahuka = 4,
		BaronVonBarron = 5,
		RocketRacer = 6,
		VeronicaVoltage = 7
	}

	public enum CircuitCleared
	{
		CaptainRedbeard = 1,
		GypsyMoth = 2,
		Basil = 3,
		JohnnyThunder = 4,
		KingKahuka = 5,
		BaronVonBarron = 6
	}

	public enum LeaderboardBlock
	{
		ImperialGrandPrix = 0,
		DarkForestDash = 1,
		MagmaMoonMarathon = 2,
		DesertAdventureDragway = 3,
		TribalIslandTrail = 4,
		RoyalKnightsRaceway = 5,
		IcePlanetPathway = 6,
		AmazonAdventureAlley = 7,
		KnightmareAthon = 8,
		PirateSkullPass = 9,
		AdventureTempleTrail = 10,
		AlienRallyAsteroid = 11
	}

	/// <summary>Language identifiers stored in the LRS settings region.</summary>
	public enum LrLanguage : byte
	{
		English = 0,
		Spanish = 1,
		French = 2,
		Deutsch = 3,
		Italian = 4,
		Dansk = 5,
		Svenska = 6,
		Norsk = 7,
		Nederlands = 8,
		Suomi = 9
	}

	/// <summary>Player input device selector stored in an LRS header. Value 0x01 is unknown and is preserved as a raw value.</summary>
	public enum LrInputDevice : byte
	{
		Gamepad = 0x00,
		KeyboardLayout1 = 0x02,
		KeyboardLayout2 = 0x03,
		KeyboardLayout3 = 0x04
	}

	/// <summary>
	/// Decoded view of the fixed 0x480-byte LRS header. Properties write through to the supplied raw header buffer.
	/// </summary>
	public sealed class LRSHeader
	{
		public const int Size = LRS.HeaderSize;
		public const int LeaderboardBlockCount = 12;
		public const int CircuitSeparatorCount = 5;
		private const int LeaderboardOffset = 0x0009;
		private const int PaddingOffset = 0x030E;
		private const int PaddingSize = 82;
		private const int CircuitCompletionOffset = 0x0360;
		private const int CircuitCompletionSize = 288;

		private readonly byte[] m_bytes;
		private readonly List<TrackLeaderboard> m_trackLeaderboards;

		internal LRSHeader(byte[] p_bytes)
		{
			if (p_bytes == null || p_bytes.Length != Size)
				throw new ArgumentException(string.Format("LRS header must be exactly {0} bytes.", Size), nameof(p_bytes));

			m_bytes = p_bytes;
			m_trackLeaderboards = new List<TrackLeaderboard>(LeaderboardBlockCount);
			for (int i = 0; i < LeaderboardBlockCount; i++)
				m_trackLeaderboards.Add(new TrackLeaderboard(this, i, GetLeaderboardBlockOffset(i)));
		}

		public string Magic
		{
			get { return Encoding.ASCII.GetString(m_bytes, 0, 2); }
		}

		public byte ActiveRacerCount
		{
			get { return m_bytes[0x0002]; }
			set { m_bytes[0x0002] = value; }
		}

		public byte[] FormatVersionBytes
		{
			get { return CopyBytes(0x0003, 2); }
			set { WriteBytes(0x0003, value, 2, nameof(FormatVersionBytes)); }
		}

		public byte BossSetUnlockFlags
		{
			get { return m_bytes[0x0005]; }
			set { m_bytes[0x0005] = value; }
		}

		public bool CaptainRedbeardUnlocked
		{
			get { return (BossSetUnlockFlags & 0x01) != 0; }
			set { SetFlag(0x0005, 0, value); }
		}

		public byte CircuitClearedFlags
		{
			get { return m_bytes[0x0006]; }
			set { m_bytes[0x0006] = value; }
		}

		[Obsolete("Use CircuitClearedFlags.")]
		public byte OtherProgressionFlags
		{
			get { return CircuitClearedFlags; }
			set { CircuitClearedFlags = value; }
		}

		public bool[] TimeTrialCompleted
		{
			get
			{
				bool[] result = new bool[LeaderboardBlockCount];
				for (int i = 0; i < result.Length; i++)
					result[i] = GetTimeTrialCompleted(i);
				return result;
			}
			set
			{
				if (value == null || value.Length != LeaderboardBlockCount)
					throw new ArgumentException("LRS time-trial completion array must contain 12 entries.", nameof(value));
				for (int i = 0; i < value.Length; i++)
					SetTimeTrialCompleted(i, value[i]);
			}
		}

		public IReadOnlyList<TrackLeaderboard> TrackLeaderboards
		{
			get { return m_trackLeaderboards; }
		}

		public byte[] CircuitSeparators
		{
			get
			{
				byte[] result = new byte[CircuitSeparatorCount];
				for (int i = 0; i < result.Length; i++)
					result[i] = m_bytes[GetCircuitSeparatorOffset(i)];
				return result;
			}
			set
			{
				if (value == null || value.Length != CircuitSeparatorCount)
					throw new ArgumentException("LRS circuit separator array must contain 5 bytes.", nameof(value));
				for (int i = 0; i < value.Length; i++)
					m_bytes[GetCircuitSeparatorOffset(i)] = value[i];
			}
		}

		public byte[] PaddingRegion
		{
			get { return CopyBytes(PaddingOffset, PaddingSize); }
			set { WriteBytes(PaddingOffset, value, PaddingSize, nameof(PaddingRegion)); }
		}

		public byte[] CircuitCompletionRegion
		{
			get { return CopyBytes(CircuitCompletionOffset, CircuitCompletionSize); }
			set { WriteBytes(CircuitCompletionOffset, value, CircuitCompletionSize, nameof(CircuitCompletionRegion)); }
		}

		/// <summary>Currently selected driver slot at 0x0367. Compacts with the racer list.</summary>
		public byte CurrentDriverSlot { get { return m_bytes[0x0367]; } set { m_bytes[0x0367] = value; } }

		/// <summary>Default opponent count at 0x034F. Valid game range is 0 through 5.</summary>
		public byte DefaultOpponentCount { get { return m_bytes[0x034F]; } set { m_bytes[0x034F] = value; } }

		/// <summary>Language identifier at 0x0365. Use <see cref="Language"/> where possible.</summary>
		public byte LanguageId { get { return m_bytes[0x0365]; } set { m_bytes[0x0365] = value; } }

		/// <summary>Language identifier at 0x0365 as a typed value.</summary>
		public LrLanguage Language { get { return (LrLanguage)LanguageId; } set { LanguageId = (byte)value; } }

		/// <summary>Default lap count at 0x0366. Valid game range is 1 through 5.</summary>
		public byte DefaultLapCount { get { return m_bytes[0x0366]; } set { m_bytes[0x0366] = value; } }

		/// <summary>Music volume at 0x0362. Confirmed by the isolated music-slider transition.</summary>
		public byte MusicVolume { get { return m_bytes[0x0362]; } set { m_bytes[0x0362] = value; } }

		/// <summary>Sound-effects volume at 0x0363. Confirmed by the isolated sounds-slider transition.</summary>
		public byte SoundsVolume { get { return m_bytes[0x0363]; } set { m_bytes[0x0363] = value; } }

		/// <summary>Player 1 input device selector at 0x036A. Unknown raw values, including 0x01, are preserved.</summary>
		public LrInputDevice Player1InputDevice { get { return (LrInputDevice)m_bytes[0x036A]; } set { m_bytes[0x036A] = (byte)value; } }

		/// <summary>Player 2 input device selector at 0x036E. Unknown raw values, including 0x01, are preserved.</summary>
		public LrInputDevice Player2InputDevice { get { return (LrInputDevice)m_bytes[0x036E]; } set { m_bytes[0x036E] = (byte)value; } }

		/// <summary>Session state byte at 0x037F; preserve on round-trip; semantics unknown; do not modify unless reverse-engineering this field.</summary>
		public byte StateA { get { return m_bytes[0x037F]; } set { m_bytes[0x037F] = value; } }

		// Keyboard layouts are three independent slots, not per-player binding records. Values are DIK scancodes.
		public byte KB1_TurnLeft { get { return m_bytes[0x03C1]; } set { m_bytes[0x03C1] = value; } }
		public byte KB1_TurnRight { get { return m_bytes[0x03C5]; } set { m_bytes[0x03C5] = value; } }
		public byte KB1_Accelerate { get { return m_bytes[0x03C9]; } set { m_bytes[0x03C9] = value; } }
		public byte KB1_Brake { get { return m_bytes[0x03CD]; } set { m_bytes[0x03CD] = value; } }
		public byte KB1_PowerUp { get { return m_bytes[0x03D1]; } set { m_bytes[0x03D1] = value; } }
		public byte KB1_CameraView { get { return m_bytes[0x03D5]; } set { m_bytes[0x03D5] = value; } }
		public byte KB1_ToggleDisplay { get { return m_bytes[0x03D9]; } set { m_bytes[0x03D9] = value; } }
		public byte KB1_PowerSlide { get { return m_bytes[0x03DD]; } set { m_bytes[0x03DD] = value; } }
		public byte KB1_LookBackwards { get { return m_bytes[0x03E1]; } set { m_bytes[0x03E1] = value; } }
		public byte KB2_TurnLeft { get { return m_bytes[0x03E8]; } set { m_bytes[0x03E8] = value; } }
		public byte KB2_TurnRight { get { return m_bytes[0x03EC]; } set { m_bytes[0x03EC] = value; } }
		public byte KB2_Accelerate { get { return m_bytes[0x03F0]; } set { m_bytes[0x03F0] = value; } }
		public byte KB2_Brake { get { return m_bytes[0x03F4]; } set { m_bytes[0x03F4] = value; } }
		public byte KB2_PowerUp { get { return m_bytes[0x03F8]; } set { m_bytes[0x03F8] = value; } }
		public byte KB2_CameraView { get { return m_bytes[0x03FC]; } set { m_bytes[0x03FC] = value; } }
		public byte KB2_ToggleDisplay { get { return m_bytes[0x0401]; } set { m_bytes[0x0401] = value; } }
		public byte KB2_PowerSlide { get { return m_bytes[0x0405]; } set { m_bytes[0x0405] = value; } }
		public byte KB2_LookBackwards { get { return m_bytes[0x0409]; } set { m_bytes[0x0409] = value; } }
		public byte KB3_TurnLeft { get { return m_bytes[0x0410]; } set { m_bytes[0x0410] = value; } }
		public byte KB3_TurnRight { get { return m_bytes[0x0414]; } set { m_bytes[0x0414] = value; } }
		public byte KB3_Accelerate { get { return m_bytes[0x0418]; } set { m_bytes[0x0418] = value; } }
		public byte KB3_Brake { get { return m_bytes[0x041C]; } set { m_bytes[0x041C] = value; } }
		public byte KB3_PowerUp { get { return m_bytes[0x0420]; } set { m_bytes[0x0420] = value; } }
		public byte KB3_CameraView { get { return m_bytes[0x0424]; } set { m_bytes[0x0424] = value; } }
		public byte KB3_ToggleDisplay { get { return m_bytes[0x0428]; } set { m_bytes[0x0428] = value; } }
		public byte KB3_PowerSlide { get { return m_bytes[0x042C]; } set { m_bytes[0x042C] = value; } }
		public byte KB3_LookBackwards { get { return m_bytes[0x0430]; } set { m_bytes[0x0430] = value; } }
		/// <summary>Reserved/unused header padding at 0x0438-0x047F. Preserve raw bytes unchanged.</summary>
		public byte[] ReservedInputPadding { get { return CopyBytes(0x0438, 0x48); } set { WriteBytes(0x0438, value, 0x48, nameof(ReservedInputPadding)); } }

		// Partial gamepad decode: P1 location and full axis/device encoding remain unknown.
		public byte GP_P2_Accelerate { get { return m_bytes[0x037A]; } set { m_bytes[0x037A] = value; } }
		public byte GP_P2_Brake { get { return m_bytes[0x037E]; } set { m_bytes[0x037E] = value; } }
		public byte GP_P2_PowerUp { get { return m_bytes[0x0383]; } set { m_bytes[0x0383] = value; } }
		public byte GP_P2_CameraView { get { return m_bytes[0x0387]; } set { m_bytes[0x0387] = value; } }
		public byte GP_P2_ToggleDisplay { get { return m_bytes[0x038B]; } set { m_bytes[0x038B] = value; } }
		public byte GP_P2_PowerSlide { get { return m_bytes[0x038F]; } set { m_bytes[0x038F] = value; } }
		public byte GP_P2_LookBackwards { get { return m_bytes[0x0393]; } set { m_bytes[0x0393] = value; } }

		public bool GetTimeTrialCompleted(int p_blockIndex)
		{
			ValidateBlockIndex(p_blockIndex);
			int offset = p_blockIndex < 8 ? 0x0007 : 0x0008;
			int bit = p_blockIndex < 8 ? p_blockIndex : p_blockIndex - 8;
			return (m_bytes[offset] & (1 << bit)) != 0;
		}

		public bool IsBossUnlocked(BossSet p_boss)
		{
			return (BossSetUnlockFlags & (1 << (int)p_boss)) != 0;
		}

		public bool IsCircuitCleared(CircuitCleared p_circuit)
		{
			return (CircuitClearedFlags & (1 << (int)p_circuit)) != 0;
		}

		public bool IsTrialBeaten(int p_blockIndex)
		{
			return GetTimeTrialCompleted(p_blockIndex);
		}

		public static string GetBossName(BossSet p_boss)
		{
			switch (p_boss)
			{
				case BossSet.CaptainRedbeard: return "Captain Redbeard";
				case BossSet.GypsyMoth: return "Gypsy Moth";
				case BossSet.Basil: return "Basil";
				case BossSet.JohnnyThunder: return "Johnny Thunder";
				case BossSet.KingKahuka: return "King Kahuka";
				case BossSet.BaronVonBarron: return "Baron von Barron";
				case BossSet.RocketRacer: return "Rocket Racer";
				case BossSet.VeronicaVoltage: return "Veronica Voltage";
				default: throw new ArgumentOutOfRangeException(nameof(p_boss));
			}
		}

		public static string GetCircuitName(CircuitCleared p_circuit)
		{
			switch (p_circuit)
			{
				case CircuitCleared.CaptainRedbeard: return "Captain Redbeard";
				case CircuitCleared.GypsyMoth: return "Gypsy Moth";
				case CircuitCleared.Basil: return "Basil";
				case CircuitCleared.JohnnyThunder: return "Johnny Thunder";
				case CircuitCleared.KingKahuka: return "King Kahuka";
				case CircuitCleared.BaronVonBarron: return "Baron von Barron";
				default: throw new ArgumentOutOfRangeException(nameof(p_circuit));
			}
		}

		public static string GetTrackName(int p_blockIndex)
		{
			return GetTrackName((LeaderboardBlock)ValidateAndReturnBlockIndex(p_blockIndex));
		}

		public static string GetTrackName(LeaderboardBlock p_block)
		{
			switch (p_block)
			{
				case LeaderboardBlock.ImperialGrandPrix: return "Imperial Grand Prix";
				case LeaderboardBlock.DarkForestDash: return "Dark Forest Dash";
				case LeaderboardBlock.MagmaMoonMarathon: return "Magma Moon Marathon";
				case LeaderboardBlock.DesertAdventureDragway: return "Desert Adventure Dragway";
				case LeaderboardBlock.TribalIslandTrail: return "Tribal Island Trail";
				case LeaderboardBlock.RoyalKnightsRaceway: return "Royal Knights Raceway";
				case LeaderboardBlock.IcePlanetPathway: return "Ice Planet Pathway";
				case LeaderboardBlock.AmazonAdventureAlley: return "Amazon Adventure Alley";
				case LeaderboardBlock.KnightmareAthon: return "Knightmare-Athon";
				case LeaderboardBlock.PirateSkullPass: return "Pirate Skull Pass";
				case LeaderboardBlock.AdventureTempleTrail: return "Adventure Temple Trail";
				case LeaderboardBlock.AlienRallyAsteroid: return "Alien Rally Asteroid";
				default: throw new ArgumentOutOfRangeException(nameof(p_block));
			}
		}

		public static string GetTrackAbbreviation(int p_blockIndex)
		{
			return GetTrackAbbreviation((LeaderboardBlock)ValidateAndReturnBlockIndex(p_blockIndex));
		}

		public static string GetTrackAbbreviation(LeaderboardBlock p_block)
		{
			switch (p_block)
			{
				case LeaderboardBlock.ImperialGrandPrix: return "IGP";
				case LeaderboardBlock.DarkForestDash: return "DFD";
				case LeaderboardBlock.MagmaMoonMarathon: return "MMM";
				case LeaderboardBlock.DesertAdventureDragway: return "DAD";
				case LeaderboardBlock.TribalIslandTrail: return "TIT";
				case LeaderboardBlock.RoyalKnightsRaceway: return "RKR";
				case LeaderboardBlock.IcePlanetPathway: return "IPP";
				case LeaderboardBlock.AmazonAdventureAlley: return "AAA";
				case LeaderboardBlock.KnightmareAthon: return "KNA";
				case LeaderboardBlock.PirateSkullPass: return "PSP";
				case LeaderboardBlock.AdventureTempleTrail: return "ATT";
				case LeaderboardBlock.AlienRallyAsteroid: return "ARA";
				default: throw new ArgumentOutOfRangeException(nameof(p_block));
			}
		}

		public void SetTimeTrialCompleted(int p_blockIndex, bool p_value)
		{
			ValidateBlockIndex(p_blockIndex);
			int offset = p_blockIndex < 8 ? 0x0007 : 0x0008;
			int bit = p_blockIndex < 8 ? p_blockIndex : p_blockIndex - 8;
			SetFlag(offset, bit, p_value);
		}

		internal uint ReadUInt32(int p_offset)
		{
			return (uint)(m_bytes[p_offset]
				| (m_bytes[p_offset + 1] << 8)
				| (m_bytes[p_offset + 2] << 16)
				| (m_bytes[p_offset + 3] << 24));
		}

		internal void WriteUInt32(int p_offset, uint p_value)
		{
			m_bytes[p_offset] = (byte)p_value;
			m_bytes[p_offset + 1] = (byte)(p_value >> 8);
			m_bytes[p_offset + 2] = (byte)(p_value >> 16);
			m_bytes[p_offset + 3] = (byte)(p_value >> 24);
		}

		internal string ReadName(int p_offset)
		{
			StringBuilder result = new StringBuilder(14);
			for (int i = 0; i < 14; i++)
			{
				ushort value = (ushort)(m_bytes[p_offset + (i * 2)] | (m_bytes[p_offset + (i * 2) + 1] << 8));
				if (value == 0)
					break;
				result.Append((char)value);
			}
			return result.ToString();
		}

		internal void WriteName(int p_offset, string p_value)
		{
			Array.Clear(m_bytes, p_offset, 28);
			string value = p_value ?? "";
			for (int i = 0; i < value.Length && i < 14; i++)
			{
				ushort ch = value[i];
				m_bytes[p_offset + (i * 2)] = (byte)ch;
				m_bytes[p_offset + (i * 2) + 1] = (byte)(ch >> 8);
			}
		}

		private byte[] CopyBytes(int p_offset, int p_length)
		{
			byte[] result = new byte[p_length];
			Buffer.BlockCopy(m_bytes, p_offset, result, 0, p_length);
			return result;
		}

		private void WriteBytes(int p_offset, byte[] p_value, int p_length, string p_name)
		{
			if (p_value == null || p_value.Length != p_length)
				throw new ArgumentException(string.Format("{0} must contain exactly {1} bytes.", p_name, p_length), p_name);
			Buffer.BlockCopy(p_value, 0, m_bytes, p_offset, p_length);
		}

		private void SetFlag(int p_offset, int p_bit, bool p_value)
		{
			byte mask = (byte)(1 << p_bit);
			if (p_value)
				m_bytes[p_offset] |= mask;
			else
				m_bytes[p_offset] &= (byte)~mask;
		}

		private static int GetLeaderboardBlockOffset(int p_blockIndex)
		{
			ValidateBlockIndex(p_blockIndex);
			return LeaderboardOffset + (p_blockIndex * 64) + (p_blockIndex / 2);
		}

		private static int GetCircuitSeparatorOffset(int p_separatorIndex)
		{
			if (p_separatorIndex < 0 || p_separatorIndex >= CircuitSeparatorCount)
				throw new ArgumentOutOfRangeException(nameof(p_separatorIndex));
			return LeaderboardOffset + ((p_separatorIndex + 1) * 128) + p_separatorIndex;
		}

		private static void ValidateBlockIndex(int p_blockIndex)
		{
			if (p_blockIndex < 0 || p_blockIndex >= LeaderboardBlockCount)
				throw new ArgumentOutOfRangeException(nameof(p_blockIndex));
		}

		private static int ValidateAndReturnBlockIndex(int p_blockIndex)
		{
			ValidateBlockIndex(p_blockIndex);
			return p_blockIndex;
		}
	}

	public sealed class TrackLeaderboard
	{
		internal TrackLeaderboard(LRSHeader p_header, int p_blockIndex, int p_offset)
		{
			BlockIndex = p_blockIndex;
			BestLap = new LeaderboardRecord(p_header, p_offset);
			BestRace = new LeaderboardRecord(p_header, p_offset + 32);
		}

		public int BlockIndex { get; private set; }
		public LeaderboardRecord BestLap { get; private set; }
		public LeaderboardRecord BestRace { get; private set; }
	}

	public sealed class LeaderboardRecord
	{
		private readonly LRSHeader m_header;
		private readonly int m_offset;

		internal LeaderboardRecord(LRSHeader p_header, int p_offset)
		{
			m_header = p_header;
			m_offset = p_offset;
		}

		public uint TimeMilliseconds
		{
			get { return m_header.ReadUInt32(m_offset); }
			set { m_header.WriteUInt32(m_offset, value); }
		}

		public TimeSpan Time
		{
			get { return TimeSpan.FromMilliseconds(TimeMilliseconds); }
			set
			{
				if (value < TimeSpan.Zero || value.TotalMilliseconds > uint.MaxValue)
					throw new ArgumentOutOfRangeException(nameof(value));
				TimeMilliseconds = (uint)value.TotalMilliseconds;
			}
		}

		/// <summary>
		/// Leaderboard name. The block-1 name padding (including file offset 0x007F) can contain
		/// unstable uninitialized engine memory after the null terminator; do not rely on it.
		/// </summary>
		public string Name
		{
			get { return m_header.ReadName(m_offset + 4); }
			set { m_header.WriteName(m_offset + 4, value); }
		}
	}
}
