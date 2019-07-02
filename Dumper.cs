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

        var api = ApiDump.API.Parse(args[0]);

        Dump("api.yaml", api);
    }

    public static void Dump(string filename, ApiDump.API api)
    {
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filename))
        {
            file.WriteLine("--- # Classes");
            foreach (var item in api.Classes)
            {
                file.WriteLine($"- {item.Name}");
            }
            file.WriteLine("--- # Enums");
            foreach (var item in api.Enums)
            {
                file.WriteLine($"- {item.Name}");
            }
            file.WriteLine("--- # Structs");
            foreach (var item in api.Structs)
            {
                file.WriteLine($"- {item.Name}");
            }
            file.WriteLine("--- # FunctionPointers");
            foreach (var item in api.FunctionPointers)
            {
                file.WriteLine($"- {item.Name}");
            }
        }
    }
}
