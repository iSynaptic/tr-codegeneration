namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class TransitionChoiceNode
    {
        public readonly string Predicate;
        public readonly TransitionActionsNode Actions;

        public TransitionChoiceNode(string predicate, TransitionActionsNode actions)
        {
            Predicate = predicate;
            Actions = actions;
        }
    }
}