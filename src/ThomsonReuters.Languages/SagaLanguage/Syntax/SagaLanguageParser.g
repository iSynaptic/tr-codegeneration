namespace ThomsonReuters.Languages.SagaLanguage.Syntax:
  import System;
  import System.Collections.Generic;
  import System.Linq;
  
  import iSynaptic.Commons;
  import iSynaptic.Commons.Linq;

  import MetaSharp.Transformation;
  import MetaSharp.Transformation.Lang;

  grammar SagaLanguageParser < Parser:
    Main = Module;
	
	Module =	ns:Namespace?
				namespaceImports:NamespaceImport*
				sagas:Saga* ->
				{
					return new SagaModuleNode(AsSequence<SagaNode>(sagas), ns as string, AsSequence<string>(namespaceImports));
				}
					
	Namespace =	InKeyword NamespaceKeyword ns:NamespaceIdentifier -> ns;

	NamespaceImport = ImportKeyword NamespaceKeyword ns:NamespaceIdentifier -> ns;

	NamespaceIdentifier = Identifier ("." i:Identifier -> i)* -> AsSequence<string>(Flatten(match)).Delimit(".");
	
    Saga =	SagaKeyword
			name:Identifier
			states:State* ->
			{
				return new SagaNode(name as string, AsSequence<SagaStateNode>(states));
			}

	State =	modifier:StateModifier?
			StateKeyword
			name:Identifier
			requirements:Requirements?
			messages:Message* ->
			{
				return new SagaStateNode(	name as string,
											modifier as string,
											AsSequence<RequirementNode>(requirements),
											AsSequence<MessageNode>(messages));  
			}

	StateModifier =	InitialKeyword
				  | FinalKeyword;

	Requirements =	RequiresKeyword
					list:RequirementList -> list;

	RequirementList = Requirement ("," r:Requirement -> r)* -> Flatten(match);

	Requirement =	SendKeyword "(" i:Identifier ")" ->
					{
						return new RequirementNode(RequirementType.Send, i as string);
					}

	Message =	AcceptsKeyword
				composableMessageName:Identifier
				choices:TransitionChoiceList? ->
				{
					return new MessageNode(composableMessageName as string, AsSequence<TransitionChoiceNode>(choices));
				}

	TransitionChoiceList =	(t:TransitionActions -> new TransitionChoiceNode(null, t as TransitionActionsNode)) 
						 |  (TransitionChoices -> Flatten(match))
					     ;
						
	TransitionChoices =	choices:PredicatedChoice+
						otherwise:DefaultChoice? ->
						{
							return AsSequence<TransitionChoiceNode>(choices).Concat(AsSequence<TransitionChoiceNode>(otherwise));
						}

	DefaultChoice =	OtherwiseKeyword
					actions:TransitionActions ->
					{
						return new TransitionChoiceNode(null, actions as TransitionActionsNode);
					}

	PredicatedChoice =	WhenKeyword
						predicate:StringLiteral
						actions:TransitionActions ->
						{
							return new TransitionChoiceNode(predicate as string, actions as TransitionActionsNode);
						}

    TransitionActions =	(requirements:Requirements GoesToKeyword state:Identifier -> new TransitionActionsNode(AsSequence<RequirementNode>(requirements), state as string))
					  | (requirementsOnly:Requirements -> new TransitionActionsNode(AsSequence<RequirementNode>(requirementsOnly), null))
					  | (stateOnly:(GoesToKeyword s:Identifier -> s) -> new TransitionActionsNode(new RequirementNode[0], stateOnly as string))
					  ;
				   
	IdentifierList = Identifier ("," i:Identifier -> i)* -> Flatten(match);

    Keywords = SagaKeyword
			 | SendKeyword
			 | StateKeyword
			 | ImportKeyword
			 | InKeyword
			 | InitialKeyword
			 | NamespaceKeyword
			 | FinalKeyword
			 | AcceptsKeyword
             | RequiresKeyword
			 | GoesToKeyword
			 | WhenKeyword
			 | OtherwiseKeyword
			 ;

	[Keyword] AcceptsKeyword = "receives";
    [Keyword] FinalKeyword = "final";
	[Keyword] GoesToKeyword = "->";
	[Keyword] ImportKeyword = "import";
	[Keyword] InKeyword = "in";
	[Keyword] InitialKeyword = "initial";
	[Keyword] NamespaceKeyword = "namespace";
	[Keyword] OtherwiseKeyword = "otherwise";
    [Keyword] RequiresKeyword = "requires";
    [Keyword] SagaKeyword = "saga";
	[Keyword] SendKeyword = "send";
    [Keyword] StateKeyword = "state";
	[Keyword] WhenKeyword = "when";

	override CustomTokens = '-' '>' | super;
  end
end