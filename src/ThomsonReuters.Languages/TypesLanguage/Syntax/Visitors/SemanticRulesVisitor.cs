using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors
{
    public class SemanticRulesVisitor : ValidationVisitor
    {
        private SemanticRulesVisitor(SymbolTable symbolTable)
            : base(symbolTable)
        {
        }

        public static Outcome<string> Validate(IEnumerable<SyntaxTree> trees, SymbolTable symbolTable)
        {
            Guard.NotNull(trees, "trees");
            Guard.NotNull(symbolTable, "symbolTable");

            var visitor = new SemanticRulesVisitor(symbolTable);
            return visitor.Validate(trees);
        }

        public override void VisitEntitySyntax(EntitySyntax entity)
        {
            if(entity.IdentityType.HasValue)
            {
                if(entity.Base.HasValue)
                    Fail("Entity '{0}' must not specify a base entity if the identifier type is specified.", entity.Name);

                var identityType = entity.IdentityType.Value;

                var type = ResolveAndValidate(identityType, entity.Parent);
                FailIf(type.HasValue && !IsValue(type.Value), "Identity type for '{0}' type must be value type.", entity.Name);

            }
            else if(!entity.Base.HasValue)
                Fail("Entity '{0}' must inherit from a base entity, or specify an identifier type.", entity.Name);

            ValidateBase(entity, "Entity", x => x.Base);
            base.VisitEntitySyntax(entity);
        }

        public override void VisitEventSyntax(EventSyntax @event)
        {
            ValidateBase(@event, "Event", x => x.Base);
            @event.Properties.Run(ValidateDataTypeProperty);

            base.VisitEventSyntax(@event);
        }

        public override void VisitValueSyntax(ValueSyntax value)
        {
            ValidateBase(value, "Value", x => x.Base, typeof(ValueSyntax), typeof(BuiltInType));
            value.Properties.Run(ValidateDataTypeProperty);

            base.VisitValueSyntax(value);
        }

        private void ValidateBase<T>(T dataType, string typeName, Func<T, Maybe<TypeReferenceSyntax>> baseSelector, params Type[] validBaseTypes)
            where T : ITypeSyntax
        {
            baseSelector(dataType)
                .Run(baseReference =>
                {
                    var type = ResolveAndValidate(baseReference, dataType.Parent).OfType<IType>();

                    var permittedBaseTypes = validBaseTypes.Or(new[] {typeof(T)});

                    FailIf(type.HasValue && !(permittedBaseTypes.Contains(type.Value.GetType())), "{0} '{1}' cannot inherit from '{2}'.", typeName, dataType.Name, type.Select(x => x.Name.ToString()).ValueOrDefault(""));
                    FailIf(type.HasValue && ReferenceEquals(type.Value, dataType), "{0} '{1}' cannot inherit from itself.", typeName, dataType.Name);
                });
        }

        private void ValidateDataTypeProperty(AtomSyntax atom)
        {
            var type = ResolveAndValidate(atom.Type, atom.Parent);
            FailIf(type.HasValue && !IsValue(type.Value), "Property '{0}' type must be value type.", atom.Name);
        }

        public override void VisitWebApiCommandSyntax(WebApiCommandSyntax command)
        {
            ValidateOperation(command, "Command");
            base.VisitWebApiCommandSyntax(command);
        }

        public override void VisitWebApiPathSyntax(WebApiPathSyntax path)
        {
            ValidateOperation(path, "Path");
            base.VisitWebApiPathSyntax(path);
        }

        public override void VisitWebApiQuerySyntax(WebApiQuerySyntax query)
        {                   
            ValidateOperation(query, "Query");
            base.VisitWebApiQuerySyntax(query);
        }

        private void ValidateOperation(WebApiOperationSyntax operation, string operationType)
        {
            var type = ResolveAndValidate(operation.Result, Namespace);
            FailIf(type.HasValue && !IsValue(type.Value), "{0} '{1}' must return a value type.", operationType, operation.Name);

            if(operation.OperationType == WebApiOperationSyntaxType.Query)
                FailIf(type.OfType<IType>().Select(x => x.IsVoid()).ValueOrDefault(), "{0} '{1}' must return some result.", operationType, operation.Name);

            var arguments = operation.Arguments.ToArray();
            if (arguments.Length > 1)
            {
                Fail("{0} '{1}' can only contain one argument.", operationType, operation.Name);
            }
            else if (arguments.Length == 1 && arguments[0] != null)
            {
                var argument = arguments[0];

                var cardinality = argument.Type.Cardinality;
                if (cardinality.Minimum != 1 || cardinality.Maximum != 1)
                    Fail("Query argument for '{0}' {1} cannot have a cardinality othen than one.", operation.Name, operationType.ToLower());

                type = ResolveAndValidate(argument.Type, Namespace);

                if(operation.OperationType == WebApiOperationSyntaxType.Query ||
                   operation.OperationType == WebApiOperationSyntaxType.Path)
                    FailIf(type.HasValue && !IsScalarValue(type.Value), "{0} argument '{1}' for '{2}' {3} must be a scalar value.", operationType, argument.Name, operation.Name, operationType.ToLower());
            }
        }

        public void ValidateWebApiQueryFilter(AtomSyntax queryFilter)
        {
            var type = ResolveAndValidate(queryFilter.Type, queryFilter.Parent);
            FailIf(type.HasValue && !IsScalarValue(type.Value), "Query filter '{0}' must be a scalar value.", queryFilter.Name);

            var cardinality = queryFilter.Type.Cardinality;
            if (cardinality.Minimum != 0 || cardinality.Maximum != 1)
                Fail("Query filter '{0}' must have a cardinality of zero or one.", queryFilter.Name);
        }


        protected Maybe<ISymbol> ResolveAndValidate(TypeReferenceSyntax reference, ISyntaxNode scope)
        {
            return ResolveAndValidate(reference, scope.ToMaybe());
        }

        protected Maybe<ISymbol> ResolveAndValidate(TypeReferenceSyntax reference, Maybe<ISyntaxNode> scope)
        {
            var type = Resolve(reference, scope);
            FailIf(!type.HasValue, "The type or namespace '{0}' could not be found.", reference.Name);

            return type;
        }

        private bool IsValue(ISymbol symbol)
        {
            return symbol is BuiltInType ||
                   symbol is ExternalEnumSyntax ||
                   symbol is ValueSyntax;
        }

        private bool IsScalarValue(ISymbol symbol)
        {
            if (symbol is BuiltInType)
                return true;

            if (symbol is ExternalEnumSyntax)
                return true;

            var value = symbol as ValueSyntax;
            return value != null && 
                   !value.Properties.Any() &&
                   value.Base
                        .SelectMaybe(x => Resolve(x, value.Parent))
                        .Select(IsScalarValue)
                        .ValueOrDefault();
        }
    }
}