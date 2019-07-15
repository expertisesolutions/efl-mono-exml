using System;
using System.IO;

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

        var filename = Path.GetFileName(args[0]);
        filename += ".api";
        using (var writer = File.Create(filename))
        {
            api.Serialize(writer);
        }

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
                file.WriteLine($"  - IsInterface: {item.IsInterface}");
                file.WriteLine($"  - IsAbstract: {item.IsAbstract}");
                file.WriteLine($"  - Parent: {item.Parent}");

                file.WriteLine($"  - Interfaces:");
                foreach (var iface in item.Interfaces)
                {
                    file.WriteLine($"    - {iface}");
                }

                file.WriteLine($"  - Constructors:");
                foreach (var ctor in item.Constructors)
                {
                    file.WriteLine($"    - {ctor}");
                }

                file.WriteLine($"  - Methods:");
                foreach (var method in item.Methods)
                {
                    file.WriteLine($"    - {method}");
                }

                file.WriteLine($"  - Properties:");
                foreach (var property in item.Properties)
                {
                    file.WriteLine($"    - {property}");
                }

                file.WriteLine($"  - Events");
                foreach (var evt in item.Events)
                {
                    file.WriteLine($"    - {evt}");
                }
            }
            file.WriteLine("--- # Enums");
            foreach (var item in api.Enums)
            {
                file.WriteLine($"- {item.Name}");
                foreach (var field in item.Fields)
                {
                    file.WriteLine($"  - {field}");
                }
            }
            file.WriteLine("--- # Structs");
            foreach (var item in api.Structs)
            {
                file.WriteLine($"- {item.Name}");
                foreach (var field in item.Fields)
                {
                    file.WriteLine($"  - Field: {field.Name} : {field.Type}");
                }
            }
            file.WriteLine("--- # FunctionPointers");
            foreach (var item in api.FunctionPointers)
            {
                file.WriteLine($"- {item}");
            }
        }
    }
}
