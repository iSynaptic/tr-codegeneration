using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages.TypesLanguage;

namespace ThomsonReuters.CodeGeneration
{
    public class ValuesCodeGenerator : MoleculeCodeGenerator
    {
        public ValuesCodeGenerator(StringBuilder sb) : base(sb)
        {
        }

        public ValuesCodeGenerator(TextWriter writer) : base(writer)
        {
        }

        protected override bool NotInterestedIn(object subject)
        {
            var interestedIn = new[]
            {
                typeof(Compilation),
                typeof(NamespaceSymbol),
                typeof(ComplexValueSymbol)
            };

            return !(interestedIn.Contains(subject.GetType()))
                   || base.NotInterestedIn(subject);
        }

        public override void VisitComplexValue(ComplexValueSymbol value)
        {
            WriteMolecule(value);
        }
    }
}