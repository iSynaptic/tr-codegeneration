using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public static class SagaDescriptionBuilder
    {
        public static Result<SagaDescription, string> Build(IEnumerable<SagaModuleNode> descriptionNodes)
        {
            return new Result<SagaDescription, string>(
                new SagaDescription(descriptionNodes.Select(BuildModule).ToArray()));
        }

        private static SagaModule BuildModule(SagaModuleNode node)
        {
            return new SagaModule(node.Sagas
                .Select(BuildSaga)
                .ToArray(),
                node.Namespace,
                node.ImportedNamespaces
                );
        }

        private static Saga BuildSaga(SagaNode node)
        {
            var states = node.States
                .Select(x => BuildState(x, node.Name))
                .ToArray();

            // resolve state names -> states
            var map = states.ToDictionary(x => x.Name);
            states.SelectMany(x => x.Transitions)
                .SelectMany(x => x.TransitionChoices)
                .Where(x => x.ResultingStateName != null)
                .Run(x => x.ResolveState(map[x.ResultingStateName]));

            return new Saga(node.Name, states);
        }

        private static SagaState BuildState(SagaStateNode node, string sagaName)
        {
            Guard.NotNull(node, "node");
            Guard.NotNull(sagaName, "sagaName");

            SagaStateModifier mod;
            switch (node.Modifier)
            {
                case null:
                    mod = SagaStateModifier.None;
                    break;
                case "initial":
                    mod = SagaStateModifier.Initial;
                    break;
                case "final":
                    mod = SagaStateModifier.Final;
                    break;
                default:
                    throw new ArgumentException(string.Format("SagaStateModifier '{0}' is not recognized", node.Modifier));
            }

            return new SagaState(node.Name, mod,
                node.Requirements
                    .Select(x => BuildRequirement(x, sagaName))
                    .ToArray(),
                node.Transitions
                    .Select(x => BuildTransition(x, sagaName))
                    .ToArray());
        }

        private static StateTransition BuildTransition(MessageNode node, string sagaName)
        {
            return new StateTransition(
                new StateMessage(node.Message, sagaName),
                node.TransitionChoices
                    .Select(x => BuildTransitionChoice(x, sagaName))
                    .ToArray()
                );
        }

        private static TransitionChoice BuildTransitionChoice(TransitionChoiceNode node, string sagaName)
        {
            IEnumerable<StateRequirement> requirements = null;
            string resultingStateName = null;

            if (node.Actions != null)
            {
                requirements = node.Actions.Requirements.Select(x => BuildRequirement(x, sagaName)).ToArray();
                resultingStateName = node.Actions.ResultingStateName;
            }

            return new TransitionChoice(node.Predicate, requirements, resultingStateName);
        }

        private static StateRequirement BuildRequirement(RequirementNode node, string sagaName)
        {
            if (node.Type != RequirementType.Send)
                throw new ArgumentException(string.Format("Requirement type {0} is not recognized", node.Type));

            return new StateRequirement(new StateMessage(node.MessageName, sagaName));
        }
    }
}
