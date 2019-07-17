using System;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Exml
{

namespace ApiDump
{

    [Serializable]
    public class API
    {
        public List<ApiModel.Class> Classes { get; private set; }
        public List<ApiModel.Function> FunctionPointers { get; private set; }
        public List<ApiModel.Enum> Enums { get; private set; }
        public List<ApiModel.Struct> Structs { get; private set; }

        private API()
        {
            Classes = new List<ApiModel.Class>();
            FunctionPointers = new List<ApiModel.Function>();
            Enums = new List<ApiModel.Enum>();
            Structs = new List<ApiModel.Struct>();
        }

        public void AddEntity(Type entity)
        {

        }

        private static bool IsBindingEntity(Type type)
        {
            var attributes = System.Attribute.GetCustomAttributes(type, false);

            foreach (var attribute in attributes)
            {
                if (attribute.GetType().ToString() == "Efl.Eo.BindingEntity")
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
                if (!IsBindingEntity(exportedType))
                {
                    continue;
                }

                switch (GetEntityKind(exportedType))
                {
                    case EntityKind.Class:
                        ret.Classes.Add(ApiModel.Class.From(exportedType));
                        break;
                    case EntityKind.Enum:
                        ret.Enums.Add(ApiModel.Enum.From(exportedType));
                        break;
                    case EntityKind.Function:
                        ret.FunctionPointers.Add(ApiModel.Function.From(exportedType));
                        break;
                    case EntityKind.Struct:
                        ret.Structs.Add(ApiModel.Struct.From(exportedType));
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

}
