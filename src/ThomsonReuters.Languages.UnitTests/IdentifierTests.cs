using System.Collections.Generic;
using NUnit.Framework;

namespace ThomsonReuters.Languages
{
    [TestFixture]
    public class IdentifierTests
    {
        [Test]
        public void Identifer_CanBeUsedAsKeyInDictionary()
        {
            var dictionary = new Dictionary<Identifier, string>
                                 {
                                     { new Identifier("FOO"), "foo" }
                                 };

            Assert.AreEqual("foo", dictionary[new Identifier("FOO")]);
        }
    }
}
