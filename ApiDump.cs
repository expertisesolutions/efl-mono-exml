using System;
using System.Reflection;
using System.Collections.Generic;

namespace ApiDump
{

    using ClassName = String;

    public enum ParameterDirection
    {
        In,
        Out,
        Inout,
        Unkown,
    }

    public class TypeRef
    {
        public string Name { get; private set; }
    }

    public class Parameter
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }
        public ParameterDirection Direction { get; private set; }
    }

    public class Function
    {
        public string Name { get; private set; }
        public TypeRef ReturnType { get; private set; }
        public List<Parameter> Parameters { get; private set; }
    }

    public class Class
    {
        public string Name { get; private set; }
        public List<Function> Methods { get; private set; }
        public TypeRef Parent { get; private set; }
        public List<TypeRef> Interfaces { get; private set; }

        public bool IsInterface { get; private set; }
        public bool IsAbstract { get; private set; }

        /// <summary> Extracts API information from the given type.</summary>
        public Class(System.Type type)
        {
        }
    }

    public class API
    {
        public List<Class> Classes { get; private set; }
        public List<Function> FunctionPointers { get; private set; }
        // FIXME Structs, enums, etc...

        public void AddEntity(Type entity)
        {

        }
    }

    class ApiDumper
    {

        private static bool IsGeneratedEntity(Type type)
        {
            var attributes = System.Attribute.GetCustomAttributes(type, false);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType().ToString() == "Efl.Eo.GeneratedEntity")
                {
                    return true;
                }
            }

            return false;
        }

        public static API Parse(string filename)
        {
            var assembly = Assembly.LoadFile(filename);

            foreach (var exportedType in assembly.GetExportedTypes())
            {
                if (IsGeneratedEntity(exportedType))
                {
                    Console.WriteLine($"Got exported type: {exportedType}");
                }
            }

            return new API();
        }
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Must provide a filename");
                return;
            }

            ApiDumper.Parse(args[0]);
        }
    }
}
