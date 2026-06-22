using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LibLR1
{
	public class BrickCatalog
	{
		private readonly Dictionary<byte, BrickSetDefinition> m_setsByTag;
		private readonly Dictionary<string, byte> m_setTagsByBrickSet;
		private readonly Dictionary<string, string> m_brickSetsByChassisTag;
		private readonly Dictionary<string, LebPieceDescriptor> m_pieceDescriptors;
		private readonly Dictionary<string, List<CsetPieceEntry>> m_csetPiecesByChassisTag;
		private readonly List<BrickCatalogEntry> m_entries;
		private readonly List<string> m_warnings;

		public IReadOnlyList<BrickCatalogEntry> Entries
		{
			get { return m_entries; }
		}

		public IReadOnlyList<string> Warnings
		{
			get { return m_warnings; }
		}

		public IReadOnlyDictionary<byte, BrickSetDefinition> SetsByTag
		{
			get { return m_setsByTag; }
		}

		private BrickCatalog(
			Dictionary<byte, BrickSetDefinition> p_setsByTag,
			Dictionary<string, byte> p_setTagsByBrickSet,
			Dictionary<string, string> p_brickSetsByChassisTag,
			Dictionary<string, LebPieceDescriptor> p_pieceDescriptors,
			Dictionary<string, List<CsetPieceEntry>> p_csetPiecesByChassisTag,
			List<BrickCatalogEntry> p_entries,
			List<string> p_warnings)
		{
			m_setsByTag = p_setsByTag;
			m_setTagsByBrickSet = p_setTagsByBrickSet;
			m_brickSetsByChassisTag = p_brickSetsByChassisTag;
			m_pieceDescriptors = p_pieceDescriptors;
			m_csetPiecesByChassisTag = p_csetPiecesByChassisTag;
			m_entries = p_entries;
			m_warnings = p_warnings;
		}

		public static BrickCatalog Load(string p_gameInstallPath)
		{
			List<string> warnings = new List<string>();
			string piecedb = Path.Combine(p_gameInstallPath ?? "", "MENUDATA", "PIECEDB");
			string common = Path.Combine(p_gameInstallPath ?? "", "GAMEDATA", "COMMON");

			Dictionary<byte, BrickSetDefinition> sets = CreateSetDefinitions();
			Dictionary<string, byte> setTagsByBrickSet = sets.Values.ToDictionary(s => s.BrickSet, s => s.SetTag, StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> brickSetsByChassisTag = LoadChassisBrickSets(Path.Combine(common, "CHASSIS.CMB"), warnings);
			Dictionary<string, LebPieceDescriptor> descriptors = LoadPieceDescriptors(piecedb, warnings);
			Dictionary<string, List<CsetPieceEntry>> csetPieces = LoadCsetPieces(piecedb, warnings);
			Dictionary<byte, HashSet<byte>> observedIds = LoadObservedBrickIds(p_gameInstallPath, warnings);

			AddKnownControlledIds(observedIds);

			List<BrickCatalogEntry> entries = BuildEntries(sets, brickSetsByChassisTag, csetPieces, descriptors, observedIds);
			return new BrickCatalog(sets, setTagsByBrickSet, brickSetsByChassisTag, descriptors, csetPieces, entries, warnings);
		}

		public BrickSetDefinition GetSet(byte p_setTag)
		{
			m_setsByTag.TryGetValue(p_setTag, out BrickSetDefinition set);
			return set;
		}

		public string GetBrickSetForChassis(string p_chassisTag)
		{
			if (string.IsNullOrWhiteSpace(p_chassisTag))
				return null;

			m_brickSetsByChassisTag.TryGetValue(p_chassisTag, out string brickSet);
			return brickSet;
		}

		public byte? GetSetTagForChassis(string p_chassisTag)
		{
			string brickSet = GetBrickSetForChassis(p_chassisTag);
			if (brickSet == null)
				return null;

			if (m_setTagsByBrickSet.TryGetValue(brickSet, out byte setTag))
				return setTag;

			return null;
		}

		public BrickCatalogEntry Find(byte p_setTag, byte p_brickId)
		{
			return m_entries.FirstOrDefault(e => e.SetTag == p_setTag && e.BrickId == p_brickId);
		}

		public IEnumerable<BrickCatalogEntry> GetEntriesForSet(byte p_setTag)
		{
			return m_entries
				.Where(e => e.SetTag == p_setTag)
				.OrderBy(e => e.IsObservedBrickId ? 0 : 1)
				.ThenBy(e => e.BrickId)
				.ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase);
		}

		public bool HasDescriptorName(string p_name)
		{
			return !string.IsNullOrWhiteSpace(p_name) && m_pieceDescriptors.ContainsKey(p_name);
		}

		private static Dictionary<byte, BrickSetDefinition> CreateSetDefinitions()
		{
			BrickSetDefinition[] sets = SetTagRegistry.Entries.Values
				.Where(entry => entry.SetTag >= 0x0B)
				.Select(entry => new BrickSetDefinition(entry.SetTag, entry.CsetFile, entry.Name, entry.ChassisTag))
				.ToArray();

			return sets.ToDictionary(s => s.SetTag);
		}

		private static Dictionary<string, string> LoadChassisBrickSets(string p_path, List<string> p_warnings)
		{
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				if (!File.Exists(p_path))
				{
					p_warnings.Add("CHASSIS.CMB not found at " + p_path);
					return result;
				}

				CMB cmb = new CMB(p_path);
				foreach (KeyValuePair<string, CMB_Chassis> pair in cmb.Chassis)
				{
					if (!string.IsNullOrWhiteSpace(pair.Value.BrickSet))
						result[pair.Key] = pair.Value.BrickSet;
				}
			}
			catch (Exception ex)
			{
				p_warnings.Add("Failed to load CHASSIS.CMB: " + ex.Message);
			}
			return result;
		}

		private static Dictionary<string, LebPieceDescriptor> LoadPieceDescriptors(string p_piecedb, List<string> p_warnings)
		{
			Dictionary<string, LebPieceDescriptor> result = new Dictionary<string, LebPieceDescriptor>(StringComparer.OrdinalIgnoreCase);
			string hiPath = Path.Combine(p_piecedb, "LPIECEHI.LEB");
			string loPath = Path.Combine(p_piecedb, "LPIECELO.LEB");
			string path = File.Exists(hiPath) ? hiPath : loPath;
			try
			{
				if (!File.Exists(path))
				{
					p_warnings.Add("LPIECEHI.LEB and LPIECELO.LEB were not found in " + p_piecedb);
					return result;
				}

				LPiece pieces = new LPiece(path);
				foreach (LPieceEntry entry in pieces.EntriesDecoded)
					result[entry.Name] = new LebPieceDescriptor(entry);
			}
			catch (Exception ex)
			{
				p_warnings.Add("Failed to load LPIECE catalog: " + ex.Message);
			}
			return result;
		}

		private static Dictionary<string, List<CsetPieceEntry>> LoadCsetPieces(string p_piecedb, List<string> p_warnings)
		{
			Dictionary<string, List<CsetPieceEntry>> result = new Dictionary<string, List<CsetPieceEntry>>(StringComparer.OrdinalIgnoreCase);
			try
			{
				if (!Directory.Exists(p_piecedb))
				{
					p_warnings.Add("PIECEDB not found at " + p_piecedb);
					return result;
				}

				foreach (string path in Directory.GetFiles(p_piecedb, "*_CSET.LEB"))
				{
					LEB leb = new LEB(path);
					if (string.IsNullOrWhiteSpace(leb.ChassisName) || leb.Pieces == null)
						continue;

					List<CsetPieceEntry> pieces = new List<CsetPieceEntry>();
					for (int i = 0; i + 1 < leb.Pieces.Length; i += 2)
						pieces.Add(new CsetPieceEntry(leb.Pieces[i], leb.Pieces[i + 1]));

					result[leb.ChassisName] = pieces;
				}
			}
			catch (Exception ex)
			{
				p_warnings.Add("Failed to load CSET rosters: " + ex.Message);
			}
			return result;
		}

		private static Dictionary<byte, HashSet<byte>> LoadObservedBrickIds(string p_gameInstallPath, List<string> p_warnings)
		{
			Dictionary<byte, HashSet<byte>> result = new Dictionary<byte, HashSet<byte>>();
			try
			{
				string menudata = Path.Combine(p_gameInstallPath ?? "", "MENUDATA");
				string[] lrsPaths =
				{
					Path.Combine(menudata, "DEFAULT.LRS"),
					Path.Combine(menudata, "QBUILD.LRS")
				};

				foreach (string path in lrsPaths)
				{
					if (!File.Exists(path))
						continue;

					LRS lrs = new LRS(path);
					foreach (LRSCar car in lrs.Cars)
					{
						if (car.Bricks == null)
							continue;

						foreach (BrickPlacement brick in car.Bricks)
						{
							if (!result.TryGetValue(brick.SetTag, out HashSet<byte> ids))
							{
								ids = new HashSet<byte>();
								result.Add(brick.SetTag, ids);
							}
							ids.Add(brick.BrickId);
						}
					}
				}
			}
			catch (Exception ex)
			{
				p_warnings.Add("Failed to scan observed LRS brick IDs: " + ex.Message);
			}
			return result;
		}

		private static void AddKnownControlledIds(Dictionary<byte, HashSet<byte>> p_observedIds)
		{
			AddObserved(p_observedIds, 0x0B, 0xFE);
			AddObserved(p_observedIds, 0x0E, 0x8F);
			AddObserved(p_observedIds, 0x0E, 0xD0);
			AddObserved(p_observedIds, 0x0F, 0xB6);
		}

		private static void AddObserved(Dictionary<byte, HashSet<byte>> p_observedIds, byte p_setTag, byte p_brickId)
		{
			if (!p_observedIds.TryGetValue(p_setTag, out HashSet<byte> ids))
			{
				ids = new HashSet<byte>();
				p_observedIds.Add(p_setTag, ids);
			}
			ids.Add(p_brickId);
		}

		private static List<BrickCatalogEntry> BuildEntries(
			Dictionary<byte, BrickSetDefinition> p_sets,
			Dictionary<string, string> p_brickSetsByChassisTag,
			Dictionary<string, List<CsetPieceEntry>> p_csetPiecesByChassisTag,
			Dictionary<string, LebPieceDescriptor> p_descriptors,
			Dictionary<byte, HashSet<byte>> p_observedIds)
		{
			List<BrickCatalogEntry> entries = new List<BrickCatalogEntry>();
			foreach (BrickSetDefinition set in p_sets.Values)
			{
				List<CsetPieceEntry> roster = GetRosterForSet(set, p_brickSetsByChassisTag, p_csetPiecesByChassisTag);
				int syntheticId = 0;
				foreach (CsetPieceEntry piece in roster)
				{
					if (entries.Any(e => e.SetTag == set.SetTag && e.Name.Equals(piece.Name, StringComparison.OrdinalIgnoreCase)))
						continue;

					p_descriptors.TryGetValue(piece.Name, out LebPieceDescriptor descriptor);
					if (descriptor != null && descriptor.Token != 0x00)
						entries.Add(new BrickCatalogEntry(set.SetTag, descriptor.BrickId, piece.Name, descriptor, true, piece.ColorName));
					else
						entries.Add(new BrickCatalogEntry(set.SetTag, (byte)syntheticId, piece.Name, descriptor, false, piece.ColorName));
					syntheticId++;
				}

				if (p_observedIds.TryGetValue(set.SetTag, out HashSet<byte> ids))
				{
					foreach (byte id in ids.OrderBy(x => x))
					{
						if (entries.Any(e => e.SetTag == set.SetTag && e.BrickId == id && e.IsObservedBrickId))
							continue;
						LebPieceDescriptor descriptor = p_descriptors.Values.FirstOrDefault(value => value.BrickId == id && value.Token != 0x00);
						string name = descriptor != null ? descriptor.Name : GetKnownName(set.SetTag, id) ?? string.Format(CultureInfo.InvariantCulture, "Brick 0x{0:X2}", id);
						entries.Add(new BrickCatalogEntry(set.SetTag, id, name, descriptor, true, null));
					}
				}
			}

			return entries
				.OrderBy(e => GetSetSortIndex(e.SetTag))
				.ThenBy(e => e.IsObservedBrickId ? 0 : 1)
				.ThenBy(e => e.BrickId)
				.ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private static List<CsetPieceEntry> GetRosterForSet(
			BrickSetDefinition p_set,
			Dictionary<string, string> p_brickSetsByChassisTag,
			Dictionary<string, List<CsetPieceEntry>> p_csetPiecesByChassisTag)
		{
			if (SetTagRegistry.TryGet(p_set.SetTag, out SetTagDefinition definition) &&
				definition.ChassisTag != null &&
				p_csetPiecesByChassisTag.TryGetValue(definition.ChassisTag, out List<CsetPieceEntry> pieces))
				return pieces;
			return new List<CsetPieceEntry>();
		}

		private static string GetKnownName(byte p_setTag, byte p_brickId)
		{
			if (p_setTag == 0x0B && p_brickId == 0xFE)
				return "sFlag";
			if (p_setTag == 0x0E && p_brickId == 0x8F)
				return "1x2 plate";
			if (p_setTag == 0x0F && p_brickId == 0xB6)
				return "Captain Redbeard brick";
			return null;
		}

		private static int GetSetSortIndex(byte p_setTag)
		{
			byte[] order = { 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
			int index = Array.IndexOf(order, p_setTag);
			return index < 0 ? 1000 + p_setTag : index;
		}
	}

	public class BrickCatalogEntry
	{
		public BrickCatalogEntry(byte p_setTag, byte p_brickId, string p_name, LebPieceDescriptor p_descriptor, bool p_isObservedBrickId, string p_rosterColorName)
		{
			SetTag = p_setTag;
			BrickId = p_brickId;
			Name = p_name;
			Descriptor = p_descriptor;
			IsObservedBrickId = p_isObservedBrickId;
			RosterColorName = p_rosterColorName;
		}

		public byte SetTag { get; }
		public byte BrickId { get; }
		public string Name { get; }
		public LebPieceDescriptor Descriptor { get; }
		public bool IsObservedBrickId { get; }
		public string RosterColorName { get; }
		public bool HasVisualDescriptor
		{
			get { return Descriptor != null; }
		}
	}

	public class BrickSetDefinition
	{
		public BrickSetDefinition(byte p_setTag, string p_brickSet, string p_label, string p_characterName)
		{
			SetTag = p_setTag;
			BrickSet = p_brickSet;
			Label = p_label;
			CharacterName = p_characterName;
		}

		public byte SetTag { get; }
		public string BrickSet { get; }
		public string Label { get; }
		public string CharacterName { get; }
	}

	public class LebPieceDescriptor
	{
		public LebPieceDescriptor(LPieceEntry p_entry)
		{
			Name = p_entry.Name;
			RawA = (p_entry.Token << 8) | p_entry.BrickId;
			RawB = unchecked((int)p_entry.VertexCount);
			RawC = unchecked((int)p_entry.FaceCount);
			RawD = unchecked((int)p_entry.GeometryOffset);
			Token = p_entry.Token;
			BrickId = p_entry.BrickId;
		}

		public string Name { get; }
		public int RawA { get; }
		public int RawB { get; }
		public int RawC { get; }
		public int RawD { get; }
		public byte Token { get; }
		public byte BrickId { get; }
	}

	public class CsetPieceEntry
	{
		public CsetPieceEntry(string p_name, string p_colorName)
		{
			Name = p_name;
			ColorName = p_colorName;
		}

		public string Name { get; }
		public string ColorName { get; }
	}
}
