using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;

namespace Sproutest.EF.EntityFramework.InMemory
{
  public class InMemoryAsyncQueryProvider<TEntity>(IQueryProvider innerQueryProvider) : IAsyncQueryProvider
  {
    private readonly IQueryProvider _innerQueryProvider = innerQueryProvider ?? throw new ArgumentNullException(nameof(innerQueryProvider));

    public IQueryable CreateQuery(Expression expression) =>
      new InMemoryAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
      new InMemoryAsyncEnumerable<TElement>(expression);

    public object? Execute(Expression expression) =>
      _innerQueryProvider.Execute(expression);

    public TResult Execute<TResult>(Expression expression) =>
      _innerQueryProvider.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = new CancellationToken())
    {
      var result = Execute(expression);

      var expectedResultType = typeof(TResult).GetGenericArguments()?.FirstOrDefault();
      if (expectedResultType == null)
      {
        return default;
      }

      return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))?
        .MakeGenericMethod(expectedResultType)
        .Invoke(null, [result]);
    }


    public Task<object> ExecuteAsync(Expression expression, CancellationToken cancellationToken) =>
      Task.FromResult(Execute(expression));
  }
}
