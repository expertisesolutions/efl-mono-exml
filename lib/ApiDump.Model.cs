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
        InOut,
        Unkown,
    }

    public enum Visibility
    {
        Public,
        Protected,
        Other,
    }

    [Serializable]
    public class TypeRef
    {
        public string Name { get; private set; }
        public bool IsReference { get; private set; }

        public static TypeRef From(System.Type type)
        {
            var obj = new TypeRef();
            obj.Name = type.FullName;

            obj.IsReference = type.IsByRef;

            if (obj.IsReference)
            {
                obj.Name = obj.Name.Remove(obj.Name.Length - 1);
            }

            return obj;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [Serializable]
    public class Parameter
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }
        public ParameterDirection Direction { get; private set; }

        public override string ToString()
        {
            string direction;

            switch (Direction)
            {
                case ParameterDirection.Out:
                    direction = "out ";
                    break;
                case ParameterDirection.InOut:
                    direction = "ref ";
                    break;
                default:
                    direction = "";
                    break;
            }
            return $"{direction}{Type} {Name}";
        }

        public static Parameter From(ParameterInfo param)
        {
            var obj = new Parameter();

            obj.Name = param.Name;
            obj.Type = TypeRef.From(param.ParameterType);
            obj.Direction = ParseDirection(param);

            return obj;
        }

        private static ParameterDirection ParseDirection(ParameterInfo param)
        {
            if (param.IsOut)
            {
                return ParameterDirection.Out;
            }
            else if (param.ParameterType.IsByRef)
            {
                return ParameterDirection.InOut;
            }

            return ParameterDirection.In;
        }
    }

    [Serializable]
    public class Function
    {
        public string Name { get; private set; }
        public TypeRef ReturnType { get; private set; }
        public List<Parameter> Parameters { get; private set; }
        public bool IsFuncPtr { get; private set; }
        public Visibility Visibility { get; private set; }

        private static List<Parameter> ParseParameters(ParameterInfo[] parameters)
        {
            var ret = new List<Parameter>();

            ret = parameters.Select(param => Parameter.From(param)).ToList();

            return ret;
        }

        public static Function From(System.Type type)
        {
            Logger.Info($"Creating function from type: {type.FullName}");
            var obj = new Function();
            obj.Name = type.FullName;
            obj.IsFuncPtr = true;

            var methInfo = type.GetMethod("Invoke");

            obj.ReturnType = TypeRef.From(methInfo.ReturnType);

            // Function pointers are toplevel API members, so always public
            obj.Visibility = Visibility.Public;

            obj.Parameters = ParseParameters(methInfo.GetParameters());

            return obj;
        }

        public static Function From(ConstructorInfo ctor)
        {
            Logger.Info($"Creating function from constructor: {ctor.Name}");
            var obj = new Function();
            obj.Name = ctor.Name;
            obj.IsFuncPtr = false;

            obj.ReturnType = TypeRef.From(ctor.DeclaringType);

            obj.Visibility = ctor.IsFamily ? Visibility.Protected : Visibility.Public;

            obj.Parameters = ParseParameters(ctor.GetParameters());

            return obj;
        }

        public static Function From(MethodInfo method)
        {
            Logger.Info($"Creating function from method: {method.Name}");

            var obj = new Function();

            obj.Name = method.Name;
            obj.IsFuncPtr = false;

            obj.ReturnType = TypeRef.From(method.ReturnType);

            obj.Visibility = method.IsFamily ? Visibility.Protected : Visibility.Public;

            obj.Parameters = ParseParameters(method.GetParameters());

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

        internal static Visibility ParseMethodVisibility(MethodInfo meth)
        {
            if (meth == null)
            {
                return Visibility.Other;
            }

            if (meth.IsPublic)
            {
                return Visibility.Public;
            }
            else if (meth.IsFamily)
            {
                return Visibility.Protected;
            }
            else
            {
                return Visibility.Other;
            }
        }
    }

    [Serializable]
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

    [Serializable]
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

    [Serializable]
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

    [Serializable]
    public class Property
    {
        public string Name { get; private set; }
        public TypeRef Type { get; private set; }
        public bool HasGet { get; private set; }
        public bool HasSet { get; private set; }
        public bool HasGetSet { get { return HasSet && HasGet; } }
        public Visibility GetVisibility { get; private set; }
        public Visibility SetVisibility { get; private set; }
        // The property own visibility is the least restrictive visibility of their accessors
        public Visibility Visibility
        {
            get
            {
                if (GetVisibility == Visibility.Public || SetVisibility == Visibility.Public)
                {
                    return Visibility.Public;
                }
                else if (GetVisibility == Visibility.Protected || SetVisibility == Visibility.Protected)
                {
                    return Visibility.Protected;
                }
                else
                {
                    return Visibility.Other;
                }
            }
        }

        public static Property From(PropertyInfo property)
        {
            var obj = new Property();

            obj.Name = property.Name;
            obj.Type = TypeRef.From(property.PropertyType);


            obj.GetVisibility = Function.ParseMethodVisibility(property.GetMethod);
            obj.SetVisibility = Function.ParseMethodVisibility(property.SetMethod);

            obj.HasGet = property.CanRead && (obj.GetVisibility != Visibility.Other);
            obj.HasSet = property.CanWrite && (obj.SetVisibility != Visibility.Other);

            return obj;
        }

        public override string ToString()
        {
            var accessors = "";

            if (HasGet)
            {
                accessors += "get; ";
            }

            if (HasSet)
            {
                accessors += "set; ";
            }

            return $"{Visibility.ToString().ToLower()} {Type} {Name} {{ {accessors}}}";
        }
    }

    [Serializable]
    public class Event
    {
        public string Name { get; private set; }
        // Name of T in EventHandler<T>
        public TypeRef Type { get; private set; }
        public Visibility Visibility {get; private set; }

        public static Event From(EventInfo evt)
        {
            var obj = new Event();

            obj.Name = evt.Name;

            var evtHandlerType = evt.EventHandlerType;

            if (evtHandlerType != null && evtHandlerType.IsGenericType)
            {
                var genParam = evtHandlerType.GenericTypeArguments[0];
                obj.Type = TypeRef.From(genParam);
            }
            else
            {
                obj.Type = TypeRef.From(typeof(EventArgs));
            }

            // EventInfo has no visibility information itself. We need to infer from their
            // add/remove methods
            obj.Visibility = evt.AddMethod.IsFamily ? Visibility.Protected : Visibility.Public;

            return obj;
        }

        public override string ToString()
        {
            return $"{Name}: {Type}";
        }
    }

    [Serializable]
    public class Class
    {
        public string Name { get; private set; }
        public List<Function> Constructors { get; private set; }
        public List<Function> Methods { get; private set; }
        public TypeRef Parent { get; private set; }
        public List<TypeRef> Interfaces { get; private set; }
        public List<Property> Properties { get; private set; }
        public List<Event> Events { get; private set; }

        public bool IsInterface { get; private set; }
        public bool IsAbstract { get; private set; }

        private Class()
        {
            Constructors = new List<Function>();
            Methods = new List<Function>();
            Interfaces = new List<TypeRef>();
            Properties = new List<Property>();
            Events = new List<Event>();
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
            foreach(var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                obj.Constructors.Add(Function.From(ctor));
            }

            // FIXME Do we need to list static methods too?
            foreach(var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // Skip anonymous property accessors (get_Name)
                if (!method.IsSpecialName)
                {
                    obj.Methods.Add(Function.From(method));
                }
            }

            foreach(var property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                obj.Properties.Add(Property.From(property));
            }

            foreach (var evt in type.GetEvents(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                obj.Events.Add(Event.From(evt));
            }

            return obj;
        }
    }
}
}
