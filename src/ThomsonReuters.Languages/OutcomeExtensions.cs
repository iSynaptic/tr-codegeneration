using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages
{
    public static class OutcomeExtensions
    {
        public static Outcome<string> InformWithFormattedErrors(this Outcome<Error> @this, string source = null)
        {
            return @this.Inform(x => FormatError(x, source));
        }

        public static string FormatError(Error error, string source)
        {
            return string.Format("{0} line {1}, col {2}: {3}",
                          source ?? "input",
                          error.Span.Start.ToMaybe<CharacterPosition>().Select(y => y.Line + 1).ValueOrDefault(),
                          error.Span.Start.ToMaybe<CharacterPosition>().Select(y => y.Column + 1).ValueOrDefault(),
                          error.Message);
        }
    }
}
