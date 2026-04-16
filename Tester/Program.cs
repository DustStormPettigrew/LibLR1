using System;
using System.IO;

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
			PrintStats();
			Console.ReadLine();
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
