using System.Collections.Generic;
using System.Linq;
using MetaSharp.Transformation;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages
{
    public class Annotation : INode
    {
        public Annotation(Identifier name, IEnumerable<AnnotationPair> pairs)
        {
            Name = Guard.NotNull(name, "name");
            Pairs = Guard.NotNull(pairs, "pairs")
                .ToDictionary(x => x.Name)
                .ToReadOnlyDictionary();
        }

        public Identifier Name { get; private set; }
        public ReadOnlyDictionary<Identifier, AnnotationPair> Pairs { get; private set; }
    }
}
