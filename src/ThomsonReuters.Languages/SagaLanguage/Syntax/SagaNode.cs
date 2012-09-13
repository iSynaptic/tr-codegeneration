using System.Collections.Generic;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class SagaNode
    {
        public readonly string Name;
        public readonly IEnumerable<SagaStateNode> States;

        public SagaNode(string name, IEnumerable<SagaStateNode> states)
        {
            Name = name;
            States = states;
        }
    }
}