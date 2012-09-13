using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public interface IAnnotatable
    {
        ReadOnlyDictionary<Identifier, ReadOnlyCollection<Annotation>> Annotations { get; }
    }
}
