using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace ApiDump
{

    [Serializable]
    public class API
    {
        public List<Model.Class> Classes { get; private set; }
        public List<Model.Function> FunctionPointers { get; private set; }
        public List<Model.Enum> Enums { get; private set; }
        public List<Model.Struct> Structs { get; private set; }

        private API()
        {
            Classes = new List<Model.Class>();
            FunctionPointers = new List<Model.Function>();
            Enums = new List<Model.Enum>();
            Structs = new List<Model.Struct>();
        }

        public void AddEntity(Type entity)
        {

        }

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

        private enum EntityKind
        {
            Class,
            Enum,
            Function,
            Struct,
        }

        private static EntityKind GetEntityKind(Type type)
        {
            if (type.IsEnum)
            {
                return EntityKind.Enum;
            }

            if (type.IsValueType)
            {
                // Enums are also ValueTypes, so we check it first above
                return EntityKind.Struct;
            }

            if (typeof(MulticastDelegate).IsAssignableFrom(type.BaseType))
            {
                return EntityKind.Function;
            }

            return EntityKind.Class;
        }

        public static API Parse(string filename)
        {
            var assembly = Assembly.LoadFile(filename);
            var ret = new API();

            foreach (var exportedType in assembly.GetExportedTypes())
            {
                if (!IsGeneratedEntity(exportedType))
                {
                    continue;
                }

                switch (GetEntityKind(exportedType))
                {
                    case EntityKind.Class:
                        ret.Classes.Add(Model.Class.From(exportedType));
                        break;
                    case EntityKind.Enum:
                        ret.Enums.Add(Model.Enum.From(exportedType));
                        break;
                    case EntityKind.Function:
                        ret.FunctionPointers.Add(Model.Function.From(exportedType));
                        break;
                    case EntityKind.Struct:
                        ret.Structs.Add(Model.Struct.From(exportedType));
                        break;
                    default:
                        throw new ArgumentException($"Failed to get entity Type of type {exportedType.FullName}");
                }
            }

            return ret;
        }

        public void Serialize(System.IO.Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        public static API Deserialize(System.IO.Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (API)formatter.Deserialize(stream);
        }
    }
}
