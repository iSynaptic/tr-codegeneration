using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage
{
    [TestFixture]
    public class ValidationTests
    {
        [Test]
        public void EmptyInput_Succedes()
        {
            AssertSuccess("");
            AssertSuccess(" ");
        }

        [Test]
        public void EmptyNamespace_Succedes()
        {
            var ns = AssertSuccess("namespace Foo { }")
                .Namespaces.TryFirst(x => x.Name == "Foo");

            Assert.IsTrue(ns.HasValue);
        }

        [Test]
        public void Annotation_OnNamespace_Fails()
        {
            AssertFailure("[SomeAnnotation] namespace Foo { }");
        }

        [Test]
        public void EmptyValue_Succedes()
        {
            var ns = AssertSuccess("namespace Foo { value Bar { } value Baz; }")
                .Namespaces.TryFirst(x => x.Name == "Foo");

            var barValue = ns.SelectMaybe(x => x.Members.OfType<ComplexValueSymbol>().TryFirst(y => y.Name == "Bar"));
            var bazValue = ns.SelectMaybe(x => x.Members.OfType<ComplexValueSymbol>().TryFirst(y => y.Name == "Baz"));

            Assert.IsTrue(barValue.HasValue);
            Assert.IsTrue(bazValue.HasValue);
        }

        [Test]
        public void Value_WithUnknownBase_Fails()
        {
            string observation = "The type or namespace 'Baz' could not be found.";
            AssertFailure("namespace Foo { value Bar : Baz; }", observation);
            AssertFailure("namespace Foo { value Bar : Baz { } }", observation);
        }

        [Test]
        public void Value_WithProperty_Succedes()
        {
            var ns = AssertSuccess("namespace Foo { value Bar { string Baz; } }")
                .Namespaces.TryFirst(x => x.Name == "Foo");

            var property = ns.SelectMaybe(x => x.Members.OfType<ComplexValueSymbol>()
                .TryFirst(y => y.Name == "Bar"))
                .SelectMaybe(x => x.Properties.TryFirst(y => y.Name == "Baz"))
                .ValueOrDefault();
            
            Assert.IsNotNull(property);
            Assert.IsTrue(property.Type.IsBuiltIn<string>());
        }

        [Test]
        public void Value_WithPropertyWithUnknownType_Fails()
        {
            AssertFailure("namespace Foo { value Bar { Blast Baz; } }", "The type or namespace 'Blast' could not be found.");
        }

        private Compilation AssertSuccess(string input)
        {
            var results = TypesLanguageCompiler.Compile(new[] { new TypesLanguageCompilerInput(input, "") }, Enumerable.Empty<Assembly>());
            
            if(!results.WasSuccessful || !results.HasValue)
                Assert.Fail("Expected compilation success.\r\n" + results.Observations.Delimit("\r\n"));

            return results.Value;
        }

        private void AssertFailure(string input, string observation = null)
        {
            var results = TypesLanguageCompiler.Compile(new[] { new TypesLanguageCompilerInput(input, "") }, Enumerable.Empty<Assembly>());

            if(results.WasSuccessful)
                Assert.Fail("Expected compilation failure.\r\n" + results.Observations.Delimit("\r\n"));
            else if(observation != null && !results.Observations.Contains(observation))
                Assert.Fail("Expected observation: {0}", observation);
            else
                Console.WriteLine("Observations:\r\n{0}", results.Observations.Delimit("\r\n"));
        }
    }
}
