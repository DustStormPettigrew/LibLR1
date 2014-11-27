using LibLR1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tester
{
	public class Program
	{
		private static int ms_testsRun = 0;
		private static int ms_testsPassed = 0;

		public static void Main(string[] p_args)
		{
			Test(@"E:\Games\LEGO Racers", "*.ADB", (path) => new ADB(path));
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
				catch
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("Fail!");
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
			Console.WriteLine("{0}/{1} tests passed!", ms_testsPassed, ms_testsRun);
			Console.ResetColor();
		}
	}
}