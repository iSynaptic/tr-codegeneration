using Microsoft.Build.Utilities;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public interface ICodeGenerator
    {
        bool Generate(string[] parameters, string[] inputs, string outputDir, string filename, TaskLoggingHelper log, out string include);
    }
}