using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmlConsole
{
    class Program
    {
        static ParserWorker worker = null;
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[0] != "" && args[1] != "")
            {
                try
                {
                    worker = new ParserWorker(args[0], args[1]);
                    worker.doWork();
                }
                catch (Exception e)
                {
                    Console.Out.WriteLine("Eroare la intializare: " + e.Message);
                }
            }
            else if (args.Length != 2)
            {
                Console.Out.WriteLine("Lipseste un parametru. Corect xmlConsole caleXML caleDBF.");
            }
        }
    }
}
