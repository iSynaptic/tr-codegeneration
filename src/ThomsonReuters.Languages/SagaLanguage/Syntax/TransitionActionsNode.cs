using System.Collections.Generic;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class TransitionActionsNode
    {
        public readonly IEnumerable<RequirementNode> Requirements;
        public readonly string ResultingStateName;

        public TransitionActionsNode(IEnumerable<RequirementNode> requirements, string resultingStateName)
        {
            Requirements = requirements;
            ResultingStateName = resultingStateName;
        }
    }
}