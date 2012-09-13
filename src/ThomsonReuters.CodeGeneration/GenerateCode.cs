using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace ThomsonReuters.CodeGeneration
{
    public class GenerateCode : Task
    {
        private static readonly IEnumerable<string> NoInputs = new string[0];

        [Output]
        public string Include { get; private set; }

        [Required]
        public string AssemblyPath { get; set; }

        [Required]
        public string GeneratorName { get; set; }

        [Required]
        public string OutputDir { get; set; }

        [Required]
        public string OutputFilename { get; set; }

        public string[] Parameters { get; set; }

        public string[] Inputs { get; set; }

        public override bool Execute()
        {
            try
            {
                var inputTimes = new[] { AssemblyPath }
                    .Concat(Inputs ?? NoInputs)
                    .Select(File.GetLastWriteTimeUtc)
                    .ToArray();
                var outputTime = File.GetLastWriteTimeUtc(Path.Combine(OutputDir, OutputFilename));

                if (inputTimes.All(x => x <= outputTime))
                    return true;

                var domain = AppDomain.CreateDomain("GenerateCodeDomain");
                try
                {
                    var generator = (ICodeGenerator)domain.CreateInstanceFromAndUnwrap(AssemblyPath, GeneratorName);
                    string include;
                    var result = generator.Generate(Parameters, Inputs, OutputDir, OutputFilename, Log, out include);
                    Include = include;
                    return result;
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
            }
            catch (Exception x)
            {
                Log.LogErrorFromException(x);
                return false;
            }
        }
    }
}
