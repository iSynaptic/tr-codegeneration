using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;
using ISymbol = ThomsonReuters.Languages.ISymbol;

namespace ThomsonReuters.CodeGeneration
{
    public class WebApiClientCodeGenerator : CodeGenerator
    {
        private readonly bool _generateInternal;
        private string _webApiMountLocation;

        public WebApiClientCodeGenerator(StringBuilder sb, bool generateInternal) 
            : base(sb)
        {
            _generateInternal = generateInternal;
        }

        public WebApiClientCodeGenerator(TextWriter writer, bool generateInternal)
            : base(writer)
        {
            _generateInternal = generateInternal;
        }

        protected override bool NotInterestedIn(object subject)
        {
            var interestedIn = new[]
            {
                typeof(Compilation),
                typeof(NamespaceSymbol),
                typeof(WebApiSymbol),
                typeof(WebApiPathSymbol),
                typeof(WebApiQuerySymbol),
                typeof(WebApiCommandSymbol),
            };

            return !(interestedIn.Contains(subject.GetType()))
               || base.NotInterestedIn(subject);
        }

        public override void VisitWebApi(WebApiSymbol webApi)
        {
            _webApiMountLocation = webApi.Annotations
                .TryGetValue("MountedAt")
                .Select(x => x.SingleOrDefault())
                .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Path"))
                .Select(x => x.Value.Value)
                .ValueOrDefault("");

            VisitBaseWebApiPath(webApi, "Client", base.VisitWebApi);
        }

        public override void VisitWebApiPath(WebApiPathSymbol path)
        {
            VisitBaseWebApiPath(path, "Path", base.VisitWebApiPath);
        }

        private void VisitBaseWebApiPath<T>(T path, string suffix, Action<T> visit)
            where T : BaseWebApiPathSymbol
        {
            var internalAnnotation = path.Annotations
                .TryGetValue("Internal")
                .Select(x => x.SingleOrDefault());

            var hasPublicChildren = path.Members.OfType<WebApiOperationSymbol>()
                .Recurse((Func<WebApiOperationSymbol, IEnumerable<WebApiOperationSymbol>>)
                    (x => x.ToMaybe<BaseWebApiPathSymbol>()
                            .Select(y => y.Members.OfType<WebApiOperationSymbol>())
                            .ValueOrDefault(Enumerable.Empty<WebApiOperationSymbol>())))
                .Any(x => !x.Annotations.TryGetValue("Internal").HasValue);

            if (!_generateInternal)
            {
                if (internalAnnotation.HasValue) return;
                if (path.Result.Type.IsVoid() && !hasPublicChildren) return;
            }
          
            bool isParameterized = path.Argument.HasValue;
            using (WriteBlock("public{0}class {1}{2}{3}", _generateInternal ? " partial " : " ", path.Name, suffix, !isParameterized ? " : WebApiClient" : ""))
            {
                if(path is WebApiSymbol) 
                {
                    WriteLine("protected override string WebApiMountLocation {{ get {{ return \"{0}\"; }} }}", _webApiMountLocation);
                    WriteLine();
                }

                if (!isParameterized)
                {
                    bool isApi = path is WebApiSymbol;

                    WriteLine("public {0}{1}(Uri rootUrl{2}, int userId, string password, bool isSandboxMode, WebApiClient parent = null)", path.Name, suffix, isApi ? "" : ", string relativeUrl");
                    WriteLine(" : base(rootUrl, {0}, userId, password, isSandboxMode, parent) {{ }}", isApi ? "\"\"" : "relativeUrl");
                }
                else
                {
                    WriteLine("private readonly Uri _rootUrl;");
                    WriteLine("private readonly string _relativeUrl;");
                    WriteLine("private readonly WebApiClient _parent;");
                    WriteLine("private readonly int _userId;");
                    WriteLine("private readonly string _password;");
                    WriteLine("private readonly bool _isSandboxMode;");
                    WriteLine("public {0}{1}(Uri rootUrl, string relativeUrl, int userId, string password, bool isSandboxMode, WebApiClient parent) {{ if(rootUrl == null) throw new ArgumentNullException(\"rootUrl\"); _rootUrl = rootUrl; if(relativeUrl == null) throw new ArgumentNullException(\"relativeUrl\"); _relativeUrl = relativeUrl; _userId = userId; if(password == null) throw new ArgumentNullException(\"password\"); _password = password; _isSandboxMode = isSandboxMode; _parent = parent; }}", path.Name, suffix);
                }

                WriteLine();

                if (isParameterized)
                    WriteParameterizedPathBody(path, visit, suffix);
                else
                    WritePathBody(path, visit);

                WriteHideMembers();
            }

            if (!(path is WebApiSymbol))
            {
                WriteLine();
                WriteLine("public {0}Path {0} {{ get {{ return new {0}{1}(RootUrl, RelativeUrl + \"/{0}\", UserId, Password, IsSandboxMode, this); }} }}", path.Name, suffix);
            }
 
            WriteLine();
        }

        private void WritePathBody<T>(T path, Action<T> visit)
            where T : BaseWebApiPathSymbol
        {
            visit(path);

            if(!path.Result.Type.IsVoid())
                WriteOperation(path);
        }

