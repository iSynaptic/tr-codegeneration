using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.CodeGeneration
{
    public class DomainCodeGenerator : CSharpCodeGenerator
    {
        public DomainCodeGenerator(TextWriter writer)
            : base(writer)
        {
        }

        public DomainCodeGenerator(string indentationToken, TextWriter writer)
            : base(indentationToken, writer)
        {
        }

        private static readonly Regex SymbolRegex = new Regex("([^a-zA-Z0-9]+)|([a-zA-Z0-9]+)");

        public static string GetSymbol(string name)
        {
            var id = SymbolRegex.Matches(name)
                .OfType<Match>()
                .Select(y => y.Value.Trim())
                .Where(y => !String.IsNullOrEmpty(y))
                .Select(TranslateIdPartToNamePart)
                .Delimit(String.Empty);
            if (Char.IsDigit(id[0]))
                return "_" + id;
            return id;
        }

        private static string TranslateIdPartToNamePart(string part)
        {
            if (Char.IsLetterOrDigit(part[0]))
            {
                return part[0].ToString(CultureInfo.InvariantCulture).ToUpperInvariant() + part.ToLowerInvariant().Substring(1);
            }
            if (part == "&")
                return "And";
            if (part == "@")
                return "At";
            return new string('_', part.Length);
        }
    }
}
