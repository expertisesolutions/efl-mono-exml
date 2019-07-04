using System;
using System.Reflection;
using System.Linq;

using ApiDump;


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
            Test.AssertEquals(default_ctor.Visibility, ApiDump.Model.Visibility.Protected);

            var param_ctor = ctors[1];
            Test.AssertEquals(param_ctor.Parameters.Count, 2);
            Test.AssertEquals(param_ctor.Visibility, ApiDump.Model.Visibility.Public);

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

            Test.AssertEquals(pubMeth.Visibility, ApiDump.Model.Visibility.Public);
        }

        public static void prot_method_visibility(API api)
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var protMeth = cls.Methods.Single(x => x.Name == "ProtectedMeth");
            Test.AssertEquals(protMeth.Visibility, ApiDump.Model.Visibility.Protected);
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
            Test.AssertEquals(clicked.Visibility, ApiDump.Model.Visibility.Public);

            var customEvt = evts[1];
            Test.AssertEquals(customEvt.Name, "CustomEvent");
            Test.AssertEquals(customEvt.Type.Name, "Dummy.CustomArgs");
            Test.AssertEquals(customEvt.Visibility, ApiDump.Model.Visibility.Protected);

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

            Test.AssertEquals(getSet.Visibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(getSet.GetVisibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(getSet.SetVisibility, ApiDump.Model.Visibility.Public);
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

            Test.AssertEquals(getOnly.Visibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(getOnly.GetVisibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(getOnly.SetVisibility, ApiDump.Model.Visibility.Other);
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

            Test.AssertEquals(setOnly.Visibility, ApiDump.Model.Visibility.Protected);
            Test.AssertEquals(setOnly.GetVisibility, ApiDump.Model.Visibility.Other);
            Test.AssertEquals(setOnly.SetVisibility, ApiDump.Model.Visibility.Protected);
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

            Test.AssertEquals(privateSet.Visibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(privateSet.GetVisibility, ApiDump.Model.Visibility.Public);
            Test.AssertEquals(privateSet.SetVisibility, ApiDump.Model.Visibility.Other);
        }
    }
}

public class TestRunner
{
    static void Main(string[] args)
    {
        // FIXME control verbosity with `meson test -v`
        /* ApiDump.Logging.Logger.AddConsoleLogger(); */
        var api = ApiDump.API.Parse(args[0]);
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
