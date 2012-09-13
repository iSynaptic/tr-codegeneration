using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages
{
    public class IndentedTextWriter : TextWriter
    {
        private readonly TextWriter _Underlying = null;
        private readonly string _IndentationToken = null;

        private string _IndentationString = null;

        private string _LastChars = null;

        public IndentedTextWriter(string indentationToken, TextWriter underlying)
            : this(indentationToken, underlying, null)
        {
        }

        public IndentedTextWriter(string indentationToken, TextWriter underlying, IFormatProvider formatProvider)
            : base(formatProvider)
        {
            _Underlying = Guard.NotNull(underlying, "underlying");
            _IndentationToken = Guard.NotNull(indentationToken, "indentationToken");

            _IndentationString = "";
        }

        public void IncreaseIndent()
        {
            SetIndentation(Indentation + 1);
        }

        public void DecreaseIndent()
        {
            SetIndentation(Indentation - 1);
        }

        private void SetIndentation(int indentation)
        {
            if (indentation < 0)
                indentation = 0;

            Indentation = indentation;
            _IndentationString = indentation > 0 
                ? String.Concat(Enumerable.Repeat(_IndentationToken, indentation)) 
                : "";
        }

        public override void Write(char value)
        {
            if (_IndentationString.Length > 0)
            {
                if (_LastChars == NewLine || _LastChars == null)
                    _Underlying.Write(_IndentationString);
            }

            if (_LastChars == null)
                _LastChars = new string(new[] {value});

            else if (_LastChars.Length < NewLine.Length)
                _LastChars = string.Concat(_LastChars, value);

            else if (_LastChars.Length == NewLine.Length)
                _LastChars = string.Concat(_LastChars.Substring(1, NewLine.Length - 1), value);

            _Underlying.Write(value);
        }

        public override string NewLine
        {
            get { return _Underlying.NewLine; }
            set { _Underlying.NewLine = value; }
        }

        public override Encoding Encoding
        {
            get { return _Underlying.Encoding; }
        }

        public int Indentation { get; private set; }
    }
}
