using Microsoft.EntityFrameworkCore.Query;
using Sproutest.EF.EntityFramework.InMemory;
using Sproutest.EF.EntityFramework.QueryProviders.Functions;
using System.Linq.Expressions;

namespace Sproutest.EF.EntityFramework.QueryProviders
{
    internal class InMemoryAsyncQueryProviderWithMocks<TEntity>(IQueryProvider innerQueryProvider) : IAsyncQueryProvider, IQueryProvider
  {
    private static readonly ReplaceFunctionsExpressionVisitor _visitor = new();

    private readonly IAsyncQueryProvider _innerQueryProvider = new InMemoryAsyncQueryProvider<TEntity>(innerQueryProvider);

    public IQueryable CreateQuery(Expression expression)
    {
      var modifiedExpression = _visitor.Visit(expression);
      return _innerQueryProvider.CreateQuery(modifiedExpression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
      var modifiedExpression = _visitor.Visit(expression);
      return _innerQueryProvider.CreateQuery<TElement>(modifiedExpression);
    }

    public object? Execute(Expression expression)
      => _innerQueryProvider.Execute(expression);

    public TResult Execute<TResult>(Expression expression)
      => _innerQueryProvider.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
      => _innerQueryProvider.ExecuteAsync<TResult>(expression, cancellationToken);
  }
}
