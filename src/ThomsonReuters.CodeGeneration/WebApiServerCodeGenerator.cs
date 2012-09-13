using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.CodeGeneration
{
    public class WebApiServerCodeGenerator : CodeGenerator
    {
        public WebApiServerCodeGenerator(StringBuilder sb) : base(sb)
        {
        }

        public WebApiServerCodeGenerator(TextWriter writer)
            : base(writer)
        {
        }

        protected override void AddUsings()
        {
            base.AddUsings();

            AddUsing("System.Web.Mvc");
            AddUsing("iSynaptic.Commons");
            AddUsing("ThomsonReuters.CodeGeneration");
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

        protected bool HasVisitableExecutableOperations(BaseWebApiPathSymbol path)
        {
            var paths = path.Recurse(x => x.Members.OfType<BaseWebApiPathSymbol>()).ToArray();
            return paths.OfType<WebApiOperationSymbol>()
                .Concat(paths.SelectMany(x => x.Members.OfType<WebApiCommandSymbol>()))
                .Concat(paths.SelectMany(x => x.Members.OfType<WebApiQuerySymbol>()))
                .Where(x => !NotInterestedIn(x))
                .Any(x => !(x is WebApiPathSymbol) || !x.Result.Type.IsVoid());
        }

        public override void VisitWebApi(WebApiSymbol webApi)
        {
            if (!HasVisitableExecutableOperations(webApi))
                return;

            VisitBaseWebApiPath(webApi);
        }

        public override void VisitWebApiPath(WebApiPathSymbol path)
        {
            VisitBaseWebApiPath(path);
        }

        protected virtual void VisitBaseWebApiPath<T>(T path) where T : BaseWebApiPathSymbol
        {
            if (!HasVisitableExecutableOperations(path))
                return;

            using (WriteBlock("public abstract class Base{0}Controller : Controller", path.Name))
            {
                VisitBaseWebApiQuery(path);
                Visit(path.Members.Where(x => !(x is WebApiPathSymbol)));
            }

            WriteLine();

            WriteLine("public partial class {0}Controller : Base{0}Controller {{ }}", path.Name);
            WriteLine();

            var childPaths = path
                .Members
                .OfType<WebApiPathSymbol>()
                .ToArray();

            if (childPaths.Length > 0)
            {
                using (WriteBlock("namespace {0}", path.Name))
                {
                    Visit(childPaths.AsEnumerable());
                }
            }
        }

        public override void VisitWebApiCommand(WebApiCommandSymbol command)
        {
            var coercedReturnShape = command.Result.Type.IsVoid()
                ? "Answer"
                : "Possible";

            var publicReturnShape = command.Result.Type.IsVoid()
                ? "Answer<Observation>"
                : String.Format("Possible<{0}, Observation>", GetPublicTypeString(command.Result, command));

            var internalReturnType = command.Result.Type.IsVoid()
                ? "Outcome<Observation>"
                : String.Format("Result<{0}, Observation>", GetPublicTypeString(command.Result, command));

            var argument = command.Argument;
            var parentArgument = command.Parent.Argument;

            var arguments = new[] { argument, parentArgument };

            if (!command.Annotations.Any(x => x.Key == "Internal"))
            {
                using (WriteBlock("public ViewResult {0}()", command.Name))
                {
                    WriteLine("var viewData = On{0}_Html();", command.Name);
                    WriteLine("return View(viewData.ViewName, viewData.ViewModel);");
                }

                WriteLine();
                WriteLine("public abstract ViewData On{0}_Html();", command.Name);
            }

            WriteLine("[HttpPost]");
            WriteLine("public ActionResult {0}({1})",
                command.Name,
                arguments.Squash().Delimit(", ", x => String.Format("{0} {1}", GetPublicTypeString(x, command), x.Name)));

            using (WithBlock())
            {
                WriteLine("return new DataResult<{0}>(On{1}({2}).As{3}());",
                    publicReturnShape,
                    command.Name,
                    arguments.Squash().Select(x => x.Name).Delimit(", "),
                    coercedReturnShape);
            }
            WriteLine("public abstract {0} On{1}({2});",
                internalReturnType,
                command.Name,
                arguments.Squash().Select(x => String.Format("{0} {1}", GetPublicTypeString(x, command), x.Name)).Delimit(", "));
        }

        public override void VisitWebApiQuery(WebApiQuerySymbol query)
        {
            VisitBaseWebApiQuery(query);
        }

        private void VisitBaseWebApiQuery(BaseWebApiQuerySymbol query)
        {
            if (query.Result.Type.IsVoid())
                return;

            var queryName = query is BaseWebApiPathSymbol ? "Index" : query.Name.ToString();
            var arguments = GetQueryArguments(query);

            var mediaTypesAnnotation = query.Annotations
                .TryGetValue("MediaTypes")
                .Select(x => x.SingleOrDefault())
                .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Supported"))
                .Select(x => x.Value);

            var specifiedMediaTypes = mediaTypesAnnotation.HasValue
                ? mediaTypesAnnotation.Value.Value.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray()
                : Enumerable.Empty<string>().ToArray();

            WriteLine("public ActionResult {0}({1})", queryName, arguments.Select(x => x.FullTypeDefinition).Delimit(", "));

            using (WithBlock())
            {
                if (specifiedMediaTypes.Any())
                {
                    WriteLine("var acceptType = ControllerContext.HttpContext.Request.AcceptTypes ?? new string[]{};");

                    specifiedMediaTypes
                        .Run(m => MediaTypeCodeGenerator.Select(WriteLine, GetPublicTypeString, GetRelativeName, m).WriteInnerMethod(query));

                    WriteLine("ControllerContext.HttpContext.Response.StatusCode = 415;");
                    WriteLine("return new EmptyResult();");
                }
                else
                    MediaTypeCodeGenerator.Select(WriteLine, GetPublicTypeString).WriteInnerMethod(query);
            }

            if (specifiedMediaTypes.Any())
                specifiedMediaTypes.Run(m => MediaTypeCodeGenerator.Select(WriteLine, GetPublicTypeString, GetRelativeName, m).WriteOverrideableMethod(query));
            else
                MediaTypeCodeGenerator.Select(WriteLine, GetPublicTypeString).WriteOverrideableMethod(query);
        }

        private IEnumerable<ArgumentDefinition> GetQueryArguments(BaseWebApiQuerySymbol query)
        {
            //get query argument, if any, then append filters, if any
            return new[]{
                            query.Argument
                                .Select(x => new ArgumentDefinition(String.Format("{0} {1}", GetPublicTypeString(x, query), x.Name), x.Name.ToString(), x.Cardinality))
                                .ValueOrDefault((ArgumentDefinition)null)}
                .Concat(query.Filters
                            .Select(x => new ArgumentDefinition(String.Format("{0} {1}", GetPublicTypeString(x, query), x.Name), x.Name.ToString(), x.Cardinality)))
                .Where(x => x != null);
        }
    }
}