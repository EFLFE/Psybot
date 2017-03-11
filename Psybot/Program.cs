using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Psybot.Modules;
using Psybot.UI;

namespace Psybot
{
	public static class Program
	{
		public const string VERSION = "0.3.0";

		private static PsybotCore core;

		public static void Main(string[] args)
		{
			Console.WriteLine("Psybot v" + VERSION + " Loading...");
			Console.Title = "Psybot  v" + VERSION;

			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			try
			{
				core = new PsybotCore();
				core.Run();
				Term.Stop();
			}
			catch (Exception ex)
			{
				Term.Stop();
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Psybot destroyed!");
				Console.WriteLine(ex.ToString());
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.ReadLine();
			}

			Thread.Sleep(333);

			Console.WriteLine("\nterminated");
			Console.ForegroundColor = ConsoleColor.Gray;
			//Console.ReadLine();
		}

		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var dllFileName = args.Name.Remove(args.Name.IndexOf(',')) + ".dll";
			var dllPah = Path.Combine(Environment.CurrentDirectory, ModuleManager.DEFAULT_MODULE_PATH, dllFileName);

			core.SendLog("Resolve: " + dllFileName, ConsoleColor.Yellow);

			if (File.Exists(dllPah))
				return Assembly.Load(File.ReadAllBytes(dllPah));
			else
				core.SendLog("Assembly not found", ConsoleColor.Red);
			return null;
		}
	}
}
