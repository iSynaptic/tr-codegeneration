using System;

namespace ThomsonReuters.CodeGeneration
{
    public struct Nil : IEquatable<Nil>
    {
        public bool Equals(Nil other)
        {
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            return obj is Nil;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Nil left, Nil right)
        {
            return true;
        }

        public static bool operator !=(Nil left, Nil right)
        {
            return false;
        }
    }
}
