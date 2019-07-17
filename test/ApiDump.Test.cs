using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

using ApiModel = Exml.ApiModel;
using Exml.ApiDump;

namespace TestSuite
{
    public class EmptyClass
    {
        public static void empty_class(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.EmptyClass");

            Test.AssertEquals(cls.Constructors.Count, 1, "Must have only the default constructor");
            var ctor = cls.Constructors.ElementAt(0);
            Test.AssertEquals(ctor.Parameters.Count, 0, "Default constructor must not have paramters");
        }
    }

    public class SimpleClass
    {
        public static void constructors(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Test.AssertEquals(cls.Constructors.Count, 2);

            var ctors = cls.Constructors.OrderBy(ctor => ctor.Parameters.Count).ToList();

            var default_ctor = ctors[0];
            Test.AssertEquals(default_ctor.Parameters.Count, 0);
            Test.AssertEquals(default_ctor.Visibility, ApiModel.Visibility.Protected);

            var param_ctor = ctors[1];
            Test.AssertEquals(param_ctor.Parameters.Count, 2);
            Test.AssertEquals(param_ctor.Visibility, ApiModel.Visibility.Public);

            var first_param = param_ctor.Parameters[0];
            Test.AssertEquals(first_param.Name, "x");
            Test.AssertEquals(first_param.Type.Name, "System.Int32");

            var second_param = param_ctor.Parameters[1];
            Test.AssertEquals(second_param.Name, "y");
            Test.AssertEquals(second_param.Type.Name, "System.Double");
        }

        public static void pub_method_visibility(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var pubMeth = cls.Methods.Single(x => x.Name == "PublicMeth");

            Test.AssertEquals(pubMeth.Visibility, ApiModel.Visibility.Public);
        }

        public static void prot_method_visibility(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var protMeth = cls.Methods.Single(x => x.Name == "ProtectedMeth");
            Test.AssertEquals(protMeth.Visibility, ApiModel.Visibility.Protected);
        }
    }

    public class Events
    {
        public static void get_all_events(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Test.AssertEquals(cls.Events.Count, 2);

            var evts = cls.Events.OrderBy(evt => evt.Name).ToList();

            var clicked = evts[0];
            Test.AssertEquals(clicked.Name, "Clicked");
            Test.AssertEquals(clicked.Type.Name, "System.EventArgs");
            Test.AssertEquals(clicked.Visibility, ApiModel.Visibility.Public);

            var customEvt = evts[1];
            Test.AssertEquals(customEvt.Name, "CustomEvent");
            Test.AssertEquals(customEvt.Type.Name, "Dummy.CustomArgs");
            Test.AssertEquals(customEvt.Visibility, ApiModel.Visibility.Protected);

        }
    }

    public class Properties
    {
        public static void get_property_count(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Test.AssertEquals(cls.Properties.Count, 4);
        }


        public static void get_set_property(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var getSet = cls.Properties.Single(x => x.Name == "PropGetSet");

            Test.AssertEquals(getSet.Type.Name, "System.Int32");
            Test.Assert(getSet.HasGet);
            Test.Assert(getSet.HasSet);
            Test.Assert(getSet.HasGetSet);
            Test.AssertEquals(getSet.ToString(), "public System.Int32 PropGetSet { get; set; }");

            Test.AssertEquals(getSet.Visibility, ApiModel.Visibility.Public);
            Test.AssertEquals(getSet.GetVisibility, ApiModel.Visibility.Public);
            Test.AssertEquals(getSet.SetVisibility, ApiModel.Visibility.Public);
        }

