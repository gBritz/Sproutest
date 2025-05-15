using System;
using System.Collections.Generic;
using System.Linq;

namespace Sproutest.EF.Sql
{
  public class SqlMultipleQueryResultCollection : ISqlQueryResultCollection
  {
    private readonly List<SqlQueryResult<object>> _results = [];

    public IQueryable Execute(object[] parameters)
    {
      var parametersCollection = parameters == null ? new ParameterCollection() : new ParameterCollection(parameters);
      var results = _results
        .AsEnumerable()
        .Reverse()
        .Where(x => x.Condition(parametersCollection))
        .SelectMany(_ => _.Data);

      return results.AsQueryable();
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

    public SqlMultipleQueryResultBuilder When(Func<ParameterCollection, bool> condition) =>
      new(this, condition);

    public SqlMultipleQueryResultCollection Returns(params object[] items)
    {
      _results.Add(new SqlQueryResult<object>(_ => true, items));
      return this;
    }

    public SqlMultipleQueryResultCollection Returns(IEnumerable<object> items)
    {
      _results.Add(new SqlQueryResult<object>(_ => true, items.ToArray()));
      return this;
    }

    internal SqlMultipleQueryResultCollection Add(SqlQueryResult<object> result)
    {
      _results.Add(result);
      return this;
    }
  }
}
