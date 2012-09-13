using MetaSharp.Transformation;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages
{
    public class AnnotationPair : INode
    {
        public AnnotationPair(Identifier name, string value)
        {
            Name = Guard.NotNull(name, "name");
            Value = Guard.NotNull(value, "value");
        }

        public Identifier Name { get; private set; }
        public string Value { get; private set; }
    }
}