using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class TypeLookup
    {
        private IType _ResolvedType = null;
        private Func<IType> _Type = null;

        public TypeLookup(IType type)
        {
            _ResolvedType = type;
        }

        public TypeLookup(Func<IType> type)
        {
            _Type = Guard.NotNull(type, "type");
        }

        public IType Type
        {
            get
            {
                if(_Type != null)
                {
                    _ResolvedType = _Type();
                    _Type = null;
                }

                return _ResolvedType;
            }
        }
    }
}
