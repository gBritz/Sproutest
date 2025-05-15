using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.Moq
{
    public static class TypeExtensions
    {
        private static readonly Type _funcTwoArgsType = typeof(Func<,>);

        public static Expression MakeExpressionByProperty(this Type type, PropertyInfo propertyInfo)
        {
            var parameter = Expression.Parameter(type, "value");
            var property = Expression.Property(parameter, propertyInfo);
            var funcType = _funcTwoArgsType.MakeGenericType(type, propertyInfo.PropertyType);
            var lambda = Expression.Lambda(funcType, property, parameter);

            return lambda;
        }
    }
}
