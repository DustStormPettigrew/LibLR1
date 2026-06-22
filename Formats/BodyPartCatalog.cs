using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace LibLR1
{
	/// <summary>Cached ordered minifig-part registries from MENUDATA\PARTDB\BODYPART.PCB.</summary>
	public sealed class BodyPartCatalog
	{
		private static readonly object s_cacheLock = new object();
		private static readonly Dictionary<string, BodyPartCatalog> s_cache = new Dictionary<string, BodyPartCatalog>(StringComparer.OrdinalIgnoreCase);

		private BodyPartCatalog(
			IReadOnlyList<BodyPartEntry> p_heads,
			IReadOnlyList<BodyPartEntry> p_hats,
			IReadOnlyList<BodyPartEntry> p_legs,
			IReadOnlyList<BodyPartEntry> p_bodies)
		{
			Heads = p_heads;
			Hats = p_hats;
			Legs = p_legs;
			Bodies = p_bodies;
		}

		public IReadOnlyList<BodyPartEntry> Heads { get; private set; }
		public IReadOnlyList<BodyPartEntry> Hats { get; private set; }
		public IReadOnlyList<BodyPartEntry> Legs { get; private set; }
		public IReadOnlyList<BodyPartEntry> Bodies { get; private set; }

		public static BodyPartCatalog LoadFromGameDirectory(string p_gameDirectory)
		{
			if (string.IsNullOrWhiteSpace(p_gameDirectory))
				throw new ArgumentException("Game directory is required.", nameof(p_gameDirectory));

			string root = Path.GetFullPath(p_gameDirectory);
			lock (s_cacheLock)
			{
				if (s_cache.TryGetValue(root, out BodyPartCatalog cached))
					return cached;

				string path = Path.Combine(root, "MENUDATA", "PARTDB", "BODYPART.PCB");
				if (!File.Exists(path))
					throw new FileNotFoundException("BODYPART.PCB was not found under the game directory.", path);

				PCB pcb = new PCB(path);
				BodyPartCatalog result = new BodyPartCatalog(
					CreateEntries(pcb.Heads),
					CreateEntries(pcb.Hats),
					CreateBodyEntries(pcb.Legs),
					CreateBodyEntries(pcb.Chests));
				s_cache[root] = result;
				return result;
			}
		}

		public (BodyPartEntry Head, BodyPartEntry Hat, BodyPartEntry Legs, BodyPartEntry Body) ResolveConfigBytes(byte[] p_configBytes)
		{
			if (p_configBytes == null || p_configBytes.Length != 4)
				throw new ArgumentException("ConfigBytes must contain exactly four bytes.", nameof(p_configBytes));

			return (
				GetEntry(Heads, p_configBytes[0]),
				GetEntry(Hats, p_configBytes[1]),
				GetEntry(Legs, p_configBytes[2]),
				GetEntry(Bodies, p_configBytes[3]));
		}

		private static IReadOnlyList<BodyPartEntry> CreateEntries(PCB_PartEntry[] p_entries)
		{
			List<BodyPartEntry> result = new List<BodyPartEntry>();
			if (p_entries != null)
			{
				for (int i = 0; i < p_entries.Length; i++)
					result.Add(new BodyPartEntry((byte)i, p_entries[i].Name, p_entries[i].SetIndex, null));
			}
			return new ReadOnlyCollection<BodyPartEntry>(result);
		}

		private static IReadOnlyList<BodyPartEntry> CreateBodyEntries(PCB_BodyEntry[] p_entries)
		{
			List<BodyPartEntry> result = new List<BodyPartEntry>();
			if (p_entries != null)
			{
				for (int i = 0; i < p_entries.Length; i++)
					result.Add(new BodyPartEntry((byte)i, p_entries[i].Name, p_entries[i].SetIndex, p_entries[i].Unknown1));
			}
			return new ReadOnlyCollection<BodyPartEntry>(result);
		}

		private static BodyPartEntry GetEntry(IReadOnlyList<BodyPartEntry> p_entries, byte p_index)
		{
			return p_index < p_entries.Count ? p_entries[p_index] : null;
		}
	}

	public sealed class BodyPartEntry
	{
		public BodyPartEntry(byte p_index, string p_assetName, int p_setIndex, int? p_unknown1)
		{
			Index = p_index;
			AssetName = p_assetName;
			SetIndex = p_setIndex;
			Unknown1 = p_unknown1;
		}

		public byte Index { get; private set; }
		public string AssetName { get; private set; }
		public int SetIndex { get; private set; }
		public int? Unknown1 { get; private set; }
	}
}
