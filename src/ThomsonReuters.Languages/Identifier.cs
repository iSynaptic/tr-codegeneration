using System;
using System.Text.RegularExpressions;
using iSynaptic.Commons;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages
{
    public class Identifier : IEquatable<Identifier>, IEquatable<string>, INode
    {
        public static readonly Regex IdentifierPattern =
            new Regex("^[a-z]([a-z0-9])*$", RegexOptions.IgnoreCase);

        private readonly string _Identifier = null;

        public Identifier(string identifier)
        {
            Guard.NotNullOrWhiteSpace(identifier, "identifier");

            if(IdentifierPattern.IsMatch(identifier) != true)
                throw new ArgumentException("Identifier is not in the right format. It must match the IdentifierPattern.", "identifier");

            _Identifier = identifier;
        }

        public bool Equals(Identifier other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return other == _Identifier;
        }

        public bool Equals(string other)
        {
            return _Identifier == other;
        }

        public override bool Equals(object obj)
        {
            var qi = obj as Identifier;
            if (ReferenceEquals(qi, null) != true)
                return Equals(qi);

            var text = obj as string;
            if (text != null)
                return Equals(text);

            return false;
        }

        public override int GetHashCode()
        {
            return _Identifier.GetHashCode();
        }

        public override string ToString()
        {
            return _Identifier;
        }

        public static bool operator ==(Identifier left, Identifier right)
        {
            if (!ReferenceEquals(left, null) ^ !ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Identifier left, Identifier right)
        {
            return !(left == right);
        }

        public static bool operator ==(Identifier left, string right)
        {
            if (!ReferenceEquals(left, null) ^ !ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(Identifier left, string right)
        {
            return !(left == right);
        }

        public static implicit operator Identifier(string identifier)
        {
            return new Identifier(identifier);
        }

        public static implicit operator string(Identifier identifier)
        {
            return identifier.ToString();
        }

        public static implicit operator QualifiedIdentifier(Identifier identifier)
        {
            Guard.NotNull(identifier, "identifier");
            return new QualifiedIdentifier(new []{identifier});
        }
    }
}
