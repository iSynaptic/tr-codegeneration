using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages;
using ThomsonReuters.Languages.TypesLanguage;
using ThomsonReuters.Languages.TypesLanguage.Visitors;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using ISymbol = ThomsonReuters.Languages.ISymbol;

namespace ThomsonReuters.CodeGeneration
{
    public abstract class CodeGenerator : TextOutputVisitor
    {
        private List<QualifiedIdentifier> _Usings = new List<QualifiedIdentifier>();

        protected CodeGenerator(StringBuilder sb) : this(new StringWriter(Guard.NotNull(sb, "sb")))
        {
        }

        protected CodeGenerator(TextWriter writer) : base("    ", writer)
        {
            AddUsings();
        }

        protected virtual void AddUsings()
        {
            AddUsing("System");
            AddUsing("System.Collections.Generic");
            AddUsing("System.ComponentModel");
            AddUsing("System.Linq");
        }

        protected void AddUsing(QualifiedIdentifier qualifiedIdentifier)
        {
            Guard.NotNull(qualifiedIdentifier, "qualifiedIdentifier");
            _Usings.Add(qualifiedIdentifier);
        }

        public override void VisitCompilation(Compilation compilation)
        {
            foreach (var @using in _Usings)
                WriteLine("using {0};", @using);

            WriteLine();

            base.VisitCompilation(compilation);
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

                WriteLine("namespace {0}", name);

                using (WithBlock())
                    base.VisitNamespace(@namespace);

                WriteLine();
            }
            else
                base.VisitNamespace(@namespace);
        }

        protected virtual string GetPublicTypeString(TypeReference reference, ISymbol relativeTo = null)
        {
            var type = reference.Type;
            var cardinality = reference.Cardinality;

            var builtInType = type as BuiltInType;
            if (builtInType != null && builtInType.Type == typeof(void))
                return "void";

            var fullName = GetRelativeName(type.FullName, relativeTo != null ? relativeTo.FullName : null);

            if (cardinality.CanBeMoreThanOne())
                return string.Format("IEnumerable<{0}>", fullName);

            if (cardinality.IsZeroOrOne() &&
                ((type is ExternalEnumSymbol) ||
                (type is ExternalType && ((ExternalType)type).Type.IsValueType)))
                return string.Format("{0}?", fullName);

            return fullName;
        }

        protected virtual string GetPrivateTypeString(TypeReference reference, ISymbol relativeTo = null)
        {
            var type = reference.Type;
            var cardinality = reference.Cardinality;

            var builtInType = type as BuiltInType;
            if (builtInType != null && builtInType.Type == typeof(void))
                return "void";

            var fullName = GetRelativeName(type.FullName, relativeTo != null ? relativeTo.FullName : null);

            if (cardinality.CanBeMoreThanOne())
                return string.Format("{0}[]", fullName);

            if (cardinality.IsZeroOrOne() &&
                ((type is ExternalEnumSymbol) ||
                (type is ExternalType && ((ExternalType)type).Type.IsValueType)))
                return string.Format("{0}?", fullName);

            return fullName;
        }

        protected QualifiedIdentifier GetRelativeName(QualifiedIdentifier name, QualifiedIdentifier relativeTo)
        {
            if (name == relativeTo)
                return name[name.Length - 1];

            QualifiedIdentifier result = name;

            if (!ReferenceEquals(relativeTo, null))
            {
                result = new QualifiedIdentifier(name
                    .ZipAll(relativeTo)
                    .SkipWhile(x => x[0] == x[1])
                    .Select(x => x[0])
                    .Squash());
            }

            var @using = _Usings
                .Where(name.StartsWith)
                .OrderByDescending(x => x.Length)
                .FirstOrDefault();

            if (!ReferenceEquals(@using, null))
                return new QualifiedIdentifier(name.Skip(@using.Length));

            return result;
        }

        protected virtual IDisposable WriteBlock(string formatString, params object[] args)
        {
            WriteLine(formatString, args);
            return WithBlock();
        }

        protected virtual IDisposable WithBlock(bool withStatementEnd = false)
        {
            WriteLine("{");
            var indentation = WithIndentation();

            Action onDispose = () =>
            {
                indentation.Dispose();
                Write("}");

                if(withStatementEnd)
                    WriteLine(";");
                else
                    WriteLine();
            };

            return onDispose.ToDisposable();
        }

        protected void WriteHideMembers()
        {
            var attribute = GetRelativeName(new QualifiedIdentifier("System.ComponentModel.EditorBrowsable"), null);
            var enumValue = GetRelativeName(new QualifiedIdentifier("System.ComponentModel.EditorBrowsableState.Never"), null);

            var neverShow = string.Format("[{0}({1})]", attribute, enumValue);

            WriteLine("{0} public override bool Equals(object obj) {{ return base.Equals(obj); }}", neverShow);
            WriteLine("{0} public override int GetHashCode() {{ return base.GetHashCode(); }}", neverShow);
            WriteLine("{0} public override string ToString() {{ return base.ToString(); }}", neverShow);
            WriteLine("{0} public new Type GetType() {{ return base.GetType(); }}", neverShow);
        }

    }
}
