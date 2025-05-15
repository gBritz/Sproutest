using System;
using System.Collections.Generic;
using System.Linq;

namespace Sproutest.EF.Sql
{
    public class SqlQueryContext
  {
    private readonly Dictionary<Type, ISqlQueryResultCollection> _data = [];
    private readonly List<ISqlQueryResultCollection> _multipleQueries = [];

    public void Add<TQuery>(Action<SqlQueryResultCollection<TQuery>> query = null)
      where TQuery : class
    {
      var result = GetOrAddQueryResult<TQuery>();
      query?.Invoke(result);
    }

    public void AddMultiples(Action<SqlMultipleQueryResultCollection> query = null)
    {
      var queryResult = new SqlMultipleQueryResultCollection();
      query?.Invoke(queryResult);
      _multipleQueries.Add(queryResult);
    }

    public IQueryable Execute(Type queryType, object[] parameters)
    {
      if (_data.TryGetValue(queryType, out var results))
      {
        return results.Execute(parameters);
      }

      throw new InvalidOperationException($"The query result was not configured for type \"{queryType.Name}\".");
    }

    public IQueryable Execute(Type queryType, IDictionary<string, object> parameters)
    {
      if (_data.TryGetValue(queryType, out var results))
      {
        return results.Execute(parameters);
      }

      throw new InvalidOperationException($"The query result was not configured for type \"{queryType.Name}\".");
    }

    public IQueryable<T> Execute<T>(IDictionary<string, object> parameters)
    {
      if (_data.TryGetValue(typeof(T), out var results))
      {
        return results.Execute<T>(parameters);
      }

      throw new InvalidOperationException($"The query result was not configured for type \"{typeof(T).Name}\".");
    }

    private SqlQueryResultCollection<TQuery> GetOrAddQueryResult<TQuery>()
      where TQuery : class
    {
      if (_data.TryGetValue(typeof(TQuery), out var temp) &&
          temp is SqlQueryResultCollection<TQuery> result)
      {
        return result;
      }

      var newResult = new SqlQueryResultCollection<TQuery>();
      _data[typeof(TQuery)] = newResult;
      return newResult;
    }
  }
}
