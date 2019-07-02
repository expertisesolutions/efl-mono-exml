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
