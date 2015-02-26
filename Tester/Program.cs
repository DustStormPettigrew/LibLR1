using LibLR1;
using System;
using System.IO;

namespace Tester
{
	public class Program
	{
		private static int ms_testsRun = 0;
		private static int ms_testsPassed = 0;

		public static void Main(string[] p_args)
		{
			const string gameFolder = @"E:\Games\LEGO Racers";

			Test(gameFolder, "*.ADB", (path) => new ADB(path));
			Test(gameFolder, "*.BDB", (path) => new BDB(path));
			Test(gameFolder, "*.BMP", (path) => new BMP(path));
			Test(gameFolder, "*.BVB", (path) => new BVB(path));
			Test(gameFolder, "*.CCB", (path) => new CCB(path));
			Test(gameFolder, "*.CDB", (path) => new CDB(path));
			Test(gameFolder, "*.CPB", (path) => new CPB(path));
			Test(gameFolder, "*.CRB", (path) => new CRB(path));
			Test(gameFolder, "*.DDB", (path) => new DDB(path));
			Test(gameFolder, "*.GCB", (path) => new GCB(path));
			Test(gameFolder, "*.GDB", (path) => new GDB(path));
			Test(gameFolder, "*.GHB", (path) => new GHB(path));
			Test(gameFolder, "*.TIB", (path) => new TIB(path));
			Test(gameFolder, "*.TRB", (path) => new TRB(path));
			Test(gameFolder, "*.LSB", (path) => new LSB(path));
			Test(gameFolder, "*.MAB", (path) => new MAB(path));
			Test(gameFolder, "*.MDB", (path) => new MDB(path));
			Test(gameFolder, "*.MIB", (path) => new MIB(path));
			Test(gameFolder, "*.PWB", (path) => new PWB(path));
			Test(gameFolder, "*.RAB", (path) => new RAB(path));
			Test(gameFolder, "*.RCB", (path) => new RCB(path));
			Test(gameFolder, "*.RRB", (path) => new RRB(path));
			Test(gameFolder, "*.SDB", (path) => new SDB(path));
			//Test(gameFolder, "*.SKB", (path) => new SKB(path));
			Test(gameFolder, "*.TDB", (path) => new TDB(path));
			Test(gameFolder, "*.WDB", (path) => new WDB(path));
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