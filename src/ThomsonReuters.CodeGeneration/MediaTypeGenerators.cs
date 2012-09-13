using System;
using System.Collections.Generic;
using System.Linq;
using ThomsonReuters.Languages;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using ISymbol = ThomsonReuters.Languages.ISymbol;

namespace ThomsonReuters.CodeGeneration
{
    public interface IOutputMediaTypes
    {
        void WriteInnerMethod(BaseWebApiQuerySymbol query);
        void WriteOverrideableMethod(BaseWebApiQuerySymbol query);
    }

    public class MediaTypeCodeGenerator
    {
        public static IOutputMediaTypes Select(
            Action<string> writer, 
            Func<TypeReference, Languages.ISymbol, string> typeNameFactory, 
            Func<QualifiedIdentifier, QualifiedIdentifier, QualifiedIdentifier> relativeNameFactory = null,
            string mediaType = null)
        {
            mediaType = mediaType ?? String.Empty;

            if (mediaType.ToLower() == "html")
                return new HtmlMediaTypeStrategy(writer, typeNameFactory);

            if (mediaType.ToLower() == "json")
                return new JsonMediaTypeStrategy(writer, typeNameFactory);

            if (mediaType.ToLower() == "resource")
                return new ApiResourceMediaTypeStrategy(writer, typeNameFactory, relativeNameFactory);

            return new DefaultMediaTypeStrategy(writer, typeNameFactory);
        }
    }

    public class HtmlMediaTypeStrategy : FilterAwareMediaTypeStrategy, IOutputMediaTypes
    {
        private readonly Action<string> _writer;

        public HtmlMediaTypeStrategy(Action<string> writer, Func<TypeReference, Languages.ISymbol, string> typeNameFactory)
            : base(typeNameFactory)
        {
            _writer = writer;
        }

        public void WriteInnerMethod(BaseWebApiQuerySymbol query)
        {
            _writer("if (acceptType.Any(x => x.Contains(\"html\") || x.Contains(\"*/*\")))");
            _writer("{");
            _writer(String.Format("    var viewData = On{0}_Html();", GetQueryName(query)));
            _writer("    return View(viewData.ViewName, viewData.ViewModel);");
            _writer("}");
        }

        public void WriteOverrideableMethod(BaseWebApiQuerySymbol query)
        {
            _writer(String.Format("public abstract ViewData On{0}_Html();", GetQueryName(query)));
        }
    }

    public abstract class FilterAwareMediaTypeStrategy
    {
        protected readonly Func<TypeReference, ISymbol, string> TypeNameFactory;

        protected FilterAwareMediaTypeStrategy(Func<TypeReference, Languages.ISymbol, string> typeNameFactory)
        {
            TypeNameFactory = typeNameFactory;
        }

        protected IEnumerable<ArgumentDefinition> GetQueryArguments(BaseWebApiQuerySymbol query)
        {
            //get query argument, if any, then append filters, if any
            return new[]{
                            query.Argument
                                .Select(x => new ArgumentDefinition(String.Format("{0} {1}", TypeNameFactory(x, query), x.Name), x.Name.ToString(), x.Cardinality))
                                .ValueOrDefault((ArgumentDefinition)null)}
                .Concat(query.Filters
                            .Select(x => new ArgumentDefinition(String.Format("{0} {1}", TypeNameFactory(x, query), x.Name), x.Name.ToString(), x.Cardinality)))
                .Where(x => x != null);
        }

        protected string GetQueryName(BaseWebApiQuerySymbol query)
        {
            return (query is WebApiQuerySymbol) ? query.Name.ToString() : "Index";
        }
    }

    public class DefaultMediaTypeStrategy : FilterAwareMediaTypeStrategy, IOutputMediaTypes
    {
        private readonly Action<string> _writer;

        public DefaultMediaTypeStrategy(Action<string> writer, Func<TypeReference, Languages.ISymbol, string> typeNameFactory)
            : base(typeNameFactory)
        {
            _writer = writer;
        }

        public void WriteInnerMethod(BaseWebApiQuerySymbol query)
        {
            var arguments = GetQueryArguments(query);

            _writer(String.Format("return new DataResult<Possible<{0}, Observation>>(On{1}({2}).AsPossible());",
                                    TypeNameFactory(query.Result, query),
                                    GetQueryName(query),
                                    arguments.Select(a => a.Name).Delimit(", ")));
        }

