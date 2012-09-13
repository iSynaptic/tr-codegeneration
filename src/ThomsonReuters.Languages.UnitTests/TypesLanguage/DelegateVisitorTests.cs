using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using ThomsonReuters.Languages.TypesLanguage.Visitors;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage
{
    [TestFixture]
    public class DelegateVisitorTests
    {
        [Test]
        public void OnWebApiCommand()
        {
            string input = "namespace Foo { webapi Bar { command void Baz(); } }";
            var results = TypesLanguageCompiler.Compile(new[] { new TypesLanguageCompilerInput(input, "") }, Enumerable.Empty<Assembly>());

            if (!results.WasSuccessful || !results.HasValue)
                Assert.Fail("Expected compilation success.\r\n" + results.Observations.Delimit("\r\n"));

            string output = "";

            var visitor = new DelegateVisitor { OnWebApiCommand = (cmd, b) => output += cmd.Name };
            visitor.Visit(results.Value);

            Assert.AreEqual("Baz", output);
        }
    }
}
