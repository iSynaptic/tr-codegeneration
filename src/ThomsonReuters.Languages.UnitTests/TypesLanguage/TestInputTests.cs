using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ThomsonReuters.CodeGeneration;
using ThomsonReuters.Languages.TypesLanguage.Visitors;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using NUnit.Framework;

namespace ThomsonReuters.Languages.TypesLanguage
{
    [TestFixture]
    public class TestInputTests
    {
        private Outcome<string> _Outcome;
        private Compilation _Compilation = null;

        [TestFixtureSetUp]
        public void BeforeAllTests()
        {
            var result = DescriptionProvider.GetDescription(".src");

            _Outcome = result.ToOutcome();
            _Compilation = result.ToMaybe().ValueOrDefault();

            _Outcome = result.ToOutcome();

            if (result.WasSuccessful && result.HasValue)
                _Compilation = result.Value;
        }

        [Test]
        public void Compilation_IsNotNull()
        {
            Assert.NotNull(_Compilation);
        }

        [Test]
        public void Compilation_ResultedInNoErrors()
        {
            Assert.IsTrue(_Outcome.WasSuccessful, _Outcome.Observations.Delimit("\r\n"));
        }

        [Test]
        public void Generate_Entities()
        {
            if (_Compilation == null)
                Assert.Inconclusive();

            var generator = new EntitiesCodeGenerator(Console.Out);
            generator.Visit(_Compilation);
        }

        [Test]
        public void Generate_Values()
        {
            if (_Compilation == null)
                Assert.Inconclusive();

            var generator = new ValuesCodeGenerator(Console.Out);
            generator.Visit(_Compilation);
        }

        [Test]
        public void Generate_WebApiClient()
        {
            if (_Compilation == null)
                Assert.Inconclusive();

            var generator = new WebApiClientCodeGenerator(Console.Out, true);
            generator.Visit(_Compilation);
        }

        [Test]
        public void Generate_WebApiServer()
        {
            if (_Compilation == null)
                Assert.Inconclusive();

            var generator = new WebApiServerCodeGenerator(Console.Out);
            generator.Visit(_Compilation);
        }

        [Test]
        public void Generate_WebApiResourceKeys()
        {
            if (_Compilation == null)
                Assert.Inconclusive();

            var generator = new WebApiResourceKeysCodeGenerator(Console.Out);
            generator.Visit(_Compilation);
        }        

        [Test]
        public void WriteSummary()
        {
            if(_Compilation == null)
                Assert.Inconclusive();

            var generator = new ModelSummaryGenerator();
            generator.Visit(_Compilation);
        }

        public class ModelSummaryGenerator : TextOutputVisitor
        {
            public ModelSummaryGenerator() : base("  ", Console.Out)
            {
            }

            public override void Visit<T>(T subject)
            {
                if(subject is NamespaceSymbol)
                {
                    base.Visit(subject);
                    return;
                }

                WriteLine("{0}: {1}({2})", subject.GetType().Name,
                          subject.ToMaybe<ISymbol>().Select(x => x.Name.ToString()).ValueOrDefault(""),
                          subject.ToMaybe<IAnnotatable>().Select(x => x.Annotations.Delimit(", ", y => y.Key.ToString())).ValueOrDefault(""));

                using(WithIndentation())
                    base.Visit(subject);
            }

            public override void VisitNamespace(NamespaceSymbol @namespace)
            {
                Func<NamespaceSymbol, bool> hasInterestingMembers =
                    n => n.Members.Any(x => !(x is NamespaceSymbol) && !NotInterestedIn(x));

                if (hasInterestingMembers(@namespace))
                {
                    var nameParts = @namespace
                        .Recurse(x => x.Parent.ToMaybe<NamespaceSymbol>())
                        .TakeWhile(x => x == @namespace || !hasInterestingMembers(x))
                        .Select(x => x.Name)
                        .Reverse()
                        .ToArray();

                    var name = new QualifiedIdentifier(nameParts);

                    WriteLine("Namespace: {0}", name);

                    using (WithIndentation())
                        base.VisitNamespace(@namespace);
                }
                else
                    base.VisitNamespace(@namespace);
            }
        }
    }
}