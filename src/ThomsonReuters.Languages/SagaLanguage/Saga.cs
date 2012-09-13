using System;
using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.SagaLanguage
{
    public class SagaDescription
    {
        public readonly IEnumerable<SagaModule> Modules;

        public SagaDescription(IEnumerable<SagaModule> modules)
        {
            Modules = modules;
        }
    }

    public class SagaModule
    {
        public readonly IEnumerable<Saga> Sagas;
        public readonly IEnumerable<string> ImportedNamespaces;
        public readonly string Namespace;

        public SagaModule(IEnumerable<Saga> sagas, string ns, IEnumerable<string> importedNamespaces)
        {
            Sagas = sagas;
            Namespace = ns;
            ImportedNamespaces = importedNamespaces;
        }
    }

    public class Saga
    {
        public readonly string Name;
        public readonly IEnumerable<SagaState> States;

        public Saga(string name, IEnumerable<SagaState> states)
        {
            Name = name;
            States = states;
        }
    }

    public enum SagaStateModifier
    {
        None = 0,
        Initial,
        Final
    }

    public class SagaState
    {
        public readonly string Name;
        public readonly SagaStateModifier Modifier;
        public readonly IEnumerable<StateRequirement> Requirements;
        public readonly IEnumerable<StateTransition> Transitions;

        public SagaState(string name, SagaStateModifier modifier, IEnumerable<StateRequirement> requirements, IEnumerable<StateTransition> transitions)
        {
            Name = name;
            Modifier = modifier;
            Requirements = requirements;
            Transitions = transitions;
        }
    }

    public enum RequirementType
    {
        Send    
    }

    public class StateRequirement
    {
        public readonly RequirementType Type;
        public readonly StateMessage Message;

        public StateRequirement(StateMessage message)
        {
            Type = RequirementType.Send;
            Message = message;
        }
    }

    public class StateTransition
    {
        public readonly StateMessage Message;
        public readonly IEnumerable<TransitionChoice> TransitionChoices;

        public StateTransition(StateMessage message, IEnumerable<TransitionChoice> transitionChoices)
        {
            Message = message;
            TransitionChoices = transitionChoices;
        }
    }

    public class StateMessage
    {
        public readonly string ShortName;
        public readonly string FullName;

        public StateMessage(string composedName, string sagaName)
        {
            FullName = composedName.Replace("__", sagaName);
            ShortName = composedName.Replace("__", "");
        }
    }

    public class TransitionChoice
    {
        public readonly string Predicate;
        public readonly IEnumerable<StateRequirement> Requirements;
        internal readonly string ResultingStateName;
        public SagaState ResultingState { get; private set; }

        public TransitionChoice(string predicate, IEnumerable<StateRequirement> requirements, string resultingStateName)
        {
            Predicate = predicate;
            Requirements = requirements;
            ResultingStateName = resultingStateName;
        }

        internal void ResolveState(SagaState state)
        {
            Guard.NotNull(state, "state");
            if (ResultingState != null)
                throw new InvalidOperationException("State has already been resolved");
            ResultingState = state;
        }
    }
}