        public void WriteOverrideableMethod(BaseWebApiQuerySymbol query)
        {
            var arguments = GetQueryArguments(query);

            _writer(String.Format("public abstract Result<{0}, Observation> On{1}({2});",
                TypeNameFactory(query.Result, query),
                GetQueryName(query),
                arguments.Select(a => a.FullTypeDefinition).Delimit(", ")));
        }
    }

    public class JsonMediaTypeStrategy : FilterAwareMediaTypeStrategy, IOutputMediaTypes
    {
        private readonly Action<string> _writer;

        public JsonMediaTypeStrategy(Action<string> writer, Func<TypeReference, Languages.ISymbol, string> typeNameFactory)
            : base(typeNameFactory)
        {
            _writer = writer;
        }

        public void WriteInnerMethod(BaseWebApiQuerySymbol query)
        {
            var resultType = TypeNameFactory(query.Result, query);
            var arguments = GetQueryArguments(query);

            _writer("if (acceptType.Any(x => x.Contains(\"json\")))");
            _writer("{");
            _writer(String.Format("    return new DataResult<Possible<{0}, Observation>>(On{1}_Json({2}).AsPossible());",
                                        resultType,
                                        GetQueryName(query),
                                        arguments.Select(a => a.Name).Delimit(", ")));
            _writer("}");
        }

        public void WriteOverrideableMethod(BaseWebApiQuerySymbol query)
        {
            var resultType = TypeNameFactory(query.Result, query);
            var arguments = GetQueryArguments(query);

            _writer(String.Format("public abstract Result<{0}, Observation> On{1}_Json({2});",
                                    resultType,
                                    GetQueryName(query),
                                    arguments.Select(x => x.FullTypeDefinition).Delimit(", ")));
        }
    }

    public class ApiResourceMediaTypeStrategy : FilterAwareMediaTypeStrategy, IOutputMediaTypes
    {
        private readonly Action<string> _writer;
        private readonly Func<QualifiedIdentifier, QualifiedIdentifier, QualifiedIdentifier> _relativeNameFactory;

        public ApiResourceMediaTypeStrategy(Action<string> writer, Func<TypeReference, Languages.ISymbol, string> typeNameFactory, Func<QualifiedIdentifier, QualifiedIdentifier, QualifiedIdentifier> relativeNameFactory)
            : base(typeNameFactory)
        {
            _writer = writer;
            _relativeNameFactory = relativeNameFactory;
        }

        public void WriteInnerMethod(BaseWebApiQuerySymbol query)
        {
            var rawReturnType = TypeNameFactory(query.Result, query);
            var arguments = GetQueryArguments(query);

            var isResourceCollection = !query.Result.Cardinality.Maximum.HasValue ||
                                       query.Result.Cardinality.Maximum.Value > 1;

            var returnType = isResourceCollection
                                ? String.Format("ApiCollectionResource<{0}>", _relativeNameFactory(query.Result.Type.FullName, query.FullName))
                                : String.Format("ApiResource<{0}>", rawReturnType);

            _writer("if (acceptType.Any(x => x.Contains(\"vnd.thomsonreuters.apiresource\")))");
            _writer("{");
            _writer(String.Format("    return new DataResult<Possible<{0}, Observation>>(On{1}_Resource({2}).AsPossible());",
                            returnType,
                            GetQueryName(query),
                            arguments.Select(x => x.Name).Delimit(", ")));
            _writer("}");
        }

        public void WriteOverrideableMethod(BaseWebApiQuerySymbol query)
        {
            var rawReturnType = TypeNameFactory(query.Result, query);
            var arguments = GetQueryArguments(query);

            var isResourceCollection = !query.Result.Cardinality.Maximum.HasValue ||
                                       query.Result.Cardinality.Maximum.Value > 1;

            var returnType = isResourceCollection
                                ? String.Format("ApiCollectionResource<{0}>", _relativeNameFactory(query.Result.Type.FullName, query.FullName))
                                : String.Format("ApiResource<{0}>", rawReturnType);

            _writer(String.Format("public abstract Result<{0}, Observation> On{1}_Resource({2});",
                                     returnType,
                                     GetQueryName(query),
                                     arguments.Select(x => x.FullTypeDefinition).Delimit(", ")));
        }
    }

    public class ArgumentDefinition
    {
        public ArgumentDefinition(string fullTypeDefinition, string name, Cardinality cardinality)
        {
            FullTypeDefinition = fullTypeDefinition;
            Name = name;
            Cardinality = cardinality;
        }

        public string FullTypeDefinition { get; private set; }
        public string Name { get; private set; }
        public Cardinality Cardinality { get; private set; }
    }
}

