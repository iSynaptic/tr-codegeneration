using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public class WebApiFilterAwareQueryCodeGenerator : CodeGenerator
    {
        public WebApiFilterAwareQueryCodeGenerator(StringBuilder sb)
            : base(sb)
        {
        }

        public WebApiFilterAwareQueryCodeGenerator(TextWriter writer)
            : base(writer)
        {
        }

        protected IEnumerable<ArgumentDefinition> GetQueryArguments(BaseWebApiQuerySymbol query)
        {
            //get query argument, if any, then append filters, if any
            return new[]{
                            query.Argument
                                .Select(x => new ArgumentDefinition(GetPublicTypeString(x, query), x.Name.ToString(), x.Cardinality))
                                .ValueOrDefault((ArgumentDefinition)null)}
                .Concat(query.Filters
                            .Select(x => new ArgumentDefinition(GetPublicTypeString(x, query), x.Name.ToString(), x.Cardinality)))
                .Where(x => x != null);
        }
    }
}