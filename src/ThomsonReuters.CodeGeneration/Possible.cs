using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ThomsonReuters.CodeGeneration
{
    public interface IPossible
    {
        bool HasValue { get; }
        bool WasSuccessful { get; }
        object Value { get; }
        IEnumerable<object> Observations { get; }
    }

    [DebuggerDisplay("{ToDebugString()}")]
    public struct Possible<T> : IEquatable<Possible<T>>, IEquatable<T>, IPossible
    {
        public static readonly Possible<T> NoValue = default(Possible<T>);

        private T _Value;

        public Possible(T value)
            : this()
        {
            Value = value;
            HasValue = value != null;
        }

        bool IPossible.WasSuccessful
        {
            get { return true; }
        }

        object IPossible.Value { get { return Value; } }

        IEnumerable<object> IPossible.Observations
        {
            get { return Enumerable.Empty<object>(); }
        }

        public T Value
        {
            get
            {
                if (HasValue)
                     return _Value;
 
                 throw new InvalidOperationException("No value can be computed.");
            }

            private set { _Value = value; }
        }

        public bool HasValue { get; private set; }

        #region Equality Implementation

        public bool Equals(T other)
        {
            return Equals(new Possible<T>(other));
        }

        public bool Equals(Possible<T> other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public bool Equals(Possible<T> other, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (!HasValue)
                return !other.HasValue;

            return other.HasValue && comparer.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (obj is Possible<T>)
                return Equals((Possible<T>)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<T>.Default);
        }

        public int GetHashCode(IEqualityComparer<T> comparer)
        {
            if(comparer == null)
                throw new ArgumentNullException("comparer");

            if (HasValue != true)
                return -1;

            return comparer.GetHashCode(Value);
        }

        #endregion

        #region Equality Operators

        public static bool operator ==(Possible<T> left, Possible<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Possible<T> left, Possible<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(Possible<T> left, T right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Possible<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, Possible<T> right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(T left, Possible<T> right)
        {
            return !(left == right);
        }

        #endregion

        public Possible<TResult> OfType<TResult>()
        {
            if (HasValue && Value is TResult)
                return new Possible<TResult>((TResult)(object)Value);

            return Possible<TResult>.NoValue;
        }

        public T ValueOrDefault()
        {
            return HasValue
                ? Value
                : default(T);
        }

        public static explicit operator T(Possible<T> value)
        {
            return value.Value;
        }

        private string ToDebugString()
        {
            return !HasValue ? "{no value}" : Value.ToString();
        }
    }

    [DebuggerDisplay("{ToDebugString()}")]
    public struct Possible<T, TObservation> : IEquatable<Possible<T, TObservation>>, IEquatable<Possible<T>>, IEquatable<T>, IPossible
    {
        public static readonly Possible<T, TObservation> NoValue = default(Possible<T, TObservation>);

        private T _Value;

        private bool _WasFailure;
        private TObservation[] _Observations;

        public Possible(T value)
            : this()
        {
            Value = value;
            HasValue = value != null;
            Observations = new TObservation[0];
        }

        public Possible(bool wasSuccessful, IEnumerable<TObservation> observations)
            : this()
        {
            WasSuccessful = wasSuccessful;
            Observations = observations != null
                ? observations.ToArray()
                : new TObservation[0];
        }

        public Possible(bool wasSuccessful, TObservation observation)
            : this(wasSuccessful, new [] { observation })
        {
        }

        public Possible(T value, bool wasSuccessful, IEnumerable<TObservation> observations)
            : this()
        {
            Value = value;
            HasValue = value != null;
            WasSuccessful = wasSuccessful;
            Observations = observations != null
                ? observations.ToArray()
                : new TObservation[0];
        }

        public T Value
        {
            get
            {
                 if (HasValue)
                     return _Value;
 
                 throw new InvalidOperationException("No value can be computed.");
            }

            private set { _Value = value; }
        }

        public bool HasValue { get; private set; }
        public bool WasSuccessful { get { return !_WasFailure; } private set { _WasFailure = !value; } }

        public IEnumerable<TObservation> Observations
        {
            get { return _Observations.AsEnumerable(); }
            private set { _Observations = value.ToArray(); }
        }

        bool IPossible.WasSuccessful
        {
            get { return !_WasFailure; }
        }

        object IPossible.Value { get { return Value; } }

        IEnumerable<object> IPossible.Observations
        {
            get { return _Observations.Cast<object>(); }
        }

        #region Equality Implementation

        public bool Equals(T other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public bool Equals(T other, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (!HasValue)
                return false;

            return comparer.Equals(Value, other);
        }

        public bool Equals(Possible<T> other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public bool Equals(Possible<T> other, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (!HasValue)
                return !other.HasValue;

            return other.HasValue && comparer.Equals(Value, other.Value);
        }

        public bool Equals(Possible<T, TObservation> other)
        {
            return Equals(other, EqualityComparer<T>.Default);
        }

        public bool Equals(Possible<T, TObservation> other, IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            if (!HasValue)
                return !other.HasValue;

            return other.HasValue && comparer.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Possible<T, TObservation>)
                return Equals((Possible<T, TObservation>)obj);

            if (obj is Possible<T>)
                return Equals((Possible<T>)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return GetHashCode(EqualityComparer<T>.Default);
        }

        public int GetHashCode(IEqualityComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            int hashCode = HasValue
                ? Value.GetHashCode()
                : -1;

            hashCode ^= WasSuccessful.GetHashCode();

            return hashCode;
        }

        #endregion

        #region Equality Operators

        public static bool operator ==(Possible<T, TObservation> left, Possible<T, TObservation> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Possible<T, TObservation> left, Possible<T, TObservation> right)
        {
            return !(left == right);
        }

        public static bool operator ==(Possible<T, TObservation> left, Possible<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Possible<T, TObservation> left, Possible<T> right)
        {
            return !(left == right);
        }

        public static bool operator ==(Possible<T, TObservation> left, T right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Possible<T, TObservation> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, Possible<T, TObservation> right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(T left, Possible<T, TObservation> right)
        {
            return !(left == right);
        }

        public static bool operator ==(Possible<T> left, Possible<T, TObservation> right)
        {
            return right.Equals(left);
        }

        public static bool operator !=(Possible<T> left, Possible<T, TObservation> right)
        {
            return !(left == right);
        }

        #endregion
        
        public Possible<TResult, TObservation> OfType<TResult>()
        {
            if(HasValue && Value is TResult)
                return new Possible<TResult, TObservation>((TResult)(object)Value, WasSuccessful, Observations);

            return new Possible<TResult, TObservation>(WasSuccessful, Observations);
        }

        public T ValueOrDefault()
        {
            return HasValue
                ? Value
                : default(T);
        }

        public static explicit operator T(Possible<T, TObservation> value)
        {
            return value.Value;
        }

        public static implicit operator bool(Possible<T, TObservation> possible)
        {
            return possible.WasSuccessful;
        }

        private string ToDebugString()
        {
            return string.Format("{0}: {1} ({2} observations)",
                WasSuccessful ? "Success" : "Failure",
                !HasValue ? (object)"{no value}" : Value,
                Observations.Count());
        }
    }
}
