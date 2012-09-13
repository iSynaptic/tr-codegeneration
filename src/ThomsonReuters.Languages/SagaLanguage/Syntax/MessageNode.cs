using System.Collections.Generic;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class MessageNode
    {
        public readonly string Message;
        public readonly IEnumerable<TransitionChoiceNode> TransitionChoices;

        public MessageNode(string message, IEnumerable<TransitionChoiceNode> transitionChoices)
        {
            Message = message;
            TransitionChoices = transitionChoices;
        }
    }
}