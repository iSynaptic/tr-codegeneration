using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class ParseError
    {
        public ParseError(string sourceName, string message, Maybe<long> line, Maybe<long> column)
        {
            SourceName = sourceName;
            Message = message;
            Line = line;
            Column = column;
        }

        public string SourceName { get; private set; }
        public string Message { get; private set; }

        public Maybe<long> Line { get; private set; }
        public Maybe<long> Column { get; private set; }
    }
}
