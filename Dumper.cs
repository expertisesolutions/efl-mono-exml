using System;

public class Dumper
{
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Must provide a filename");
                return;
            }
            ApiDump.Logging.Logger.AddConsoleLogger();

            ApiDump.API.Parse(args[0]);
        }
}
