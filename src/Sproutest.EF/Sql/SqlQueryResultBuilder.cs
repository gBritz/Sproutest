using System;
using System.Collections.Generic;
using System.Linq;

namespace Sproutest.EF.Sql
{
  public class SqlQueryResultBuilder<TQuery>
    where TQuery : class
  {
    private SqlQueryResultCollection<TQuery> _collection;
    private Func<ParameterCollection, bool> _condition;

    public SqlQueryResultBuilder(
        SqlQueryResultCollection<TQuery> collection,
        Func<ParameterCollection, bool> condition)
    {
      _collection = collection;
      _condition = condition;
    }

    public void Returns(params TQuery[] data)
      => Returns(data.AsEnumerable());

    public void Returns(IEnumerable<TQuery> data)
      => _collection.Add(new(_condition, data));
  }
}
