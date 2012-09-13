namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class RequirementNode
    {
        public readonly RequirementType Type;
        public readonly string MessageName;

        public RequirementNode(RequirementType type, string messageName)
        {
            Type = type;
            MessageName = messageName;
        }
    }
}