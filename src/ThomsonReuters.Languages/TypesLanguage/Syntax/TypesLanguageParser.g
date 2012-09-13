namespace ThomsonReuters.Languages.TypesLanguage.Syntax:
  import System;
  import System.Collections.Generic;
  import System.Linq;
  
  import iSynaptic.Commons;
  import iSynaptic.Commons.Linq;

  import MetaSharp.Transformation;
  import MetaSharp.Transformation.Lang;

  grammar TypesLanguageParser < Parser:
    Main = Tree;

    override CustomTokens = '.' '.' | super;

    Tree = usings:UsingStatement*
           namespaces:NamespaceDeclaration* ->
           {
                return new SyntaxTree(usings.Cast<QualifiedIdentifier>(),
                                      namespaces.Cast<NamespaceSyntax>());
           }

    NamespaceDeclaration = NamespaceKeyword
                           name:QualifiedIdentifier
                           BlockBegin
                           usings:UsingStatement*
                           members:NamespaceMember*
                           error unless BlockEnd -> 
                           {
                               return new NamespaceSyntax(name as QualifiedIdentifier,
                                                           usings.Cast<QualifiedIdentifier>(),
                                                           members.Cast<INamespaceSyntaxMember>());
                           }

	NamespaceMember = EntityDeclaration
                    | ValueDeclaration
                    | EnumDeclaration
                    | WebApiDeclaration
					| NamespaceDeclaration;

    WebApiDeclaration = BaseWebApiPathDeclaration(WebApiKeyword);
    WebApiPathDeclaration = BaseWebApiPathDeclaration(PathKeyword);

    BaseWebApiPathDeclaration(Keyword) = annotations:AnnotationList?
                                         k:Keyword
                                         atom:(AtomDeclaration | name:ValidIdentifier -> new AtomSyntax(Enumerable.Empty<Annotation>(), new TypeReferenceSyntax(new QualifiedIdentifier("void"), CardinalityOrDefault(null)) ,name as Identifier))
                                         arguments:ArgumentList(AtomDeclaration)?
                                         filters:WebApiQueryFilters?
                                         members:WebApiPathBody ->
                                         {
                                             var a = atom as AtomSyntax;
                                         
                                             if((k as string) == "webapi"):
                                                 return new WebApiSyntax(
                                                     AsSequence<Annotation>(annotations),
                                                     a.Type,
                                                     a.Name,
                                                     AsSequence<AtomSyntax>(arguments),
                                                     AsSequence<AtomSyntax>(filters),
                                                     AsSequence<IWebApiPathSyntaxMember>(members));
                                             end
                                             
                                             return new WebApiPathSyntax(
                                                 AsSequence<Annotation>(annotations),
                                                 a.Type,
                                                 a.Name,
                                                 AsSequence<AtomSyntax>(arguments),
                                                 AsSequence<AtomSyntax>(filters),
                                                 AsSequence<IWebApiPathSyntaxMember>(members));
                                         }

    WebApiPathBody =  BlockBegin
                      members:WebApiPathMember*
                      error unless BlockEnd -> members;

    WebApiPathMember = WebApiPathDeclaration
                     | WebApiPathOperation;

    WebApiPathOperation = WebApiPathQuery
                        | WebApiPathCommand;

    WebApiPathCommand = annotations:AnnotationList?
                        CommandKeyword
                        type:TypeReferenceWithoutCardinality
                        name:ValidIdentifier
                        arguments:ArgumentList(AtomDeclaration)
                        StatementEnd -> new WebApiCommandSyntax(AsSequence<Annotation>(annotations),
                                                                type as TypeReferenceSyntax,
                                                                name as Identifier,
                                                                AsSequence<AtomSyntax>(arguments));

    WebApiPathQuery = annotations:AnnotationList?
                      QueryKeyword
                      type:TypeReferenceWithCardinality
                      name:ValidIdentifier
                      arguments:ArgumentList(AtomDeclaration)
                      filters:WebApiQueryFilters?
                      StatementEnd ->
                      {
                          return new WebApiQuerySyntax(AsSequence<Annotation>(annotations),
                                                       type as TypeReferenceSyntax,
                                                       name as Identifier,
                                                       AsSequence<AtomSyntax>(arguments),
                                                       AsSequence<AtomSyntax>(filters));
                      }

    WebApiQueryFilters = FiltersKeyword 
                         filters:RequiredArgumentList(AtomDeclaration)-> filters;

    UsingStatement = UsingKeyword
                     ns:QualifiedIdentifier
                     error until StatementEnd -> ns;

	MoleculeMember = ValueDeclaration
	               | EnumDeclaration
				   | PropertyDeclaration;

    EnumDeclaration = annotations:AnnotationList?
                      ExternKeyword
                      EnumKeyword
                      name:ValidIdentifier
                      StatementEnd ->
                      {
                           return new ExternalEnumSyntax(AsSequence<Annotation>(annotations),
                                                         name as Identifier);
                      }

    ValueDeclaration = annotations:AnnotationList?
                       isExternal:ExternKeyword?
                       isAbstract:AbstractKeyword?
                       ValueKeyword
                       name:ValidIdentifier
                       baseValue:(":" b:TypeReferenceWithoutCardinality -> b)?
                       members:(BlockBegin m:MoleculeMember* error unless BlockEnd -> m | error unless StatementEnd -> null)
                       e:ValueDeclarationEpilogue? ->
                       {
                            return new ValueSyntax(AsSequence<Annotation>(annotations),
                                                   isExternal != null,
                                                   isAbstract != null,
                                                   name as Identifier,
                                                   (baseValue as TypeReferenceSyntax).ToMaybe(),
                                                   AsSequence<PropertySyntax>(members),
                                                   e.Cast<Identifier>());
                       }
                       
    ValueDeclarationEpilogue = e:ValueEqualityDeclaration?
                               StatementEnd -> e;
                               
    ValueEqualityDeclaration = EqualKeyword
                               ByKeyword
                               l:RequiredArgumentList(ValidIdentifier, ",") -> l;


    EntityDeclaration = annotations:AnnotationList?
                        isAbstract:AbstractKeyword?
                        EntityKeyword
                        identityType:("<" i:TypeReferenceWithoutCardinality ">" -> i)?
                        name:ValidIdentifier
                        baseEntity:(":" b:TypeReferenceWithoutCardinality -> b)?
                        BlockBegin
                        events:EventDeclaration*
                        error unless BlockEnd ->
                        {
                            return new EntitySyntax(AsSequence<Annotation>(annotations),
                                                    isAbstract != null,
                                                    (identityType as TypeReferenceSyntax).ToMaybe(),
                                                    name as Identifier,
                                                    (baseEntity as TypeReferenceSyntax).ToMaybe(),
                                                    AsSequence<EventSyntax>(events));
                        }

    EventDeclaration = annotations:AnnotationList?
                       isAbstract:AbstractKeyword?
                       EventKeyword
                       name:ValidIdentifier
                       baseEvent:(":" b:TypeReferenceWithoutCardinality -> b)?
                       members:(BlockBegin m:MoleculeMember* error unless BlockEnd -> m | error unless StatementEnd -> null) ->
                       {
                            return new EventSyntax(AsSequence<Annotation>(annotations),
                                                   isAbstract != null,
                                                   name as Identifier,
                                                   (baseEvent as TypeReferenceSyntax).ToMaybe(),
                                                   AsSequence<PropertySyntax>(members));
                       }

    PropertyDeclaration = atom:AtomDeclaration
                          aliases:(AliasesKeyword a:ValidIdentifier -> a)?
                          StatementEnd -> 
						  {
						      var a = atom as AtomSyntax;
						      return new PropertySyntax(a.Annotations,
                                                        a.Type,
                                                        a.Name,
                                                        (aliases as Identifier).ToMaybe());
						  }

    AtomDeclaration = annotations:AnnotationList?
                      type:TypeReferenceWithCardinality
                      name:ValidIdentifier -> new AtomSyntax(AsSequence<Annotation>(annotations), type as TypeReferenceSyntax, name as Identifier);

    TypeReferenceWithoutCardinality = name:BuiltInTypes -> new TypeReferenceSyntax(new QualifiedIdentifier(name as string), CardinalityOrDefault(null))
                                    | name:QualifiedIdentifier -> new TypeReferenceSyntax(name as QualifiedIdentifier, CardinalityOrDefault(null));

    TypeReferenceWithCardinality = name:BuiltInTypes
                                   cardinality:Cardinality? -> new TypeReferenceSyntax(new QualifiedIdentifier(name as string), CardinalityOrDefault(cardinality))
                                 | name:QualifiedIdentifier
                                   cardinality:Cardinality? -> new TypeReferenceSyntax(name as QualifiedIdentifier, CardinalityOrDefault(cardinality));

    AnnotationList = ("[" l:RequiredList(Annotation, ArgumentSeperator) "]" -> l)* -> Flatten(match);

    Annotation = name:ValidIdentifier
                 pairs:("(" l:List(AnnotationPair, ArgumentSeperator) ")" -> l)? -> new Annotation(name as Identifier, AsSequence<AnnotationPair>(pairs));

    AnnotationPair = name:ValidIdentifier ":" value:(Number|StringLiteral) -> new AnnotationPair(name as Identifier, value as string);

    QualifiedIdentifier = ValidIdentifier
                     (IdentifierDelimiter i:ValidIdentifier -> i)* -> new QualifiedIdentifier(Flatten(match).Cast<Identifier>());

    ValidIdentifier = !Keywords i:Identifier -> new Identifier(i as string);

    Cardinality = BoundCardinalityRange
                | UnboundCardinalityRange
                | FixedCardinality
                | ZeroOrOneOperator -> new Cardinality(0, 1)
                | ZeroOrMoreOperator -> new Cardinality(0)
                | OneOrMoreOperator -> new Cardinality(1);
                        
    FixedCardinality = CardinalityRangeStart i:PositiveInteger -> new Cardinality(int.Parse(i as string), Maybe.Return(int.Parse(i as string)));

    BoundCardinalityRange = CardinalityRangeStart
                            min:PositiveInteger
                            RangeDelimiter
                            max:PositiveInteger
                            CardinalityRangeEnd -> new Cardinality(int.Parse(min as string), Maybe.Return(int.Parse(max as string)));
                       
    UnboundCardinalityRange = CardinalityRangeStart
                              min:PositiveInteger
                              RangeDelimiter
                              UnboundedCardinality
                              CardinalityRangeEnd -> new Cardinality(int.Parse(min as string));

    Keywords = BuiltInTypes
             | ThisKeyword
             
             | NamespaceKeyword
             | ValueKeyword
             | EnumKeyword
             | EntityKeyword
             | EventKeyword
             | CommandKeyword
             | QueryKeyword
             | PathKeyword
             | MessageKeyword
             | ApiKeyword
             | WebApiKeyword
             | SubjectKeyword
             | VerbKeyword
             | SliceKeyword
             | FilterKeyword
             | FiltersKeyword
             | DefaultKeyword
             | ExternKeyword
             | AbstractKeyword
             | EqualKeyword
             | EqualsKeyword
             | ByKeyword
             | WhereKeyword
             | SelectKeyword
             | JoinKeyword
             | GroupKeyword
             | OnKeyword
             | MatchesKeyword
             | BetweenKeyword
             | BeforeKeyword
             | AfterKeyword
             | InKeyword
             | AsKeyword
             | UsingKeyword
             | AliasesKeyword
             | TrueKeyword
             | FalseKeyword;

    [Keyword] ThisKeyword = "this";
    [Keyword] NamespaceKeyword = "namespace";
    [Keyword] ValueKeyword = "value";
    [Keyword] EnumKeyword = "enum";
    [Keyword] EntityKeyword = "entity";
    [Keyword] EventKeyword = "event";
    [Keyword] CommandKeyword = "command";
    [Keyword] QueryKeyword = "query";
    [Keyword] PathKeyword = "path";
    [Keyword] MessageKeyword = "message";
    [Keyword] ApiKeyword = "api";
    [Keyword] WebApiKeyword = "webapi";
    [Keyword] SubjectKeyword = "subject";
    [Keyword] VerbKeyword = "verb";
    [Keyword] SliceKeyword = "slice";
    [Keyword] FilterKeyword = "filter";
    [Keyword] FiltersKeyword = "filters";

    [Keyword] DefaultKeyword = "default";
    [Keyword] ExternKeyword = "extern";
    [Keyword] AbstractKeyword = "abstract";
    [Keyword] EqualKeyword = "equal";
    [Keyword] EqualsKeyword = "equals";
    [Keyword] ByKeyword = "by";
    [Keyword] WhereKeyword = "where";
    [Keyword] SelectKeyword = "select";
    [Keyword] JoinKeyword = "join";
    [Keyword] GroupKeyword = "group";
    [Keyword] OnKeyword = "on";
    [Keyword] MatchesKeyword = "matches";
    [Keyword] BetweenKeyword = "between";
    [Keyword] BeforeKeyword = "before";
    [Keyword] AfterKeyword = "after";
    [Keyword] InKeyword = "in";
    [Keyword] AsKeyword = "as";
    [Keyword] UsingKeyword = "using";
    [Keyword] AliasesKeyword = "aliases";
    [Keyword] TrueKeyword = "true";
    [Keyword] FalseKeyword = "false";

    [Keyword]
    BuiltInTypes = "void"
                 | "bool"
                 | "byte"
                 | "char"
                 | "decimal"
                 | "double"
                 | "float"
                 | "short"
                 | "int"
                 | "long"
                 | "uint"
                 | "ulong"
                 | "ushort"
                 | "sbyte"
                 | "string"
                 | "guid"
                 | "datetime";

    IdentifierDelimiter = ".";
    RangeDelimiter = "..";

    BlockBegin = "{";
    BlockEnd = "}";
    StatementEnd = ";";

    RequiredList(Match, Separator) = Match (Separator m:Match -> m)* -> Flatten(match);

    ArgumentListBegin= "(";
    ArgumentListEnd = ")";
    ArgumentSeperator = ",";

    ArgumentList(Match) = ArgumentListBegin
                          l:List(Match, ArgumentSeperator)
                          ArgumentListEnd -> l;



    RequiredArgumentList(Match) = ArgumentListBegin
                                  l:RequiredList(Match, ArgumentSeperator)
                                  ArgumentListEnd -> l;

    CardinalityRangeStart = "[";
    CardinalityRangeEnd = "]";

    ZeroOrOneOperator = "?";
    ZeroOrMoreOperator = "*";
    OneOrMoreOperator = "+";
    UnboundedCardinality = "*";
    
    PositiveInteger = i:['0'..'9'+] -> Stringify(i);

  end
end