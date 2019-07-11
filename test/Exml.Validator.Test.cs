using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using System.Xml.Schema;

using Exml.Validator;


namespace TestSuite
{
    public class Valid_Exml
    {
        const string schema_name = "sample_schema.xsd";

        public static void valid(string test_folder)
        {
            ValidationEventHandler cb = (object sender, ValidationEventArgs e) =>
            {
                Console.WriteLine($"{e.Severity}: {e.Message}");
                Console.WriteLine($"{e.Exception.ToString()}");
            };

            string filePath = Path.Combine(test_folder, "hello_valid.xml");
            string schemaPath = Path.Combine(test_folder, schema_name);
            using (var xmlStream = File.OpenRead(filePath))
            using (var schemaStream = File.OpenRead(schemaPath))
            {
                try
                {
                    ExmlValidator.Validate(xmlStream, schemaStream, cb);
                }
                catch (XmlSchemaException e)
                {
                    Console.WriteLine($"{e.ToString()}");
                    throw e;
                }
            }

        }
    }
}

public class TestRunner
{
    static void Main(string[] args)
    {
        // FIXME control verbosity with `meson test -v`
        /* ApiDump.Logging.Logger.AddConsoleLogger(); */
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

