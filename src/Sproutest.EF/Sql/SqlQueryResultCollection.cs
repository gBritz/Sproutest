using Microsoft.EntityFrameworkCore;
using Moq;
using Sproutest.EF.EntityFramework.InMemory;
using Sproutest.EF.EntityFramework.QueryProviders;

namespace Sproutest.EF.Sql
{
    public class SqlQueryResultCollection<TQuery> : ISqlQueryResultCollection
      where TQuery : class
  {
    private readonly List<SqlQueryResult<TQuery>> _results = [];

    public SqlQueryResultBuilder<TQuery> Always()
      => new SqlQueryResultBuilder<TQuery>(this, _ => true);

    public SqlQueryResultBuilder<TQuery> When(Func<ParameterCollection, bool> condition)
      => new SqlQueryResultBuilder<TQuery>(this, condition);

    IQueryable ISqlQueryResultCollection.Execute(object[] parameters)
    {
      var parametersCollection = parameters == null ? new ParameterCollection() : new ParameterCollection(parameters);
      var results = _results
        .AsEnumerable()
        .Reverse()
        .Where(x => x.Condition(parametersCollection))
        .SelectMany(_ => _.Data);

      return results.AsQueryable();
      // return CreateQueryable(results);
    }

    IQueryable ISqlQueryResultCollection.Execute(IDictionary<string, object> parameters)
    {
      var parametersCollection = parameters == null ? new ParameterCollection() : new ParameterCollection(parameters);
      var results = _results
        .AsEnumerable()
        .Reverse()
        .Where(x => x.Condition(parametersCollection))
        .SelectMany(_ => _.Data);

      return results.AsQueryable();
    }

    IQueryable<T> ISqlQueryResultCollection.Execute<T>(IDictionary<string, object> parameters)
    {
      var parametersCollection = parameters == null ? new ParameterCollection() : new ParameterCollection(parameters);
      var results = _results
        .AsEnumerable()
        .Reverse()
        .Where(x => x.Condition(parametersCollection))
        .SelectMany(_ => _.Data)
        .OfType<T>();

      return results.AsQueryable();
    }

    internal void Add(SqlQueryResult<TQuery> result)
      => _results.Add(result);

    // TODO: lembrar o motivo desse método
    // TODO: caso desejar criar um dbset com a listagem dentro
    /*
    private static IQueryable CreateQueryableDbSet(IEnumerable<TQuery> enumerable)
    {
      var queryable = enumerable.AsQueryable();
      var dbSetMock = new Mock<DbSet<TQuery>>();

      dbSetMock
        .As<IAsyncEnumerable<TQuery>>()
        .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
        .Returns(new InMemoryDbAsyncEnumerator<TQuery>(queryable.GetEnumerator()));

      dbSetMock
        .As<IQueryable<TQuery>>()
        .Setup(m => m.Provider)
        .Returns(new InMemoryAsyncQueryProviderWithMocks<TQuery>(queryable.Provider));

      dbSetMock
        .As<IQueryable<TQuery>>()
        .Setup(m => m.Expression)
        .Returns(queryable.Expression);

      dbSetMock
        .As<IQueryable<TQuery>>()
        .Setup(m => m.ElementType)
        .Returns(queryable.ElementType);

      dbSetMock
        .As<IQueryable<TQuery>>()
        .Setup(m => m.GetEnumerator())
        .Returns(() => queryable.GetEnumerator());

      return dbSetMock.Object;
    }
    */
  }
}
