using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public class MessageMediator<TObservation> : IMessageMediator<TObservation>
    {
        public enum PolymorphicMethodMatchingBehavior
        {
            MatchAll,
            MatchMostDerived
        }

        public enum MissingMethodBehavior
        {
            ThrowException,
            ReturnFailure,
            ReturnSuccess
        }

        public enum AmbiguousMethodMatchBehavior
        {
            ThrowException,
            ReturnFailure,
            InvokeAll
        }

        private class TargetMethod
        {
            public TargetMethod(MethodInfo method)
            {
                Method = Guard.NotNull(method, "method");

                MessageType = Method.GetParameters()[0].ParameterType;
                ReturnType = Method.ReturnType;
                DeclaringType = Method.DeclaringType;
            }

            public MethodInfo Method { get; private set; }

            public Type MessageType { get; private set; }
            public Type ReturnType { get; private set; }
            public Type DeclaringType { get; private set; }
        }

        public class TypePair : IEquatable<TypePair>
        {
            public TypePair(Type messageType, Type resultType)
            {
                MessageType = Guard.NotNull(messageType, "messageType");
                ResultType = Guard.NotNull(resultType, "resultType");
            }

            public Type MessageType { get; private set; }
            public Type ResultType { get; private set; }

            public bool Equals(TypePair other)
            {
                if (ReferenceEquals(other, null))
                    return false;

                return MessageType == other.MessageType &&
                       ResultType == other.ResultType;
            }

            public override bool Equals(object obj)
            {
                if (obj is TypePair)
                    return Equals((TypePair)obj);

                return false;
            }

            public override int GetHashCode()
            {
                return MessageType.GetHashCode() ^
                       ResultType.GetHashCode();
            }

            public static bool operator ==(TypePair left, TypePair right)
            {
                bool leftNull = ReferenceEquals(left, null);
                bool rightNull = ReferenceEquals(right, null);

                if (leftNull ^ rightNull)
                    return false;

                if (leftNull && rightNull)
                    return true;

                return left.Equals(right);
            }

            public static bool operator !=(TypePair left, TypePair right)
            {
                return !(left == right);
            }
        }

        private readonly ConcurrentDictionary<Type, IEnumerable<Func<object, IOutcome<TObservation>>>> _publishMethods = null;
        private readonly ConcurrentDictionary<TypePair, Delegate> _dispatchMethods = null;

        private readonly TargetMethod[] _candidateMethods = null;
        private readonly Func<Type, object> _handlerFactory = null;

        private readonly PolymorphicMethodMatchingBehavior _polymorphicMethodMatchingBehavior = PolymorphicMethodMatchingBehavior.MatchAll;
        private readonly MissingMethodBehavior _missingMethodBehavior = MissingMethodBehavior.ThrowException;
        private readonly AmbiguousMethodMatchBehavior _ambiguousMethodMatchBehavior = AmbiguousMethodMatchBehavior.ThrowException;

        public MessageMediator(IEnumerable<Type> handlerTypes,
                                 Func<Type, object> handlerFactory,
                                 Func<MethodInfo, bool> handlerPredicate = null, 
                                 PolymorphicMethodMatchingBehavior polymorphicMethodMatchingBehavior = PolymorphicMethodMatchingBehavior.MatchAll,
                                 MissingMethodBehavior missingMethodBehavior = MissingMethodBehavior.ThrowException,
                                 AmbiguousMethodMatchBehavior ambiguousMethodMatchBehavior = AmbiguousMethodMatchBehavior.ThrowException)
        {
            Guard.NotNull(handlerTypes, "handlerTypes").ToArray();

            _handlerFactory = Guard.NotNull(handlerFactory, "handlerFactory");

            _polymorphicMethodMatchingBehavior = Guard.MustBeDefined(polymorphicMethodMatchingBehavior, "polymorphicMethodMatchingBehavior");
            _missingMethodBehavior = Guard.MustBeDefined(missingMethodBehavior, "missingMethodBehavior");
            _ambiguousMethodMatchBehavior = Guard.MustBeDefined(ambiguousMethodMatchBehavior, "ambiguousMethodMatchBehavior");

            _publishMethods = new ConcurrentDictionary<Type, IEnumerable<Func<object, IOutcome<TObservation>>>>();
            _dispatchMethods = new ConcurrentDictionary<TypePair, Delegate>();

            _candidateMethods = handlerTypes
                .SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     .Where(m => IsCandidateMethod(m, handlerPredicate)))
                .Select(x => new TargetMethod(x))
                .ToArray();
        }

        private static bool IsCandidateMethod(MethodInfo method, Func<MethodInfo, bool> handlerPredicate = null)
        {
            if (method.IsSpecialName)
                return false;

            if (method.GetParameters().Length != 1)
                return false;

            if (handlerPredicate != null && !handlerPredicate(method))
                return false;

            return true;
        }

        private Func<object, IResult<T, TObservation>> GetDispatchMethod<T>(TypePair typePair)
        {
            TargetMethod[] finalMethodList = GetFinalMethodList<IResult<T, TObservation>>(typePair.MessageType);

            if (finalMethodList.Length <= 0)
                return CompileEmptyMethod<IResult<T, TObservation>>(_missingMethodBehavior, "Unable to find method to dispatch message to.", new Result<T, TObservation>(Maybe.NoValue, Outcome.Failure()), new Result<T, TObservation>(Maybe.NoValue, Outcome.Success()));

            if (finalMethodList.Length == 1)
                return CompileMethod<IResult<T, TObservation>>(finalMethodList[0], new Result<T, TObservation>());

            return CompileEmptyMethod<IResult<T, TObservation>>(_ambiguousMethodMatchBehavior == AmbiguousMethodMatchBehavior.ReturnFailure
                ? MissingMethodBehavior.ReturnFailure
                : MissingMethodBehavior.ThrowException, "Ambiguous method match.", new Result<T, TObservation>(Maybe.NoValue, Outcome.Failure()), new Result<T, TObservation>(Maybe.NoValue, Outcome.Success()));
        }

        private IEnumerable<Func<object, IOutcome<TObservation>>> GetPublishMethods(Type messageType)
        {
            TargetMethod[] finalMethodList = GetFinalMethodList<IOutcome<TObservation>>(messageType);

            if (finalMethodList.Length <= 0)
                return new[] { CompileEmptyMethod<IOutcome<TObservation>>(_missingMethodBehavior, "Unable to find any methods to publish message to.", new Outcome<TObservation>(false), new Outcome<TObservation>(true)) };

            if (finalMethodList.Length == 1 || _ambiguousMethodMatchBehavior == AmbiguousMethodMatchBehavior.InvokeAll)
                return CompileMethods<IOutcome<TObservation>>(finalMethodList, new Outcome<TObservation>());

            return new[]{CompileEmptyMethod<IOutcome<TObservation>>(_ambiguousMethodMatchBehavior == AmbiguousMethodMatchBehavior.ThrowException
                ? MissingMethodBehavior.ThrowException
                : MissingMethodBehavior.ReturnFailure, "Ambiguous method match.", new Outcome<TObservation>(false), new Outcome<TObservation>(true))};
        }

        private TargetMethod[] GetFinalMethodList<TResult>(Type messageType)
        {
            Func<Type, Type, int> comparison = CompareTypes;

            var groupedOrderedMethods = _candidateMethods
                .Where(x => x.ReturnType == typeof(void) || typeof(TResult).IsAssignableFrom(x.ReturnType) || IsConvertableOutcome<TResult>(x.ReturnType))
                .Where(x => x.MessageType.IsAssignableFrom(messageType))
                .GroupBy(x => x.MessageType)
                .OrderBy(x => x.Key, comparison.ToComparer())
                .ToArray();

            if (_polymorphicMethodMatchingBehavior == PolymorphicMethodMatchingBehavior.MatchMostDerived)
            {
                groupedOrderedMethods = groupedOrderedMethods
                    .Skip(groupedOrderedMethods.Length - 1)
                    .ToArray();
            }

            return groupedOrderedMethods
                .SelectMany(x => x)
                .ToArray();
        }

        private static bool IsConvertableOutcome<T>(Type returnType)
        {
            if (typeof(T).GetGenericTypeDefinition() != typeof(IResult<,>))
                return false;

            if (returnType.IsGenericType != true)
                return false;

            if (returnType.GetGenericTypeDefinition() != typeof(Outcome<>))
                return false;

            if (typeof(T).GetGenericArguments()[0].IsAssignableFrom(returnType.GetGenericArguments()[0]) != true)
                return false;

            return true;
        }

        private static int CompareTypes(Type left, Type right)
        {
            int result = 0;

            if (left.IsAssignableFrom(right))
                result--;

            if (right.IsAssignableFrom(left))
                result++;

            return result;
        }

        private IEnumerable<Func<object, TResult>> CompileMethods<TResult>(IEnumerable<TargetMethod> methods, TResult @default)
        {
            return methods.Select(x => CompileMethod(x, @default));
        }

        private Func<object, TResult> CompileMethod<TResult>(TargetMethod method, TResult @default)
        {
            var message = Expression.Parameter(typeof(object));

            Expression result = Expression.Call(
                Expression.Convert(
                    Expression.Invoke(Expression.Constant(_handlerFactory),
                                      Expression.Constant(method.DeclaringType)),
                    method.DeclaringType),
                method.Method,
                Expression.Convert(message, method.MessageType));

            if (method.ReturnType == typeof(void))
                result = Expression.Block(result, Expression.Constant(@default, typeof(TResult)));
            else if (method.ReturnType != typeof(TResult) && typeof(TResult).IsAssignableFrom(method.ReturnType))
                result = Expression.Convert(result, typeof (TResult));
            else if(method.ReturnType != typeof(TResult))
            {
                result = Expression.Call(result, "ToResult", Type.EmptyTypes);

                if(typeof(TResult).IsAssignableFrom(result.Type) != true)
                {
                    result = Expression.Call(typeof(Result<,>).MakeGenericType(typeof(TResult).GetGenericArguments()),
                                "op_Implicit", Type.EmptyTypes, result);
                }

                result = Expression.Convert(result, typeof(TResult));
            }

            return Expression.Lambda<Func<object, TResult>>(result, message)
                .Compile();
        }


        private static Func<object, TResult> CompileEmptyMethod<TResult>(MissingMethodBehavior behavior, string message, TResult failure, TResult success)
        {
            var parameter = Expression.Parameter(typeof (object));

            if (behavior == MissingMethodBehavior.ThrowException)
            {
                return Expression.Lambda<Func<object, TResult>>(
                    Expression.Throw(
                    Expression.Constant(new InvalidOperationException(message)), typeof(TResult)), parameter)
                    .Compile();
            }

            if (behavior == MissingMethodBehavior.ReturnFailure)
            {
                return Expression.Lambda<Func<object, TResult>>(
                    Expression.Constant(failure, typeof(TResult)), parameter)
                    .Compile();
            }

            return Expression.Lambda<Func<object, TResult>>(
                    Expression.Constant(success, typeof(TResult)), parameter)
                    .Compile();
        }

        public IOutcome<TObservation> Publish(object message)
        {
            Guard.NotNull(message, "message");

            var messageType = message.GetType();

            var outcomes =  _publishMethods
                .GetOrAdd(messageType, GetPublishMethods)
                .Select(x => x(message))
                .Select(x => x.AsOutcome())
                .ToArray();

            return outcomes
                .Combine();
        }

        public IResult<T, TObservation> Dispatch<T>(object message)
        {
            Guard.NotNull(message, "message");
            var typePair = new TypePair(message.GetType(), typeof (T));

            var method = ((Func<object, IResult<T, TObservation>>)_dispatchMethods.GetOrAdd(typePair, GetDispatchMethod<T>));
            return method(message);
        }
    }
}