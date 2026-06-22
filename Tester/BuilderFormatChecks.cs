using System;
using System.IO;
using System.Linq;

namespace LibLR1.Tester
{
	internal static class BuilderFormatChecks
	{
		internal static void Run(string p_gameFolder)
		{
			string piecedb = Path.Combine(p_gameFolder, "MENUDATA", "PIECEDB");
			RoundTrip(Path.Combine(piecedb, "L_COLORS.LEB"), stream => new LColors(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "CR_CSET.LEB"), stream => new CSet(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "DP_CSET.LEB"), stream => new CSet(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "GM_CSET.LEB"), stream => new CSet(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "CRSTMGR.LEB"), stream => new CrstMgr(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "LPIECEHI.LEB"), stream => new LPiece(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "LPIECELO.LEB"), stream => new LPiece(stream), (value, output) => value.Save(output));
			RoundTrip(Path.Combine(piecedb, "CHAMPS.CCB"), stream => new Champs(stream), (value, output) => value.Save(output));

			CrstMgr registry = new CrstMgr(Path.Combine(piecedb, "CRSTMGR.LEB"));
			Assert(registry.SetTagOf("dp_cset.leg") == 0x0B, "CRSTMGR ordinal 0x0B should identify dp_cset.leg.");
			Assert(SetTagRegistry.TryGet(0x0B, out SetTagDefinition johnny) && johnny.CsetFile == "jt_cset.leg", "LRS SetTag 0x0B should identify Johnny Thunder, not CRSTMGR entry zero.");
			Assert(SetTagRegistry.TryGet(0x11, out SetTagDefinition defaultOne) && defaultOne.CsetFile == "dp_cset.leg", "LRS SetTag 0x11 should identify Default Set 1.");

			string menuData = Path.Combine(p_gameFolder, "MENUDATA");
			string[] mibFixtures = { "SPLASH.MIB", "MAINMENU.MIB", "PICKLANG.MIB", "PICKMEM.MIB", "PICKRCR.MIB", "EDITCAR.MIB", "EDITDRVR.MIB", "CARBUILD.MIB" };
			foreach (string fixture in mibFixtures)
			{
				LoadMib(Path.Combine(menuData, fixture));
			}
			Assert(new MIB(Path.Combine(menuData, "PICKRCR.MIB")).UnresolvedReferences.Contains("racer"), "PICKRCR should report its runtime-populated racer reference.");

			LPiece pieces = new LPiece(Path.Combine(piecedb, "LPIECEHI.LEB"));
			if (pieces.Names.Count != 161 || pieces.EntriesDecoded.Count != 161) throw new InvalidDataException("LPIECEHI should expose 161 entries.");
			AssertPiece(pieces.FindByBrickId(0x13, 0xEE), "L300100", 0x13, 0xEE);
			AssertPiece(pieces.FindByBrickId(0x13, 0x8E), "L300300", 0x13, 0x8E);
			AssertPiece(pieces.FindByBrickId(0x13, 0x8F), "L300400", 0x13, 0x8F);
			AssertPiece(pieces.FindByName("L300100"), "L300100", 0x13, 0xEE);
			if (pieces.EntriesDecoded.Count(e => e.Token == 0x00) != 12) throw new InvalidDataException("LPIECE should contain 12 chassis entries.");
			AssertPiece(pieces.EntriesDecoded[0], "Cylinder", 0x08, 0x00);
			if (new Champs(Path.Combine(piecedb, "CHAMPS.CCB")).Champions.Count != 20) throw new InvalidDataException("CHAMPS should expose 20 champion records.");
			Console.WriteLine("builder-format fixtures ok");
		}

		private static void RoundTrip<T>(string p_path, Func<Stream, T> p_load, Action<T, Stream> p_save)
		{
			byte[] source = File.ReadAllBytes(p_path);
			using (MemoryStream input = new MemoryStream(source, false))
			using (MemoryStream output = new MemoryStream())
			{
				p_save(p_load(input), output);
				if (!source.SequenceEqual(output.ToArray())) throw new InvalidDataException(Path.GetFileName(p_path) + " failed byte-identical round-trip.");
			}
		}

		private static void LoadMib(string p_path)
		{
			new MIB(p_path);
		}

		private static void AssertPiece(LPieceEntry? p_entry, string p_name, byte p_token, byte p_brickId)
		{
			if (!p_entry.HasValue) throw new InvalidDataException("LPIECE entry was not found: " + p_name);
			AssertPiece(p_entry.Value, p_name, p_token, p_brickId);
		}

		private static void Assert(bool p_condition, string p_message)
		{
			if (!p_condition) throw new InvalidDataException(p_message);
		}

		private static void AssertPiece(LPieceEntry p_entry, string p_name, byte p_token, byte p_brickId)
		{
			if (p_entry.Name != p_name || p_entry.Token != p_token || p_entry.BrickId != p_brickId || p_entry.OpaqueBytes == null || p_entry.OpaqueBytes.Length != 20)
				throw new InvalidDataException("Unexpected LPIECE metadata for " + p_name + ".");
		}
	}
}
