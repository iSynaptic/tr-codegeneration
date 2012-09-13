using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages
{
    public abstract class Visitor
    {
        private readonly LazySelectionDictionary<Type, Action<object>> _Methods = null;

        protected Visitor()
        {
            _Methods = new LazySelectionDictionary<Type, Action<object>>(type => 
            {
                var methods = GetType()
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                    .Where(x =>
                    {
                        var parameters = x.GetParameters();

                        return x.Name.StartsWith("Visit") &&
                               x.ReturnType == typeof (void) &&
                               parameters.Length == 1 &&
                               parameters[0].ParameterType == type;
                    })
                    .ToArray();

                if(methods.Length > 1)
                    throw new InvalidOperationException(string.Format("Ambiguous visitor match; {0} methods found matching '{1}' type:\r\n{2}", methods.Length, type.Name, methods.Select(x => x.Name).Delimit("\r\n")));

                return methods
                    .FirstOrDefault()
                    .ToMaybe()
                    .Select(BuildDelegate);
            });
        }

        private Action<object> BuildDelegate(MethodInfo method)
        {
            var parameter = Expression.Parameter(typeof (object));
            var @this = Expression.Constant(this);

            return Expression.Lambda<Action<object>>(Expression.Call(@this, method, Expression.Convert(parameter, method.GetParameters()[0].ParameterType)), parameter)
                .Compile();
        }

        public virtual void Visit<T>(IEnumerable<T> subjects)
        {
            if (subjects == null) 
                return;

            foreach (var subject in subjects)
                Visit(subject);
        }

        public virtual void Visit<T>(T subject)
        {
            if (subject == null || NotInterestedIn(subject))
                return;

            _Methods
                .TryGetValue(subject.GetType())
                .Run(x => x(subject));
        }

        protected virtual bool NotInterestedIn(object subject)
        {
            return false;
        }
    }
}
