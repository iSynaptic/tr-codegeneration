using System;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages
{
    public class Cardinality
    {
        public Cardinality(int minimum)
            : this(minimum, default(Maybe<int>))
        {
        }

        public Cardinality(int minimum, int maximum)
            : this(minimum, new Maybe<int>(maximum))
        {
        }

        public Cardinality(int minimum, Maybe<int> maximum)
        {
            if (minimum < 0)
                throw new ArgumentOutOfRangeException("minimum", "Start of range must be a positive number.");

            Minimum = minimum;
            Maximum = maximum
                .ThrowOn(x => x.Select(y => y == 0 || y < minimum
                    ? new ArgumentOutOfRangeException("end", "End must be not be zero and greater than or equal to start.")
                    : null).ValueOrDefault())
                .Run();
        }

        public bool IsAtLeastOne()
        {
            return Minimum > 0;
        }

        public bool CanBeMoreThanOne()
        {
            return !Maximum.HasValue || Maximum.Value > 1;
        }

        public bool IsZeroOrOne()
        {
            return !IsAtLeastOne() &&
                   !CanBeMoreThanOne();
        }

        public bool IsOne()
        {
            return IsAtLeastOne() &&
                   !CanBeMoreThanOne();
        }

        public bool IsZeroOrMore()
        {
            return !IsAtLeastOne();
        }

        public bool IsOneOrMore()
        {
            return IsAtLeastOne() &&
                   CanBeMoreThanOne();
        }

        public int Minimum { get; private set; }
        public Maybe<int> Maximum { get; private set; }
    }
}