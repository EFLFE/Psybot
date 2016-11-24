using System;
using System.Threading;
using Psybot.UI;

namespace Psybot
{
    public static class Program
    {
        public const string VERSION = "0.1.4";

        public static void Main(string[] args)
        {
            Console.WriteLine("Psybot  v" + VERSION + " -=- Loading...");
            Console.Title = "Psybot  v" + VERSION + " βeta";

            try
            {
                new PsybotCore().Run();
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
            //Console.ReadLine();
        }

    }
}
