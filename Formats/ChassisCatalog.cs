using System;

namespace LibLR1
{
	/// <summary>Known LRS chassis-tag classifications shared by LRS consumers.</summary>
	public static class ChassisCatalog
	{
		/// <summary>Returns the confirmed character associated with a chassis tag, or null when not catalogued.</summary>
		public static string GetCharacterName(string p_chassisTag)
		{
			switch (p_chassisTag ?? "")
			{
				case "bvbcha0": return "Baron Von Barron";
				default: return null;
			}
		}

		/// <summary>Gets the player-facing default set number for a default chassis tag.</summary>
		public static bool TryGetDefaultSetNumber(string p_chassisTag, out byte p_setNumber)
		{
			switch (p_chassisTag ?? "")
			{
				case "ddchas0": p_setNumber = 1; return true;
				case "dbchas0": p_setNumber = 2; return true;
				case "dcchas0": p_setNumber = 3; return true;
				case "dachas0": p_setNumber = 4; return true;
				default: p_setNumber = 0; return false;
			}
		}

		/// <summary>Returns the player-facing default-set label for a known default chassis tag, or null.</summary>
		public static string GetDefaultSetLabel(string p_chassisTag)
		{
			return TryGetDefaultSetNumber(p_chassisTag, out byte setNumber)
				? "Set " + setNumber.ToString(System.Globalization.CultureInfo.InvariantCulture)
				: null;
		}
	}
}
