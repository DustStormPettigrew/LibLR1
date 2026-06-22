using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibLR1.Tester
{
	public class Program
	{
		private static int ms_testsRun = 0;
		private static int ms_testsPassed = 0;

		public static void Main(string[] p_args)
		{
			string gameFolder;
			string configurationError;
			if (!TesterConfiguration.TryGetInstallationPath(out gameFolder, out configurationError))
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine(configurationError);
				Console.ResetColor();
				return;
			}
			if (p_args.Contains("--builder-formats"))
			{
				BuilderFormatChecks.Run(gameFolder);
				return;
			}

			Test(gameFolder, "*.ADB", (path) => new LibLR1.ADB(path));
			Test(gameFolder, "*.BDB", (path) => new LibLR1.BDB(path));
			Test(gameFolder, "*.BMP", (path) => new LibLR1.BMP(path));
			Test(gameFolder, "*.BVB", (path) => new LibLR1.BVB(path));
			Test(gameFolder, "*.CCB", (path) => new LibLR1.CCB(path));
			Test(gameFolder, "*.CDB", (path) => new LibLR1.CDB(path));
			Test(gameFolder, "*.CPB", (path) => new LibLR1.CPB(path));
			Test(gameFolder, "*.CRB", (path) => new LibLR1.CRB(path));
			Test(gameFolder, "*.DDB", (path) => new LibLR1.DDB(path));
			Test(gameFolder, "*.GCB", (path) => new LibLR1.GCB(path));
			Test(gameFolder, "*.GDB", (path) => new LibLR1.GDB(path));
			Test(gameFolder, "*.GHB", (path) => new LibLR1.GHB(path));
			Test(gameFolder, "*.TIB", (path) => new LibLR1.TIB(path));
			Test(gameFolder, "*.TRB", (path) => new LibLR1.TRB(path));
			Test(gameFolder, "*.LSB", (path) => new LibLR1.LSB(path));
			Test(gameFolder, "*.MAB", (path) => new LibLR1.MAB(path));
			Test(gameFolder, "*.MDB", (path) => new LibLR1.MDB(path));
			Test(gameFolder, "*.MIB", (path) => new LibLR1.MIB(path));
			Test(gameFolder, "*.PWB", (path) => new LibLR1.PWB(path));
			Test(gameFolder, "*.RAB", (path) => new LibLR1.RAB(path));
			Test(gameFolder, "*.RCB", (path) => new LibLR1.RCB(path));
			Test(gameFolder, "*.RRB", (path) => new LibLR1.RRB(path));
			Test(gameFolder, "*.SDB", (path) => new LibLR1.SDB(path));
			Test(gameFolder, "*.SKB", (path) => new LibLR1.SKB(path));
			Test(gameFolder, "*.TDB", (path) => new LibLR1.TDB(path));
			Test(gameFolder, "*.WDB", (path) => new LibLR1.WDB(path));
			Test(gameFolder, "*.CEB", (path) => new LibLR1.CEB(path));
			Test(gameFolder, "*.CMB", (path) => new LibLR1.CMB(path));
			Test(gameFolder, "*.EMB", (path) => new LibLR1.EMB(path));
			Test(gameFolder, "*.EVB", (path) => new LibLR1.EVB(path));
			Test(gameFolder, "*.FDB", (path) => new LibLR1.FDB(path));
			Test(gameFolder, "*.HZB", (path) => new LibLR1.HZB(path));
			Test(gameFolder, "*.IDB", (path) => new LibLR1.IDB(path));
			Test(gameFolder, "*.LEB", (path) => new LibLR1.LEB(path));
			Test(gameFolder, "*.MSB", (path) => new LibLR1.MSB(path));
			Test(gameFolder, "*.PCB", (path) => new LibLR1.PCB(path));
			Test(gameFolder, "*.SPB", (path) => new LibLR1.SPB(path));
			Test(gameFolder, "*.TGB", (path) => new LibLR1.TGB(path));
			TestLRS(gameFolder);
			PrintStats();
			Console.ReadLine();
		}

		private static void TestLRS(string p_gameFolder)
		{
			Console.WriteLine("\n--- LRS round-trip and content tests ---");

			string defaultLrs = Path.Combine(p_gameFolder, "MENUDATA", "DEFAULT.LRS");
			string qbuildLrs  = Path.Combine(p_gameFolder, "MENUDATA", "QBUILD.LRS");
			string legoRac1   = Path.Combine(p_gameFolder, "Save", "0", "LEGORac1");
			string legoRac15  = Path.Combine(p_gameFolder, "Save", "15", "LEGORac1");
			string legoRac6   = FindReferenceFile(p_gameFolder,
				Path.Combine("docs", "discovery", "6_LEGORac1"),
				Path.Combine("docs", "discovery", "6_LEGORac1.LRS"),
				Path.Combine("docs", "discovery", "lrs", "6_LEGORac1"),
				Path.Combine("Save", "6", "LEGORac1"));
			string legoRac7   = FindReferenceFile(p_gameFolder,
				Path.Combine("docs", "discovery", "7_LEGORac1"),
				Path.Combine("docs", "discovery", "7_LEGORac1.LRS"),
				Path.Combine("docs", "discovery", "lrs", "7_LEGORac1"),
				Path.Combine("Save", "7", "LEGORac1"));

			// Round-trip: Write(Read(bytes)) == original bytes
			RoundTrip(defaultLrs);
			RoundTrip(qbuildLrs);
			RoundTrip(legoRac1);
			RoundTrip(legoRac15);
			RoundTrip("6_LEGORac1", legoRac6);
			RoundTrip("7_LEGORac1", legoRac7);

			ContentAssert("BrickPlacement packed fields", () =>
			{
				BrickPlacement brick = BrickPlacement.Read(new byte[] { 0x14, 0x96, 0xFA, 0x07, 0x2F, 0x03, 0x09, 0x16 });
				Assert(brick.BrickTypeToken == 0x14 && brick.PositionB == 0x07, "Token and 3-bit PositionB should deserialize.");
				Assert(brick.Rotation == 3 && brick.Variant == 11 && brick.Color == 3 && brick.ColorSecondary == 9 && brick.SetTag == 0x16,
					"Packed Rotation/Variant and both colors should deserialize.");
				byte[] bytes = BrickBytes(brick);
				Assert(bytes.SequenceEqual(new byte[] { 0x14, 0x96, 0xFA, 0x07, 0x2F, 0x03, 0x09, 0x16 }), "Packed placement should round-trip byte-identically.");
			});

			// Content — DEFAULT.LRS: 14 records, names in order
			string[] expectedDefaultNames = new string[]
			{
				"JOAN OF KART", "TURBO CHARGER", "SCOOTER",
				"ROBORACER", "ROBORACER", "ROBORACER", "ROBORACER",
				"ROBORACER", "ROBORACER", "ROBORACER", "ROBORACER",
				"KEN EIVEL", "WAGON DRAGON", "WAGON DRAGON"
			};
			ContentAssert("DEFAULT.LRS record count", () =>
			{
				LRS f = new LRS(defaultLrs);
				Assert(f.Cars.Count == 14, string.Format("Expected 14 records, got {0}", f.Cars.Count));
				for (int i = 0; i < expectedDefaultNames.Length; i++)
				{
					Assert(
						f.Cars[i].Name == expectedDefaultNames[i],
						string.Format("Record {0}: expected \"{1}\", got \"{2}\"", i, expectedDefaultNames[i], f.Cars[i].Name));
				}
			});

			// Content — QBUILD.LRS: 25 records, 8 non-PLAYER chassis tags, 17 PLAYER slots, last = empty terminator
			ContentAssert("QBUILD.LRS record count and chassis distribution", () =>
			{
				LRS f = new LRS(qbuildLrs);
				Assert(f.Cars.Count == 25, string.Format("Expected 25 records, got {0}", f.Cars.Count));
				int playerCount    = f.Cars.Count(c => c.Name == "PLAYER");
				int nonPlayerCount = f.Cars.Count(c => c.Name != "PLAYER");
				Assert(nonPlayerCount == 8,  string.Format("Expected 8 non-PLAYER records, got {0}", nonPlayerCount));
				Assert(playerCount    == 17, string.Format("Expected 17 PLAYER records, got {0}", playerCount));
				// Last record is the empty terminator slot (RecordId == 0x80)
				Assert(f.Cars[24].RecordIdRaw == 0x80,
					string.Format("Expected last record RecordId=0x80, got 0x{0:X2}", f.Cars[24].RecordIdRaw));
			});

			// Content — LEGORac1: at least 1 record with non-empty name and chassis tag ending in "chas0"
			ContentAssert("LEGORac1 content", () =>
			{
				LRS f = new LRS(legoRac1);
				Assert(f.Cars.Count >= 1, "Expected at least 1 record in LEGORac1");
				bool found = false;
				foreach (LRSCar car in f.Cars)
				{
					if (!string.IsNullOrEmpty(car.Name) && car.ChassisTag.EndsWith("chas0"))
					{
						found = true;
						break;
					}
				}
				Assert(found, "No record with non-empty name and chassis tag matching *chas0");
			});

			ContentAssertIfFileExists("7_LEGORac1 rotation and set tags", legoRac7, () =>
			{
				LRS f = new LRS(legoRac7);
				Assert(f.Cars.Count == 8, string.Format("Expected 8 records, got {0}", f.Cars.Count));
				for (int i = 0; i < 8; i++)
				{
					string expectedName = i == 0 ? "LRTEST" : "LRTEST" + i.ToString();
					Assert(f.Cars[i].Name == expectedName,
						string.Format("Record {0}: expected \"{1}\", got \"{2}\"", i, expectedName, f.Cars[i].Name));
				}

				BrickPlacement baseline = null;
				for (int i = 3; i <= 6; i++)
				{
					LRSCar car = f.Cars[i];
					Assert(car.BrickCount == 2,
						string.Format("{0}: expected BrickCount=2, got {1}", car.Name, car.BrickCount));
					Assert(car.Bricks.Count == 1,
						string.Format("{0}: expected 1 stored BrickPlacement, got {1}", car.Name, car.Bricks.Count));
					byte expectedRotation = (byte)(i - 3);
					Assert(car.Bricks[0].Rotation == expectedRotation,
						string.Format("{0}: expected Rotation={1}, got {2}", car.Name, expectedRotation, car.Bricks[0].Rotation));
					if (baseline == null)
					{
						baseline = car.Bricks[0];
					}
					else
					{
						Assert(BricksEqualExcept(baseline, car.Bricks[0], 4),
							string.Format("{0}: expected only Rotation byte to differ from LRTEST3", car.Name));
					}
				}

				LRSCar setTagCar = f.Cars[7];
				Assert(setTagCar.Bricks.Count == 4,
					string.Format("LRTEST7: expected 4 stored BrickPlacements, got {0}", setTagCar.Bricks.Count));
				byte[] setTags = setTagCar.Bricks.Select(b => b.SetTag).OrderBy(b => b).ToArray();
				byte[] expectedSetTags = new byte[] { 0x11, 0x12, 0x13, 0x14 };
				Assert(setTags.SequenceEqual(expectedSetTags),
					string.Format("LRTEST7: expected set tags 11 12 13 14, got {0}", HexBytes(setTags)));
			});

			ContentAssertIfFileExists("6_LEGORac1 placement deltas", legoRac6, () =>
			{
				LRS f = new LRS(legoRac6);
				LRSCar lrtest1 = FindCarByName(f, "LRTEST1");
				LRSCar lrtest2 = FindCarByName(f, "LRTEST2");
				Assert(lrtest1.Bricks.Count == 2,
					string.Format("LRTEST1: expected 2 stored BrickPlacements, got {0}", lrtest1.Bricks.Count));
				Assert(lrtest2.Bricks.Count == 2,
					string.Format("LRTEST2: expected 2 stored BrickPlacements, got {0}", lrtest2.Bricks.Count));
				Assert(BricksEqual(lrtest1.Bricks[0], lrtest1.Bricks[1]),
					"LRTEST1: expected stacked duplicate placements to be byte-identical");
				Assert(BricksEqualExcept(lrtest2.Bricks[0], lrtest2.Bricks[1], 2),
					"LRTEST2: expected placements to differ only in PositionA");
				Assert(Math.Abs((int)lrtest2.Bricks[0].PositionA - lrtest2.Bricks[1].PositionA) == 1,
					"LRTEST2: expected PositionA to differ by exactly one unit");
			});
		}

		private static void RoundTrip(string p_filepath)
		{
			RoundTrip(p_filepath, p_filepath);
		}

		private static void RoundTrip(string p_name, string p_filepath)
		{
			ms_testsRun++;
			Console.WriteLine("Round-trip: " + p_name);
			if (string.IsNullOrEmpty(p_filepath) || !File.Exists(p_filepath))
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("  SKIP (file not found)");
				Console.ResetColor();
				ms_testsPassed++;
				return;
			}
			Console.WriteLine("  " + p_filepath);
			try
			{
				byte[] original = File.ReadAllBytes(p_filepath);
				LRS lrs;
				using (MemoryStream ms = new MemoryStream(original, writable: false))
					lrs = new LRS(ms);
				byte[] roundTripped;
				using (MemoryStream ms = new MemoryStream())
				{
					lrs.Write(ms);
					roundTripped = ms.ToArray();
				}
				if (original.Length != roundTripped.Length)
					throw new Exception(string.Format("Length mismatch: {0} vs {1}", original.Length, roundTripped.Length));
				for (int i = 0; i < original.Length; i++)
				{
					if (original[i] != roundTripped[i])
						throw new Exception(string.Format("Byte mismatch at offset 0x{0:X}: expected 0x{1:X2}, got 0x{2:X2}", i, original[i], roundTripped[i]));
				}
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("  Success!");
				ms_testsPassed++;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("  Fail! " + ex.Message);
			}
			Console.ResetColor();
		}

		private static void ContentAssert(string p_name, Action p_assertion)
		{
			ms_testsRun++;
			Console.WriteLine("Content: " + p_name);
			try
			{
				p_assertion();
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine("  Success!");
				ms_testsPassed++;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("  Fail! " + ex.Message);
			}
			Console.ResetColor();
		}

		private static void ContentAssertIfFileExists(string p_name, string p_filepath, Action p_assertion)
		{
			if (string.IsNullOrEmpty(p_filepath) || !File.Exists(p_filepath))
			{
				ms_testsRun++;
				Console.WriteLine("Content: " + p_name);
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("  SKIP (file not found)");
				Console.ResetColor();
				ms_testsPassed++;
				return;
			}

			ContentAssert(p_name, p_assertion);
		}

		private static void Assert(bool p_condition, string p_message)
		{
			if (!p_condition)
				throw new Exception(p_message);
		}

		private static LRSCar FindCarByName(LRS p_lrs, string p_name)
		{
			LRSCar car = p_lrs.Cars.FirstOrDefault(c => c.Name == p_name);
			Assert(car != null, "Expected record named " + p_name);
			return car;
		}

		private static bool BricksEqual(BrickPlacement p_a, BrickPlacement p_b)
		{
			return BrickBytes(p_a).SequenceEqual(BrickBytes(p_b));
		}

		private static bool BricksEqualExcept(BrickPlacement p_a, BrickPlacement p_b, int p_ignoredByteIndex)
		{
			byte[] a = BrickBytes(p_a);
			byte[] b = BrickBytes(p_b);
			for (int i = 0; i < a.Length; i++)
			{
				if (i == p_ignoredByteIndex)
				{
					continue;
				}
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}

		private static byte[] BrickBytes(BrickPlacement p_brick)
		{
			byte[] bytes = new byte[BrickPlacement.Size];
			p_brick.Write(bytes);
			return bytes;
		}

		private static string HexBytes(byte[] p_bytes)
		{
			return string.Join(" ", p_bytes.Select(b => b.ToString("X2")));
		}

		private static string FindReferenceFile(string p_gameFolder, params string[] p_candidates)
		{
			foreach (string candidate in BuildReferenceCandidates(p_gameFolder, p_candidates))
			{
				if (File.Exists(candidate))
				{
					return Path.GetFullPath(candidate);
				}
			}
			return null;
		}

		private static IEnumerable<string> BuildReferenceCandidates(string p_gameFolder, string[] p_candidates)
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string testerRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
			string libRoot = Path.GetFullPath(Path.Combine(testerRoot, ".."));
			string workspaceRoot = Path.GetFullPath(Path.Combine(libRoot, ".."));
			string[] roots = new string[]
			{
				p_gameFolder,
				Directory.GetCurrentDirectory(),
				baseDirectory,
				testerRoot,
				libRoot,
				workspaceRoot
			};

			foreach (string candidate in p_candidates)
			{
				if (Path.IsPathRooted(candidate))
				{
					yield return candidate;
					continue;
				}

				foreach (string root in roots)
				{
					if (!string.IsNullOrEmpty(root))
					{
						yield return Path.Combine(root, candidate);
					}
				}
			}
		}

		private static void Test<T>(string p_folder, string p_extension, Func<string, T> p_constructor)
		{
			foreach (string filepath in Directory.GetFiles(p_folder, p_extension, SearchOption.AllDirectories))
			{
				ms_testsRun++;

				Console.WriteLine(filepath);
				try
				{
					T t = p_constructor(filepath);
					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine("Success!");
					ms_testsPassed++;
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Fail!");
					Console.WriteLine(ex.Message);
				}

				Console.ResetColor();
			}
		}

		private static void PrintStats()
		{
			if (ms_testsPassed == ms_testsRun)
			{
				Console.ForegroundColor = ConsoleColor.Green;
			}
			else if (ms_testsPassed == 0)
			{
				Console.ForegroundColor = ConsoleColor.Red;
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
			}

			Console.WriteLine("{0}/{1} ({2:0.0%}) tests passed!", ms_testsPassed, ms_testsRun, (float)ms_testsPassed / ms_testsRun);
			Console.ResetColor();
		}
	}
}
