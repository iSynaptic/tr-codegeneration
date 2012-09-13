using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Visitors
{
    public abstract class TextOutputVisitor : TypesLanguageVisitor
    {
        private readonly IndentedTextWriter _Writer = null;

        protected TextOutputVisitor(TextWriter writer) : this("  ", writer)
        {
        }

        protected TextOutputVisitor(string indentationToken, TextWriter writer)
        {
            Guard.NotNull(indentationToken, "indentationToken");
            Guard.NotNull(writer, "writer");

            _Writer = new IndentedTextWriter(indentationToken, writer);
        }

        protected virtual void VisitDelimited<T>(IEnumerable<T> subjects, string delimiter)
        {
            Guard.NotNullOrEmpty(delimiter, "delimiter");
            VisitDelimited(subjects, (x, y) => delimiter);
        }

        protected virtual void VisitDelimited<T>(IEnumerable<T> subjects, Func<T, T, string> delimiter)
        {
            Guard.NotNull(delimiter, "delimiter");
            if(subjects != null)
            {
                T lastSubject = default(T);
                bool isFirst = true;

                foreach(var subject in subjects)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        Write(delimiter(lastSubject, subject));

                    Visit(subject);
                }
            }
        }

        protected virtual void Write(string text)
        {
            _Writer.Write(text);
        }

        protected virtual void Write(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args));
        }

        protected virtual void WriteLine()
        {
            Write(Environment.NewLine);
        }

        protected virtual void WriteLine(string text)
        {
            Write(text);
            WriteLine();
        }

        protected virtual void WriteLine(string formatString, params object[] args)
        {
            Write(string.Format(formatString, args));
            WriteLine();
        }

        protected virtual void IncreaseIndent()
        {
            _Writer.IncreaseIndent();
        }

        protected virtual void DecreaseIndent()
        {
            _Writer.DecreaseIndent();
        }

        protected IDisposable WithIndentation()
        {
            IncreaseIndent();

            Action onDispose = DecreaseIndent;
            return onDispose.ToDisposable();
        }
    }
}