        public static void get_only_property(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var getOnly = cls.Properties.Single(x => x.Name == "PropGetOnly");

            Test.AssertEquals(getOnly.Type.Name, "System.String");
            Test.Assert(getOnly.HasGet);
            Test.Assert(!getOnly.HasSet);
            Test.Assert(!getOnly.HasGetSet);
            Test.AssertEquals(getOnly.ToString(), "public System.String PropGetOnly { get; }");

            Test.AssertEquals(getOnly.Visibility, ApiModel.Visibility.Public);
            Test.AssertEquals(getOnly.GetVisibility, ApiModel.Visibility.Public);
            Test.AssertEquals(getOnly.SetVisibility, ApiModel.Visibility.Other);
        }

        public static void set_only_property(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var setOnly = cls.Properties.Single(x => x.Name == "PropSetOnly");

            Test.AssertEquals(setOnly.Type.Name, "System.Double");
            Test.Assert(!setOnly.HasGet);
            Test.Assert(setOnly.HasSet);
            Test.Assert(!setOnly.HasGetSet);
            Test.AssertEquals(setOnly.ToString(), "protected System.Double PropSetOnly { set; }");

            Test.AssertEquals(setOnly.Visibility, ApiModel.Visibility.Protected);
            Test.AssertEquals(setOnly.GetVisibility, ApiModel.Visibility.Other);
            Test.AssertEquals(setOnly.SetVisibility, ApiModel.Visibility.Protected);
        }

        public static void get_private_set_property(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var privateSet = cls.Properties.Single(x => x.Name == "PropPrivateSet");

            Test.AssertEquals(privateSet.Type.Name, "System.Int32");
            Test.Assert(privateSet.HasGet);
            Test.Assert(!privateSet.HasSet);
            Test.Assert(!privateSet.HasGetSet);
            Test.AssertEquals(privateSet.ToString(), "public System.Int32 PropPrivateSet { get; }");

            Test.AssertEquals(privateSet.Visibility, ApiModel.Visibility.Public);
            Test.AssertEquals(privateSet.GetVisibility, ApiModel.Visibility.Public);
            Test.AssertEquals(privateSet.SetVisibility, ApiModel.Visibility.Other);
        }
    }

    public class Serialization
    {
        public static void test_serialization(API api)
        {
            var memory = new System.IO.MemoryStream();

            api.Serialize(memory);

            memory.Position = 0;

            var copy = API.Deserialize(memory);

            Func<API, List<string> > GetNames = (API data) => {
                List<string> acc = new List<string>();

                acc.AddRange(data.Classes.Select(x => x.Name).OrderBy(x => x).ToList());
                acc.AddRange(data.FunctionPointers.Select(x => x.Name).OrderBy(x => x).ToList());
                acc.AddRange(data.Enums.Select(x => x.Name).OrderBy(x => x).ToList());
                acc.AddRange(data.Structs.Select(x => x.Name).OrderBy(x => x).ToList());

                return acc;
            };

            var originalNames = GetNames(api);
            var newNames = GetNames(copy);

            Test.AssertEquals(originalNames.Count, newNames.Count);
            Test.Assert(originalNames.SequenceEqual(newNames));
        }
    }
}

public class TestRunner
{
    static void Main(string[] args)
    {
        // FIXME control verbosity with `meson test -v`
        /* ApiDump.Logging.Logger.AddConsoleLogger(); */
        var api = API.Parse(args[0]);
        bool failed = false;

        var tcases = from t in Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == "TestSuite"
            select t;

        foreach (var tcase in tcases)
        {
            var tcaseName = tcase.Name;

            var tests = tcase.GetMethods(BindingFlags.Public | BindingFlags.Static);

            foreach (var test in tests)
            {
                var testName = test.Name;

                Console.WriteLine($"[BEGIN   ] {tcaseName}.{testName}");
                try
                {
                    test.Invoke(null, new object[]{api});
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[    FAIL] {tcaseName}.{testName}");
                    Console.WriteLine(ex.InnerException.ToString());
                    failed = true;
                    continue;
                }

                Console.WriteLine($"[    PASS] {tcaseName}.{testName}");
            }
        }

        if (failed)
        {
            Environment.Exit(-1);
        }
    }
}
