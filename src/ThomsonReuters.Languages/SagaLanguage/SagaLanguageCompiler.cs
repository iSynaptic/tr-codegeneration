using System.Linq;
using ThomsonReuters.Languages.SagaLanguage.Syntax;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.SagaLanguage
{
    public class SagaLanguageCompiler
    {
        public static Result<SagaDescription, string> Compile(params string[] sagasInput)
        {
            Guard.NotNull(sagasInput, "sagasInput");

            var results = sagasInput
                .Select(x => new SagaLanguageParser().Parse(x))
                .ToArray();

            var outcome = results.Aggregate(new Outcome<string>(), (a,x) => a & x.ToOutcome().InformWithFormattedErrors("saga input"));

            if (!outcome.WasSuccessful)
                return outcome.ToResult();

            return SagaDescriptionBuilder.Build(results.Select(x => x.Value).ToArray());            
        }
    }
}
