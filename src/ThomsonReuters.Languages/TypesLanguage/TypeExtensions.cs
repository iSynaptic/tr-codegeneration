using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public static class TypeExtensions
    {
        public static bool IsBuiltIn(this IType type)
        {
            Guard.NotNull(type, "type");

            return type is BuiltInType;
        }

        public static bool IsBuiltIn<T>(this IType type)
        {
            return IsBuiltIn(type, typeof (T));
        }

        public static bool IsBuiltIn(this IType type, Type desiredType)
        {
            Guard.NotNull(type, "type");
            Guard.NotNull(desiredType, "desiredType");

            var builtIn = type as BuiltInType;
            return builtIn != null && builtIn.Type == desiredType;
        }

        public static bool IsVoid(this IType type)
        {
            return type.IsBuiltIn(typeof (void));
        }
    }
}
