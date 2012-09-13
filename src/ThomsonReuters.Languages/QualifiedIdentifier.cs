using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages
{
    public class QualifiedIdentifier : IEquatable<QualifiedIdentifier>, IEnumerable<Identifier>, IEquatable<string>, INode
    {
        private readonly Identifier[] _Identifiers = null;

        public QualifiedIdentifier(string identifier)
        {
            Guard.NotNullOrWhiteSpace(identifier, "identifier");
            Identifier[] ids = null;

            try
            {
                ids = identifier.Split('.')
                    .Select(x => new Identifier(x))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Identifier was not valid.", "identifier", ex);
            }

            var isValid = ValidateIdentifierChain(ids);

            if (!isValid)
                throw new ArgumentException(string.Format("Identifier chain was not valid:\r\n{0}", isValid.Observations.Delimit("\r\n")));

            _Identifiers = ids;
        }

        public QualifiedIdentifier(IEnumerable<Identifier> identifierChain)
        {
            Guard.NotNull(identifierChain, "identifierChain");

            var ids = identifierChain.ToArray();
            var isValid = ValidateIdentifierChain(ids);

            if(!isValid)
                throw new ArgumentException(string.Format("Identifier chain was not valid:\r\n{0}", isValid.Observations.Delimit("\r\n")));

            _Identifiers = ids;
        }

        private static Outcome<string> ValidateIdentifierChain(Identifier[] identifierChain)
        {
            if (identifierChain.Length <= 0)
                return Outcome.Failure("No identifiers were provided.");

            if (identifierChain.Any(x => ReferenceEquals(x ,null)))
                return Outcome.Failure("One or more identifiers were null.");

            return Outcome<string>.Success;
        }

        public Maybe<QualifiedIdentifier> Parent
        {
            get
            {
                return Length >= 2 
                    ? new QualifiedIdentifier(this.Take(Length - 1)).ToMaybe()
                    : Maybe.NoValue;
            }
        }

        public int Length
        {
            get { return _Identifiers.Length; }
        }

        public Identifier this[int index]
        {
            get { return _Identifiers[index]; }
        }

        public IEnumerator<Identifier> GetEnumerator()
        {
            return _Identifiers
                .AsEnumerable()
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return _Identifiers.Delimit(".");
        }

        public bool StartsWith(Identifier identifier)
        {
            return StartsWith(new QualifiedIdentifier(new[] {identifier}));
        }

        public bool StartsWith(QualifiedIdentifier identifier)
        {
            return identifier
                .ZipAll(_Identifiers)
                .Where(x => x[0].HasValue)
                .All(x => x[0] == x[1]);
        }

        public bool Equals(QualifiedIdentifier other)
        {
            if(ReferenceEquals(other, null))
                return false;

            return _Identifiers
                .ZipAll(other)
                .All(x => x[0] == x[1]);
        }

        public bool Equals(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return ToString() == text;
        }

        public override bool Equals(object obj)
        {
            var qi = obj as QualifiedIdentifier;
            if (ReferenceEquals(qi, null) != true)
                return Equals(qi);

            var text = obj as string;
            if (text != null)
                return Equals(text);

            return false;
        }

        public override int GetHashCode()
        {
            return _Identifiers.Aggregate(0, (i, id) => i ^ id.ToString().GetHashCode());
        }

        public static bool operator ==(QualifiedIdentifier left, QualifiedIdentifier right)
        {
            if (!ReferenceEquals(left, null) ^ !ReferenceEquals(right, null))
                return false;
            
            return left.Equals(right);
        }

        public static bool operator !=(QualifiedIdentifier left, QualifiedIdentifier right)
        {
            return !(left == right);
        }

        public static bool operator==(QualifiedIdentifier left, string right)
        {
            if (!ReferenceEquals(left, null) ^ !ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(QualifiedIdentifier left, string right)
        {
            return !(left == right);
        }

        public static QualifiedIdentifier operator+(QualifiedIdentifier left, Identifier right)
        {
            Guard.NotNull(left, "left");
            Guard.NotNull(right, "right");

            return new QualifiedIdentifier(left.Concat(new []{right}));
        }

        public static QualifiedIdentifier operator +(QualifiedIdentifier left, QualifiedIdentifier right)
        {
            Guard.NotNull(left, "left");
            Guard.NotNull(right, "right");

            return new QualifiedIdentifier(left.Concat(right));
        }

        public static implicit operator QualifiedIdentifier(string identifier)
        {
            return new QualifiedIdentifier(identifier);
        }

        public static implicit operator string(QualifiedIdentifier identifier)
        {
            return identifier.ToString();
        }
    }
}