        private void WriteParameterizedPathBody<T>(T path, Action<T> visit, string suffix)
            where T : BaseWebApiPathSymbol
        {
            var argument = path.Argument.Value;
            var argumentType = GetPublicTypeString(argument, path);

            using(WriteBlock("public class {0}{1}Implementation : WebApiClient", path.Name, suffix))
            {
                bool isApi = path is WebApiSymbol;

                WriteLine("public {0}{1}Implementation(Uri rootUrl{2}, int userId, string password, bool isSandboxMode, WebApiClient parent, {3} {4})", path.Name, suffix, isApi ? "" : ", string relativeUrl", argumentType, argument.Name);
                WriteLine(" : base(rootUrl, {0}{1}.ToString(), userId, password, isSandboxMode, parent) {{ }}",
                    isApi ? "" : "relativeUrl + \"/\" + ", argument.Name);

                WriteLine();

                WritePathBody(path, visit);

                WriteHideMembers();
            }

            WriteLine("public {0}{1}Implementation this[{2} {3}] {{ get {{ return new {0}{1}Implementation(_rootUrl, _relativeUrl, _userId, _password, _isSandboxMode, _parent, {3}); }} }}", path.Name, suffix, argumentType, argument.Name);
        }

        public override void VisitWebApiCommand(WebApiCommandSymbol command)
        {
            WriteOperation(command);
        }

        public override void VisitWebApiQuery(WebApiQuerySymbol query)
        {
            WriteOperation(query);
        }

        private void WriteOperation(WebApiOperationSymbol operation)
        {
            var internalAnnotation = operation.Annotations
                .TryGetValue("Internal")
                .Select(x => x.SingleOrDefault());

            if (!_generateInternal && internalAnnotation.HasValue) return;

            var queryType = GetReturnTypeString(operation.Result, operation);

            var filters = operation
                .ToMaybe<BaseWebApiQuerySymbol>()
                .Select(x => x.Filters)
                .Where(x => x.Any())
                .Select(x => x.Delimit(", ", y => string.Format("{0} {1} = default({0})", GetPublicTypeString(y, operation), y.Name)))
                .ValueOrDefault();

            var argument = operation.Argument.Where(x => !(operation is WebApiPathSymbol));

            WriteLine("public {0} {1}({2}{3})", queryType,
                      operation is WebApiPathSymbol ? "Get" : operation.Name.ToString(),
                      argument.Select(x => string.Format("{0} {1}", GetPublicTypeString(x), x.Name)).ValueOrDefault(""),
                      filters != null ? (argument.HasValue ? ", " : "") + filters : "");
            using (WithBlock())
            {
                WriteInvokeCall(operation);
            }
        }

        private void WriteInvokeCall(WebApiOperationSymbol operation)
        {
            var nilTypeName = GetRelativeName(new QualifiedIdentifier("ThomsonReuters.CodeGeneration.Nil"), operation.FullName);

            var returnType = operation.Result.Type.IsVoid()
                ? nilTypeName.ToString()
                : GetPublicTypeString(operation.Result, operation);

            var bodyType = operation.ToMaybe<WebApiCommandSymbol>()
                .SelectMaybe(x => x.Argument)
                .Select(x => GetPublicTypeString(x, operation))
                .ValueOrDefault(nilTypeName);

            var operationType = operation is WebApiCommandSymbol
                                    ? "WebApiOperationType.Command"
                                    : "WebApiOperationType.Query";

            var bodyValue = operation.ToMaybe<WebApiCommandSymbol>()
                .SelectMaybe(x => x.Argument)
                .Select(x => string.Format("{0}", x.Name))
                .ValueOrDefault(string.Format("default({0})", bodyType));

            var url = operation.ToMaybe().Unless(x => x is WebApiPathSymbol).Select(x => string.Format("\"/{0}\"", x.Name)).ValueOrDefault("\"\"");

            WriteLine("string url = {0};", url);

            var filters = operation
                .ToMaybe<BaseWebApiQuerySymbol>()
                .Select(x => x.Filters)
                .Select(x => x.ToArray())
                .ValueOrDefault();

            if(filters != null && filters.Length > 0)
            {
                WriteLine();
                WriteLine("bool isFirstFilter = true;");
                WriteLine();

                foreach (var filter in filters)
                {
                    WriteLine("if({0} != null)", filter.Name);
                    using(WithBlock())
                    {
                        WriteLine("url += (isFirstFilter ? \"?{0}=\" : \"&{0}=\") + Uri.EscapeDataString({0}.ToString());", filter.Name);
                        WriteLine("isFirstFilter = false;");
                    }

                    WriteLine();
                }
            }

            WriteLine("return Invoke<{0}, {1}>({2}, RelativeUrl + url, {3});", returnType, bodyType, operationType, bodyValue);
        }

        public virtual string GetReturnTypeString(TypeReference reference, ISymbol relativeTo = null)
        {
            var typeString = GetPublicTypeString(reference, relativeTo);
            return reference.Type.IsVoid()
                ? "Answer<Observation>"
                : string.Format("Possible<{0}, Observation>", typeString);
        }
    }
}
