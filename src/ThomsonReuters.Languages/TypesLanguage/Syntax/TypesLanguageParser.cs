using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors;
using iSynaptic.Commons;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public partial class TypesLanguageParser : Parser
    {
        public static new Result<SyntaxTree, ParseError> Parse(string file)
        {
            if (!File.Exists(file))
                return Result.Failure(new ParseError(file, string.Format("Unable to file file: {0}", file), Maybe.NoValue, Maybe.NoValue));

            var fileInfo = new FileInfo(file);

            return Parse(File.ReadAllText(file), fileInfo.Name);
        }

        public static Result<SyntaxTree, ParseError> Parse(string text, string sourceName)
        {
            var context = new PatternContext();
            var parser = new TypesLanguageParser();

            var results = parser.DoParse(text, context);

            var syntaxTree = results as SyntaxTree;
            if (syntaxTree != null)
            {
                ParentAssignmentVisitor.AssignParent(syntaxTree);

                if (!context.Errors.Any())
                    return Result.Return(syntaxTree);
            }

            return Result.Failure(context.Errors.Select(x => new ParseError(sourceName,
                                                                            x.Message,
                                                                            x.Span.Start.ToMaybe<CharacterPosition>().Select(y => y.Line + 1),
                                                                            x.Span.Start.ToMaybe<CharacterPosition>().Select(y => y.Column + 1))));
        }

        private INode DoParse(string text, PatternContext context)
        {
            return base.Parse(text, context);
        }

        public sealed override INode Parse(string text, PatternContext context)
        {
            throw new NotSupportedException("You must call the following method: Result<Compilation, Error> Parse(string text);");
        }

        public Cardinality CardinalityOrDefault(object value)
        {
            return ((Cardinality)value) ?? new Cardinality(1, 1);
        }

        public IEnumerable<T> AsSequence<T>(object input)
        {
            var node = input as Node;

            if (input is IEnumerable)
                return ((IEnumerable) input).OfType<T>();

            if (node != null)
                return new[] { (T)node.Value };

            var nodes = input as Nodes;
            return nodes != null
                ? nodes.OfType<Node>().Select(x => x.Value).Cast<T>()
                : new T[0];
        }

        public Maybe<T> NoValue<T>()
        {
            return Maybe<T>.NoValue;
        }
    }
}
