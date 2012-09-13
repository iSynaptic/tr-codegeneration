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

namespace ThomsonReuters.CodeGeneration
{
    public class WebApiResourceKeysCodeGenerator : CodeGenerator
    {
        public WebApiResourceKeysCodeGenerator(StringBuilder sb) : base(sb)
        {
        }

        public WebApiResourceKeysCodeGenerator(TextWriter writer) : base(writer)
        {
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
            VisitBaseWebApiPath(webApi, base.VisitWebApi);
        }

        public override void VisitWebApiPath(WebApiPathSymbol path)
        {
            VisitBaseWebApiPath(path, base.VisitWebApiPath);
        }

        private void VisitBaseWebApiPath<T>(T path, Action<T> visit)
            where T : BaseWebApiPathSymbol
        {
            if (!HasChildrenWithKeys(path)) return;

            if(path is WebApiSymbol)
            {
                WriteLine("public static class {0}ResourceKeys", path.Name);
                using (WithBlock())
                {
                    WriteLine("private const string WebApiMountLocation = \"{0}\";",
                              path.Annotations
                                  .TryGetValue("MountedAt")
                                  .Select(x => x.SingleOrDefault())
                                  .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Path"))
                                  .Select(x => x.Value.Value.ToLower())
                                  .ValueOrDefault(""));
                    
                    WritePathKeys(path);
                }
            }
        }

        private void WritePathKeys<T>(T path)
            where T:BaseWebApiPathSymbol
        {
            if (!(path is WebApiSymbol)
                && (ShouldGenerateKey(path) || HasChildrenWithKeys(path)) )
            {
                WriteLine("public static class {0}", path.Name);
                using (WithBlock())
                {
                    WriteLine("public const string Path = WebApiMountLocation + \"/{0}/\";", GetPathInfo(path));
                    BuildKey(path);
                    path.Members.OfType<WebApiQuerySymbol>().Run(BuildKey);
                    path.Members.OfType<BaseWebApiPathSymbol>().Run(WritePathKeys);
                }
            }
            path.Members.OfType<BaseWebApiPathSymbol>().Run(WritePathKeys);
        }

        private void BuildKey<T>(T path) where T : BaseWebApiQuerySymbol
        {
            if (ShouldGenerateKey(path))
            {
                WritePathKey(path);

                if(path.Filters.Any())
                    WriteFilteredKey(path);
            }
        }

        private void WritePathKey<T>(T path) where T : BaseWebApiQuerySymbol
        {
            var keyName = (path is WebApiQuerySymbol) ? path.Name.ToString() : "Default";
            var argumentName = path.Argument
                .Select(x => GetPublicTypeString(x, path) + " " + x.Name)
                .ValueOrDefault("");
            var argumentOperation = path.Argument
                .Select(x => String.Format(" + \"/\" + {0}.ToString()", x.Name))
                .ValueOrDefault("");
            
            WriteLine("public static ApiResourceKey {0}({1})", keyName, argumentName);
            using (WithBlock())
            {
                WriteLine("return new ApiResourceKey(WebApiMountLocation + \"/{0}\"{1});", GetPathInfo(path), argumentOperation);
            }
        }

        private string GetPathInfo<T>(T path) where T:BaseWebApiQuerySymbol
        {
            Func<BaseWebApiQuerySymbol, Maybe<BaseWebApiQuerySymbol>> parentSelector = p => 
                p.ToMaybe<WebApiPathSymbol>().Select(x => x.Parent).OfType<BaseWebApiQuerySymbol>();

            var ancestors = new List<string>();
            if(path is WebApiPathSymbol)
                path.Recurse(parentSelector).Reverse().Skip(1)//skip the parent webapi node
                    .Run(p => ancestors.Add(p.Name.ToString().ToLower()));
            else if(path is WebApiQuerySymbol)
            {
                var query = path as WebApiQuerySymbol;
                query.Parent.Recurse(parentSelector).Reverse().Skip(1)//skip the parent webapi node
                    .Run(p => ancestors.Add(p.Name.ToString().ToLower()));
                ancestors.Add(query.Name.ToString().ToLower());
            }

            return ancestors.Delimit("/");
        }

        private void WriteFilteredKey<T>(T path) where T : BaseWebApiQuerySymbol
        {
            var argument = path.Argument
                .Select(x => GetPublicTypeString(x, path) + " " + x.Name)
                .ValueOrDefault("");

            path.Filters.Run(filter =>
            {
                var filterOperation = String.Format(" + \"?{0}=\" + {1}.ToString()", filter.Name.ToString().ToLower(), filter.Name);

                WriteLine("public static ApiResourceKey {0}({1}{2}{3})", Inflector.Pascalize(filter.Name), argument, path.Argument.HasValue ? ", " : "", GetPublicTypeString(filter, path) + " " + filter.Name);
                using (WithBlock())
                {
                    WriteLine("return new ApiResourceKey(WebApiMountLocation + \"/{0}\"{1});", GetPathInfo(path), filterOperation);
                }
            });
        }

        private bool ShouldGenerateKey(BaseWebApiQuerySymbol querySymbol)
        {
            return !querySymbol.Result.Type.IsVoid()
                && querySymbol.Annotations.TryGetValue("MediaTypes")
                    .Select(x => x.SingleOrDefault())
                    .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Supported"))
                    .Select(x => x.Value)
                    .Select(x => x.Value.Contains("resource"))
                    .ValueOrDefault();
        }

        private bool HasChildrenWithKeys(BaseWebApiPathSymbol path)
        {
            return
                path.Members.OfType<WebApiOperationSymbol>()
                    .Recurse((Func<WebApiOperationSymbol, IEnumerable<WebApiOperationSymbol>>)
                        (x => x.ToMaybe<BaseWebApiPathSymbol>()
                        .Select(y => y.Members.OfType<WebApiOperationSymbol>())
                        .ValueOrDefault(Enumerable.Empty<WebApiOperationSymbol>())))
                    .Any(symbol => //does any child have [MediaTypes(Supported:"resource")] 
                    {
                        var resourceAnnotation = symbol.Annotations
                            .TryGetValue("MediaTypes")
                            .Select(x => x.SingleOrDefault())
                            .SelectMaybe(x => x.Pairs.TrySingle(y => y.Key == "Supported"))
                            .Select(x => x.Value);

                        return resourceAnnotation.HasValue
                           && resourceAnnotation.Value.Value.Contains("resource");
                    });
        }
    }
}
