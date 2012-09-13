using NUnit.Framework;

namespace ThomsonReuters.Languages
{
    [TestFixture]
    public class QualifiedIdentifierTests
    {
        [Test]
        public void StartsWith_WorksCorrectly()
        {
            var identifier = new QualifiedIdentifier("ThomsonReuters.Languages.UnitTests");

            Assert.IsTrue(identifier.StartsWith(new Identifier("ThomsonReuters")));
            Assert.IsTrue(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters")));
            Assert.IsTrue(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters.Languages")));
            Assert.IsTrue(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters.Languages.UnitTests")));

            Assert.IsFalse(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters.Languages.Tests")));
            Assert.IsFalse(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters.Langs")));
            Assert.IsFalse(identifier.StartsWith(new QualifiedIdentifier("ThomsonReuters.Foo")));
            Assert.IsFalse(identifier.StartsWith(new QualifiedIdentifier("TR")));
        }
    }
}
