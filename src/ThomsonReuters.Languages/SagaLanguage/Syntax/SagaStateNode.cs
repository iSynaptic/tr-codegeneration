using System.Collections.Generic;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class SagaStateNode
    {
        public readonly string Name;
        public readonly string Modifier;
        public readonly IEnumerable<RequirementNode> Requirements;
        public readonly IEnumerable<MessageNode> Transitions;

        public SagaStateNode(string name, string modifier, IEnumerable<RequirementNode> requirements, IEnumerable<MessageNode> transitions)
        {
            Name = name;
            Modifier = modifier;
            Requirements = requirements;
            Transitions = transitions;
        }
    }
}