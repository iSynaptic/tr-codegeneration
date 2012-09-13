using System.IO;
using System.Linq;
using System.Reflection;
using ThomsonReuters.Languages.TypesLanguage;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public static class DescriptionProvider
    {
        public static Result<Compilation, string> GetDescription(string extension)
        {
            var thisAssembly = Assembly.GetCallingAssembly();

            var input = thisAssembly.GetManifestResourceNames().Where(x => x.EndsWith(extension))
                .Select(x =>
                            {
                                using (Stream stream = thisAssembly.GetManifestResourceStream(x))
                                using (var streamReader = new StreamReader(stream))
                                {
                                    return new TypesLanguageCompilerInput(streamReader.ReadToEnd(), x);
                                }
                            });

            return TypesLanguageCompiler.Compile(input, new [] {typeof (int).Assembly});
        }
    }
}