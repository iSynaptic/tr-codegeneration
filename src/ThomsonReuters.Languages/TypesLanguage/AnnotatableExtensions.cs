using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public static class AnnotatableExtensions
    {
        public static IEnumerable<Annotation> GetAnnotations(this IAnnotatable @this, Identifier key)
        {
            Guard.NotNull(@this, "@this");
            Guard.NotNull(key, "key");

            return @this.Annotations
                .TryGetValue(key)
                .Squash();
        }

        public static Maybe<Annotation> GetFirstAnnotation(this IAnnotatable @this, Identifier key)
        {
            return GetAnnotations(@this, key)
                .TryFirst();
        }

        public static Maybe<Annotation> GetSingleAnnotation(this IAnnotatable @this, Identifier key)
        {
            return GetAnnotations(@this, key)
                .TrySingle();
        }
    }
}
