using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

using NUnit.Framework;

using Exml.Validator;
using Exml.ValidatorModel;
using ApiModel = Exml.ApiModel;

namespace TestSuite
{

    public class EflSharpProvider
    {
        public static void EnsureApiLoaded()
        {
            if (s_api != null)
            {
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ExmlTests.efl_mono.dll"))
            {
                byte[] data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                var eflAssembly = Assembly.Load(data);
                s_api = ApiModel.API.Parse(eflAssembly);
            }

            Exml.XmlModel.Widget.SetApi(s_api);
        }

        private static Exml.ApiModel.API s_api = null;
    }

    [TestFixture]
    public class ExmlValidation
    {

        public ExmlValidation()
        {
            EflSharpProvider.EnsureApiLoaded();
        }

        private List<Exml.ValidatorModel.ValidationIssue> GetIssues(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("ExmlTests." + resourceName))
            {
                return ExmlValidator.Validate(stream);
            }
        }

        [Test]
        public void valid()
        {
            var issues = GetIssues("hello_valid.xml");
            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void valid_with_nested_namespace()
        {
            var issues = GetIssues("valid_nested.xml");
            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void invalid()
        {
            var issues = GetIssues("invalid_xml.xml");

            Assert.That(issues.Count, Is.EqualTo(1));

            var issue = issues[0];

            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.CriticalError));
            Assert.That(issue.Line, Is.EqualTo(6));
            Assert.That(issue.Position, Is.EqualTo(3));
        }

        [Test]
        public void unkown_tag()
        {
            var issues = GetIssues("unknown_widget.xml");

            Assert.That(issues.Count, Is.EqualTo(1));
            var issue = issues[0];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(4));
            Assert.That(issue.Position, Is.EqualTo(10));
            Assert.That(issue.Message, Is.EqualTo("Unknown type MyUnknownButton"));
        }

        [Test]
        public void invalid_container()
        {
            var issues = GetIssues("invalid_container.xml");

            Assert.That(issues.Count, Is.EqualTo(2));
            var issue = issues[0];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(4));
            Assert.That(issue.Position, Is.EqualTo(10));
            Assert.That(issue.Message, Is.EqualTo("Type Button is not a container"));

            issue = issues[1];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(5));
            Assert.That(issue.Position, Is.EqualTo(10));
            Assert.That(issue.Message, Is.EqualTo("Type Button is not a container"));
        }

        [Test]
        public void non_existent_properties()
        {
            var issues = GetIssues("non_existent_members.xml");

            Assert.That(issues.Count, Is.EqualTo(2));

            // Property issue
            var issue = issues[0];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(5));
            Assert.That(issue.Position, Is.EqualTo(17));
            Assert.That(issue.Message, Is.EqualTo("Property \"nontext\" does not exist in \"Button\""));

            // Event issue
            issue = issues[1];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(5));
            Assert.That(issue.Position, Is.EqualTo(38));
            Assert.That(issue.Message, Is.EqualTo("Event \"wtfEvt\" does not exist in \"Button\""));
        }

        [Test]
        public void read_only_property()
        {
            var issues = GetIssues("read_only_property.xml");

            Assert.That(issues.Count, Is.EqualTo(1));

            var issue = issues[0];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(4));
            Assert.That(issue.Position, Is.EqualTo(17));
            Assert.That(issue.Message, Is.EqualTo("Property \"invalidated\" is not writeable"));

        }

        [Test]
        public void property_invalid_value_format()
        {
            var issues = GetIssues("invalid_property_value.xml");

            Assert.That(issues.Count, Is.EqualTo(2));

            var issue = issues[0];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(8));
            Assert.That(issue.Position, Is.EqualTo(15));
            Assert.That(issue.Message, Is.EqualTo("\"Hello\" is not a valid value for property \"efl:rangeValue\" of type \"System.Double\""));

            issue = issues[1];
            Assert.That(issue.Severity, Is.EqualTo(ValidationIssueSeverity.Error));
            Assert.That(issue.Line, Is.EqualTo(9));
            Assert.That(issue.Position, Is.EqualTo(15));
            Assert.That(issue.Message, Is.EqualTo("\"3.3.3\" is not a valid value for property \"efl:rangeValue\" of type \"System.Double\""));
        }
    }
}

