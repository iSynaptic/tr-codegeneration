using System.Collections;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public partial class SagaLanguageParser
    {
        public new Result<SagaModuleNode, Error> Parse(string text)
        {
            var context = new PatternContext();
            var results = Parse(text, context);

            var syntax = results
                .ToMaybe()
                .OfType<Node>()
                .Select(x => x.Value)
                .OfType<SagaModuleNode>();

            if (syntax.HasValue)
            {
                if (!context.Errors.Any())
                {
                    var description = SagaDescriptionBuilder.Build(new[] {syntax.Value});
                    return Result.Return(syntax.Value);
                }
            }

            return Result.Failure(context.Errors.ToArray());
        }

        public IEnumerable<T> AsSequence<T>(object input)
        {
            if (input is T)
                return new [] { (T)input };

            var nodes = input as Nodes;
            if (nodes != null)
                return nodes.OfType<Node>().Select(x => x.Value).OfType<T>().ToArray();

            if (input is IEnumerable)
                return ((IEnumerable)input).OfType<T>().ToArray();

            var node = input as Node;
            if (node != null)
                return new[] { (T)node.Value };

            return new T[0];
        }

        public Maybe<T> NoValue<T>()
        {
            return Maybe<T>.NoValue;
        }

        protected uint ToUint(string s)
        {
            return s == null ? 0 : uint.Parse(s);
        }
    }
}
