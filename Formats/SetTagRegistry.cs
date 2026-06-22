using System;
using System.Collections.Generic;

namespace LibLR1
{
	public sealed class SetTagDefinition
	{
		public SetTagDefinition(byte p_setTag, string p_csetFile, string p_chassisTag, string p_name)
		{
			SetTag = p_setTag; CsetFile = p_csetFile; ChassisTag = p_chassisTag; Name = p_name;
		}
		public byte SetTag { get; }
		public string CsetFile { get; }
		public string ChassisTag { get; }
		public string Name { get; }
	}

	/// <summary>Complete LRS BrickPlacement.SetTag registry. The values are not CRSTMGR ordinal positions.</summary>
	public static class SetTagRegistry
	{
		private static readonly Dictionary<byte, SetTagDefinition> s_entries = new Dictionary<byte, SetTagDefinition>
		{
			{ 0x00, new SetTagDefinition(0x00, null, null, "Chassis-built-in pieces") },
			{ 0x0B, new SetTagDefinition(0x0B, "jt_cset.leg", "jtchas0", "Johnny Thunder") },
			{ 0x0C, new SetTagDefinition(0x0C, "gm_cset.leg", "gm_chas0", "Gypsy Moth") },
			{ 0x0D, new SetTagDefinition(0x0D, "bvb_cset.leg", "bvbcha0", "Baron Von Barron") },
			{ 0x0E, new SetTagDefinition(0x0E, "bb_cset.leg", "bbchas0", "Basil the Bat") },
			{ 0x0F, new SetTagDefinition(0x0F, "cr_cset.leg", "crchas0", "Captain Redbeard") },
			{ 0x10, new SetTagDefinition(0x10, "kk_cset.leg", "kkchas0", "King Kahuka") },
			{ 0x11, new SetTagDefinition(0x11, "dp_cset.leg", "ddchas0", "Default Set 1") },
			{ 0x12, new SetTagDefinition(0x12, "da_cset.leg", "dbchas0", "Default Set 2") },
			{ 0x13, new SetTagDefinition(0x13, "ds_cset.leg", "dcchas0", "Default Set 3") },
			{ 0x14, new SetTagDefinition(0x14, "dc_cset.leg", "dachas0", "Default Set 4") },
			{ 0x15, new SetTagDefinition(0x15, "rr_cset.leg", "rrchas0", "Rocket Racer") },
			{ 0x16, new SetTagDefinition(0x16, "vv_cset.leg", "vvchas0", "Veronica Voltage") }
		};

		public static IReadOnlyDictionary<byte, SetTagDefinition> Entries { get { return s_entries; } }
		public static bool TryGet(byte p_setTag, out SetTagDefinition p_definition) { return s_entries.TryGetValue(p_setTag, out p_definition); }
	}

	/// <summary>Validates a resolved piece name against a CSET and the global L_COLORS palette.</summary>
	public static class BrickColorValidator
	{
		public static bool IsValid(string p_pieceName, byte p_setTag, byte p_colorIndex, LCOLORS p_palette, CSET p_colorSet)
		{
			return p_palette != null && p_colorSet != null && SetTagRegistry.TryGet(p_setTag, out SetTagDefinition entry) && entry.CsetFile != null && p_palette.TryGetName(p_colorIndex, out string colorName) && p_colorSet.IsColorValid(p_pieceName, colorName);
		}
	}
}
