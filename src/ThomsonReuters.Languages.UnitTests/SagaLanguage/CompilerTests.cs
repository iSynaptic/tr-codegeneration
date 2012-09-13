using System.IO;
using System.Reflection;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using NUnit.Framework;

namespace ThomsonReuters.Languages.SagaLanguage
{
    // Resharper disable InconsistentNaming
    [TestFixture]
    public class CompilerTests
    {
        private Result<SagaDescription, string> _compilation;

        #region Setup

        [TestFixtureSetUp]
        public void BeforeAllTests()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string pf;
            using (var stream = assembly.GetManifestResourceStream("ThomsonReuters.Languages.SagaLanguage.Input.ExtractionDef.saga"))
            using (var reader = new StreamReader(stream))
                pf = reader.ReadToEnd();

            _compilation = SagaLanguageCompiler.Compile(pf);
        }
/*
        static Outcome<string> Compare(PublicFormSyntax actual, PublicFormSyntax expected)
        {
            var outcome = Outcome.Success()
                .FailIf(actual.Name != expected.Name,
                        () => string.Format("Public form name '{0}' was expected to be '{1}'", actual.Name, expected.Name));
            foreach (var extra in actual.Templates.Values.Except(expected.Templates.Values,
                new Func<TemplateSyntax, string>(x => x.Code).ToEqualityComparer()))
                outcome = outcome.FailIf(true, string.Format("Template code '{0}' was not expected", extra.Code));
            foreach (var missing in expected.Templates.Values.Except(actual.Templates.Values,
                new Func<TemplateSyntax, string>(x => x.Code).ToEqualityComparer()))
                outcome = outcome.FailIf(true, string.Format("Template code '{0}' was expected but not found", missing.Code));
            foreach (var p in actual.Templates.Values.Join(expected.Templates.Values, x => x.Code, x => x.Code, (a, e) => new { a, e }))
                outcome = outcome.Combine(Compare(p.a, p.e));
            return outcome;
        }

        static Outcome<string> Compare(TemplateSyntax actual, TemplateSyntax expected)
        {
            var outcome = Outcome.Success()
                .FailIf(actual.Code != expected.Code,
                        () => string.Format("Actual template code '{0}' was expected to be '{1}'", actual.Code, expected.Code))
                .FailIf(actual.Name != expected.Name,
                        () => string.Format("Template '{0}' name '{1}' was expected to be '{2}'", expected.Code, actual.Name, expected.Name))
                .FailIf(actual.IsInternal != expected.IsInternal,
                () => string.Format("Template '{0}' was '{1}' but was expected to be '{2}'", expected.Code,
                    actual.IsInternal ? "internal" : "not internal",
                    expected.IsInternal ? "internal" : "not internal"));

            foreach (var extra in actual.Products.Values.Except(expected.Products.Values,
                new Func<ProductSyntax, string>(x => x.Name).ToEqualityComparer()))
                outcome = outcome.FailIf(true, string.Format("Product '{0}' was not expected", extra.Name));
            foreach (var missing in expected.Products.Values.Except(actual.Products.Values,
                new Func<ProductSyntax, string>(x => x.Name).ToEqualityComparer()))
                outcome = outcome.FailIf(true, string.Format("Product '{0}' was expected but not found", missing.Name));
            foreach (var p in actual.Products.Values.Join(expected.Products.Values, x => x.Name, x => x.Name, (a, e) => new { a, e }))
                outcome = outcome.Combine(Compare(p.a, p.e));
            return outcome;
        }

        static Outcome<string> Compare(ProductSyntax actual, ProductSyntax expected)
        {
            return Outcome.Success()
                .FailIf(actual.Name != expected.Name,
                        () => string.Format("Actual product name '{0}' was expected to be '{1}'", actual.Name, expected.Name))
                .FailIf(actual.Description != expected.Description,
                        () => string.Format("Product '{0}' description '{1}' was expected to be '{2}'", expected.Name, actual.Description, expected.Description))
                .FailIf(actual.IsInternal != expected.IsInternal,
                () => string.Format("Product '{0}' was '{1}' but was expected to be '{2}'", expected.Name,
                    actual.IsInternal ? "internal" : "not internal",
                    expected.IsInternal ? "internal" : "not internal"))
                .Combine(Compare(actual.Type, expected.Type, expected.Name));
        }

        static Outcome<string> Compare(ProductType actual, ProductType expected, string productName)
        {
            if (actual.GetType() != expected.GetType())
                return Outcome.Failure(string.Format("Product '{0}' type '{1}' was expected to be '{2}'",
                    productName, actual.GetType().Name, expected.GetType().Name));
            if (actual is DateProductType)
                return Outcome.Success();
            if (actual is TextProductType)
            {
                var a = ((TextProductType) actual).MaxWidth;
                var e = ((TextProductType) expected).MaxWidth;
                return Outcome.Success().FailIf(a != e,
                    () =>string.Format("Text product '{0}' had a max width of {1} but was expected to have {2}",
                        productName, a, e));
            }
            if (actual is NumericProductType)
            {
                var an = (NumericProductType)actual;
                var en = (NumericProductType)expected;
                var a = string.Format("{0}.{1}", an.Scale, an.Precision);
                var e = string.Format("{0}.{1}", en.Scale, en.Precision);
                return Outcome.Success().FailIf(a != e,
                    () => string.Format("Text product '{0}' had a spec of {1} but was expected to have {2}",
                        productName, a, e));
            }
            return Outcome.Failure(string.Format("Product '{0}' had unexpected type '{1}'",
                productName,actual.GetType().Name));
        }
        */
        #endregion

        [Test]
        public void Succeeded()
        {
            Assert.That(_compilation.WasSuccessful, Is.True, _compilation.Observations.Delimit("\r\n"));
        }
        /*
        [Test]
        public void EmptyPublicForm_SyntaxWasBuiltAsExpected()
        {
            var c = Compare(_compilations["Empty"].Value, ExpectedEmptyPublicForm);
            Assert.That(c.WasSuccessful, Is.True, c.Observations.Delimit("\r\n"));
        }

        [Test]
        public void Full_Succeeded()
        {
            var c = _compilations["Full"];
            Assert.That(c.WasSuccessful, Is.True, c.Observations.Select(x => x.Message).Delimit("\r\n"));
        }
        [Test]
        public void FullPublicForm_SyntaxWasBuiltAsExpected()
        {
            var c = Compare(_compilations["Full"].Value, ExpectedFullPublicForm);
            Assert.That(c.WasSuccessful, Is.True, c.Observations.Delimit("\r\n"));
        }
        */
    }
}