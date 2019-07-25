using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

using ApiModel = Exml.ApiModel;

namespace TestSuite
{

    public class ApiProvider
    {
        public static Exml.ApiModel.API Api {
            get {
                return Exml.ApiModel.API.Parse(Assembly.GetExecutingAssembly());
            }
        }
    }

    [TestFixture]
    public class EmptyClass
    {
        public EmptyClass()
        {
            this.api = ApiProvider.Api;
        }

        private Exml.ApiModel.API api;

        [Test]
        public void empty_class()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.EmptyClass");

            Assert.That(cls.Constructors.Count, Is.EqualTo(1));
            var ctor = cls.Constructors.ElementAt(0);
            Assert.That(ctor.Parameters, Is.Empty);
        }
    }

    [TestFixture]
    public class SimpleClass
    {

        public SimpleClass()
        {
            this.api = ApiProvider.Api;
        }
        private Exml.ApiModel.API api;

        [Test]
        public void constructors()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Assert.That(cls.Constructors.Count, Is.EqualTo(2));

            var ctors = cls.Constructors.OrderBy(ctor => ctor.Parameters.Count).ToList();

            var default_ctor = ctors[0];
            Assert.That(default_ctor.Parameters, Is.Empty);
            Assert.That(default_ctor.Visibility, Is.EqualTo(ApiModel.Visibility.Protected));

            var param_ctor = ctors[1];
            Assert.That(param_ctor.Parameters.Count, Is.EqualTo(2));
            Assert.That(param_ctor.Visibility, Is.EqualTo(ApiModel.Visibility.Public));

            var first_param = param_ctor.Parameters[0];
            Assert.That(first_param.Name, Is.EqualTo("x"));
            Assert.That(first_param.Type.Name, Is.EqualTo("System.Int32"));

            var second_param = param_ctor.Parameters[1];
            Assert.That(second_param.Name, Is.EqualTo("y"));
            Assert.That(second_param.Type.Name, Is.EqualTo("System.Double"));
        }

        [Test]
        public void pub_method_visibility()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var pubMeth = cls.Methods.Single(x => x.Name == "PublicMeth");

            Assert.That(pubMeth.Visibility, Is.EqualTo(ApiModel.Visibility.Public));
        }

        [Test]
        public void prot_method_visibility()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var protMeth = cls.Methods.Single(x => x.Name == "ProtectedMeth");
            Assert.That(protMeth.Visibility, Is.EqualTo(ApiModel.Visibility.Protected));
        }
    }

    [TestFixture]
    public class Events
    {
        public Events()
        {
            this.api = ApiProvider.Api;
        }
        private Exml.ApiModel.API api;

        [Test]
        public void get_all_events()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Assert.That(cls.Events.Count, Is.EqualTo(2));

            var evts = cls.Events.OrderBy(evt => evt.Name).ToList();

            var clicked = evts[0];
            Assert.That(clicked.Name, Is.EqualTo("Clicked"));
            Assert.That(clicked.Type.Name, Is.EqualTo("System.EventArgs"));
            Assert.That(clicked.Visibility, Is.EqualTo(ApiModel.Visibility.Public));

            var customEvt = evts[1];
            Assert.That(customEvt.Name, Is.EqualTo("CustomEvent"));
            Assert.That(customEvt.Type.Name, Is.EqualTo("Dummy.CustomArgs"));
            Assert.That(customEvt.Visibility, Is.EqualTo(ApiModel.Visibility.Protected));

        }
    }

    [TestFixture]
    public class Properties
    {

        public Properties()
        {
            this.api = ApiProvider.Api;
        }
        private Exml.ApiModel.API api;

        [Test]
        public void get_property_count()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            Assert.That(cls.Properties.Count, Is.EqualTo(4));
        }

        [Test]
        public void get_set_property()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var getSet = cls.Properties.Single(x => x.Name == "PropGetSet");

            Assert.That(getSet.Type.Name, Is.EqualTo("System.Int32"));
            Assert.That(getSet.HasGet, Is.True);
            Assert.That(getSet.HasSet, Is.True);
            Assert.That(getSet.HasGetSet, Is.True);
            Assert.That(getSet.ToString(), Is.EqualTo("public System.Int32 PropGetSet { get; set; }"));

            Assert.That(getSet.Visibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(getSet.GetVisibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(getSet.SetVisibility, Is.EqualTo(ApiModel.Visibility.Public));
        }

        [Test]
        public void get_only_property()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var getOnly = cls.Properties.Single(x => x.Name == "PropGetOnly");

            Assert.That(getOnly.Type.Name, Is.EqualTo("System.String"));
            Assert.That(getOnly.HasGet, Is.True);
            Assert.That(getOnly.HasSet, Is.False);
            Assert.That(getOnly.HasGetSet, Is.False);
            Assert.That(getOnly.ToString(), Is.EqualTo("public System.String PropGetOnly { get; }"));

            Assert.That(getOnly.Visibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(getOnly.GetVisibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(getOnly.SetVisibility, Is.EqualTo(ApiModel.Visibility.Other));
        }

        [Test]
        public void set_only_property()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var setOnly = cls.Properties.Single(x => x.Name == "PropSetOnly");

            Assert.That(setOnly.Type.Name, Is.EqualTo("System.Double"));
            Assert.That(setOnly.HasGet, Is.False);
            Assert.That(setOnly.HasSet, Is.True);
            Assert.That(setOnly.HasGetSet, Is.False);
            Assert.That(setOnly.ToString(), Is.EqualTo("protected System.Double PropSetOnly { set; }"));

            Assert.That(setOnly.Visibility, Is.EqualTo(ApiModel.Visibility.Protected));
            Assert.That(setOnly.GetVisibility, Is.EqualTo(ApiModel.Visibility.Other));
            Assert.That(setOnly.SetVisibility, Is.EqualTo(ApiModel.Visibility.Protected));
        }

        [Test]
        public void get_private_set_property()
        {
            var cls = api.Classes.Single(x => x.Name == "Dummy.Parent");
            var privateSet = cls.Properties.Single(x => x.Name == "PropPrivateSet");

            Assert.That(privateSet.Type.Name, Is.EqualTo("System.Int32"));
            Assert.That(privateSet.HasGet, Is.True);
            Assert.That(privateSet.HasSet, Is.False);
            Assert.That(privateSet.HasGetSet, Is.False);
            Assert.That(privateSet.ToString(), Is.EqualTo("public System.Int32 PropPrivateSet { get; }"));

            Assert.That(privateSet.Visibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(privateSet.GetVisibility, Is.EqualTo(ApiModel.Visibility.Public));
            Assert.That(privateSet.SetVisibility, Is.EqualTo(ApiModel.Visibility.Other));
        }
    }

/*     public class Serialization : IClassFixture<ApiModelFixture> */
/*     { */
/*         public Serialization(ApiModelFixture api) */
/*         { */
/*             this.api = api.Api; */
/*         } */
/*         private ApiModel.API api; */

/*         // Skipping as dotnet core has no support for serializing System.Type directly yet, as needed by */
/*         // ApiModel.TypeRef. */
/*         // https://github.com/dotnet/corefx/issues/23213 */
/*         /1* [Fact] *1/ */
/*         public void test_serialization() */
/*         { */
/*             var memory = new System.IO.MemoryStream(); */

/*             api.Serialize(memory); */

/*             memory.Position = 0; */

/*             var copy = ApiModel.API.Deserialize(memory); */

/*             Func<ApiModel.API, List<string> > GetNames = (ApiModel.API data) => { */
/*                 List<string> acc = new List<string>(); */

/*                 acc.AddRange(data.Classes.Select(x => x.Name).OrderBy(x => x).ToList()); */
/*                 acc.AddRange(data.FunctionPointers.Select(x => x.Name).OrderBy(x => x).ToList()); */
/*                 acc.AddRange(data.Enums.Select(x => x.Name).OrderBy(x => x).ToList()); */
/*                 acc.AddRange(data.Structs.Select(x => x.Name).OrderBy(x => x).ToList()); */

/*                 return acc; */
/*             }; */

/*             var originalNames = GetNames(api); */
/*             var newNames = GetNames(copy); */

/*             Assert.Equal(originalNames.Count, newNames.Count); */
/*             Test.Assert(originalNames.SequenceEqual(newNames)); */
/*         } */
/*     } */
}
