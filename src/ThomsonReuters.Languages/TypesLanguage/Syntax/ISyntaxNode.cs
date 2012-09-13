using iSynaptic.Commons;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public interface ISyntaxNode : INode
    {
        Maybe<ISyntaxNode> Parent { get; set; }
    }
}
