using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using ApiDump.Logging;

namespace ApiDump
{
namespace Model
{
    using ClassName = String;
    using EnumField = String;

    // Notes:
    // * Name always must be the fully qualified name of the entity.

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

        public static TypeRef From(System.Type type)
        {
            var obj = new TypeRef();
            obj.Name = type.FullName;

            return obj;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Parameter
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }
        public ParameterDirection Direction { get; private set; }

        public override string ToString()
        {
            return $"{Type} {Name}"; // FIXME Add support for direction
        }
    }

    public class Function
    {
        public string Name { get; private set; }
        public TypeRef ReturnType { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public bool IsFuncPtr { get; private set; }

        private Function()
        {
            Parameters = new List<Parameter>();
        }

        public static Function From(System.Type type)
        {
            Logger.Info($"Creating function from type: {type.FullName}");
            var obj = new Function();
            obj.Name = type.FullName;
            obj.IsFuncPtr = true;

            var methInfo = type.GetMethod("Invoke");

            obj.ReturnType = TypeRef.From(methInfo.ReturnType);


            return obj;
        }

        public static Function From(ConstructorInfo ctor)
        {
            Logger.Info($"Creating function from constructor: {ctor.Name}");
            var obj = new Function();
            obj.Name = ctor.Name;
            obj.IsFuncPtr = false;

            obj.ReturnType = TypeRef.From(ctor.DeclaringType);

            return obj;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (IsFuncPtr)
            {
                sb.Append("(");
            }

            sb.Append(Name);

            if (IsFuncPtr)
            {
                sb.Append(")");
            }


            sb.Append("(");

            if (Parameters.Count > 0)
            {
                sb.Append(Parameters.Select(i => i.ToString()).Aggregate((current, next) => current + ", " + next));
            }

            sb.Append(")");

            sb.Append($" -> {ReturnType}");

            return sb.ToString();
        }
    }

    public class Enum
    {
        public string Name { get; private set; }
        public List<EnumField> Fields {get; private set; }

        private Enum()
        {
            Fields = new List<EnumField>();
        }

        public static Enum From(System.Type type)
        {
            Logger.Info($"Creating enum from type: {type.FullName}");
            var obj = new Enum();
            obj.Name = type.FullName;

            foreach(var field in type.GetFields())
            {
                // Avoid the `value__` field
                if (field.IsLiteral)
                {
                    obj.Fields.Add(field.Name);
                }
            }

            return obj;
        }
    }

    public class StructField
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }

        public static StructField From(FieldInfo field)
        {
            var ret = new StructField();
            ret.Name = field.Name;
            ret.Type = TypeRef.From(field.FieldType);

            return ret;
        }
    }

    public class Struct
    {
        public string Name { get; private set; }
        public List<StructField> Fields { get; private set; }

        private Struct()
        {
            Fields = new List<StructField>();
        }

        public static Struct From(System.Type type)
        {
            Logger.Info($"Creating struct from type: {type.FullName}");
            var obj = new Struct();
            obj.Name = type.FullName;

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach(var field in fields)
            {
                obj.Fields.Add(StructField.From(field));
            }

            return obj;
        }
    }

    public class Class
    {
        public string Name { get; private set; }
        public List<Function> Constructors { get; private set; }
        public List<Function> Methods { get; private set; }
        public TypeRef Parent { get; private set; }
        public List<TypeRef> Interfaces { get; private set; }

        public bool IsInterface { get; private set; }
        public bool IsAbstract { get; private set; }

        private Class()
        {
            Constructors = new List<Function>();
            Methods = new List<Function>();
            Interfaces = new List<TypeRef>();
        }

        public static Class From(System.Type type)
        {
            Logger.Info($"Creating class from type: {type.FullName}");
            var obj = new Class();
            obj.Name = type.FullName;

            obj.IsInterface = type.IsInterface;
            obj.IsAbstract = type.IsAbstract;

            if (type.BaseType != null)
            {
                obj.Parent = TypeRef.From(type.BaseType);
            }

            foreach(var iface in type.GetInterfaces())
            {
                obj.Interfaces.Add(TypeRef.From(iface));
            }

            // No to pass BindingFlags, already gets only the public constructors by default
            foreach(var ctor in type.GetConstructors())
            {
                obj.Constructors.Add(Function.From(ctor));
            }

            return obj;
        }
    }
}
}
