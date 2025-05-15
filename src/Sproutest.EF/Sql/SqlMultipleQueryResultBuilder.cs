using System;
using System.Collections.Generic;
using System.Linq;

namespace Sproutest.EF.Sql
{
  public class SqlMultipleQueryResultBuilder(
    SqlMultipleQueryResultCollection query,
    Func<ParameterCollection, bool> condition)
  {
    private readonly SqlMultipleQueryResultCollection _collection = query ?? throw new ArgumentNullException(nameof(query));
    private readonly Func<ParameterCollection, bool> _condition = condition ?? throw new ArgumentNullException(nameof(condition));

    public SqlMultipleQueryResultBuilder When(Func<ParameterCollection, bool> condition) =>
      _collection.When(condition);

    public SqlMultipleQueryResultBuilder Returns(params object[] data) =>
      Returns(data.AsEnumerable());

    public SqlMultipleQueryResultBuilder Returns(IEnumerable<object> data)
    {
      _collection.Add(new(_condition, data));
      return this;
    }
  }
}
