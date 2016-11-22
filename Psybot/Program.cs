using System;
using System.Threading;
using Psybot.UI;

namespace Psybot
{
    public static class Program
    {
        public const string VERSION = "0.1.2";

        // int.to https://msdn.microsoft.com/en-us/library/6t7dwaa5(v=vs.110).aspx

        public static void Main(string[] args)
        {
            Console.Title = "Psybot  v" + VERSION;

            try
            {
                new PsybotCore().Run();
                Term.Stop();
            }
            catch (Exception ex)
            {
                Term.Stop();
                Console.ForegroundColor = ConsoleColor.Red;
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
