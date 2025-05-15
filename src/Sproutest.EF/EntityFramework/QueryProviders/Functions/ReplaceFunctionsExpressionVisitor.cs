using Sproutest.EF.EntityFramework.QueryProviders.Functions.Mocks;
using System.Linq.Expressions;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.QueryProviders.Functions;

internal class ReplaceFunctionsExpressionVisitor : ExpressionVisitor
{
    private readonly Dictionary<MethodInfo, MethodInfo> _mocks = new()
    {
        // TODO: se tiver alguma função sendo utilizada, mockar assim
        [ILikeMock.Original] = ILikeMock.Replacement,
        [UnaccentMock.Original] = UnaccentMock.Replacement,
    };

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (_mocks.TryGetValue(node.Method, out var mock))
        {
            node = Expression.Call(mock, node.Arguments);
        }

        return base.VisitMethodCall(node);
    }
}