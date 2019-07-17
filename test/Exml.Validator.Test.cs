using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using Exml.Validator;
using Exml.ValidatorModel;

namespace TestSuite
{
    public class Valid_Exml
    {
        public static void valid(string test_folder)
        {
            var issues = ExmlValidator.Validate(Path.Combine(test_folder, "hello_valid.xml"));
            Test.AssertEquals(issues.Count, 0);
        }

        public static void invalid(string test_folder)
        {
            var issues = ExmlValidator.Validate(Path.Combine(test_folder, "invalid_xml.xml"));

            Test.AssertEquals(issues.Count, 1);

            var issue = issues[0];

            Test.AssertEquals(issue.Severity, ValidationIssueSeverity.CriticalError);
            Test.AssertEquals(issue.Line, 9);
            Test.AssertEquals(issue.Position, 3);
        }

    }
}

public class TestRunner
{
    static void Main(string[] args)
    {
        // FIXME control verbosity with `meson test -v`
        Exml.Logging.Logger.AddConsoleLogger();
        string test_folder = args[0];
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
                    test.Invoke(null, new object[]{test_folder});
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

