using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThomsonReuters.Languages;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.CodeGeneration
{
    public abstract class CSharpCodeGenerator
    {
        private readonly IndentedTextWriter _writer;

        protected CSharpCodeGenerator(TextWriter writer)
            : this("  ", writer)
        {
        }

        protected CSharpCodeGenerator(string indentationToken, TextWriter writer)
        {
            Guard.NotNull(indentationToken, "indentationToken");
            Guard.NotNull(writer, "writer");

            _writer = new IndentedTextWriter(indentationToken, writer);
        }

        protected void Write(string text)
        {
            _writer.Write(text);
        }

        protected void Write(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args));
        }

        protected void WriteLine()
        {
            Write(Environment.NewLine);
        }

        protected void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        protected void WriteLine(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args));
            WriteLine();
        }

        protected void Indent(Action contents)
        {
            _writer.IncreaseIndent();
            contents();
            _writer.DecreaseIndent();
        }

        protected void GenerateDelimited<T>(IEnumerable<T> items, Action delimit, Action<T> generator)
        {
            if (items == null)
                return;

            var i = items.ToArray();
            if (!i.Any())
                return;

            generator(i.First());
            i.Skip(1).Run(x =>
            {
                delimit();
                generator(x);
            });
        }

        protected void WriteBlock(Action contents, string closing = null)
        {
            WriteLine("{");
            Indent(contents);
            WriteLine("}}{0}", closing ?? string.Empty);
        }

        protected string Literal(string value)
        {
            if (value == null)
                return "null";
            return @"""" + value.Replace(@"\", @"\\")
                .Replace("\"", "\\\"")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ") + @"""";
        }

        protected string Literal(bool value)
        {
            return value ? "true" : "false";
        }

        protected string Literal(Enum value)
        {
            return string.Format("{0}.{1}", value.GetType().Name, value);
        }

        protected string Literal(object value)
        {
            if (value is string)
                return Literal((string) value);
            if (value is bool)
                return Literal((bool) value);
            if (value is Enum)
                return Literal((Enum) value);
            return value.ToString();
        }

        protected string Literal<T>(IEnumerable<T> value, string empty = null)
        {
            if (value == null)
                return empty ?? "null";
            return string.Format("new [] {{ {0} }}", value.Delimit(", ", x => Literal(x)));
        }
    }
}
