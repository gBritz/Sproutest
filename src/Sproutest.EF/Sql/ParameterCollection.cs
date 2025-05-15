using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sproutest.EF.Sql
{
  public class ParameterCollection
  {
    private readonly IDictionary<string, object> _parameters;

    public ParameterCollection()
    {
      _parameters = new Dictionary<string, object>();
    }

    public ParameterCollection(IDictionary<string, object> parameters)
    {
      ArgumentNullException.ThrowIfNull(parameters, nameof(parameters));
      _parameters = parameters;
    }

    public ParameterCollection(object[] parameters)
      => _parameters = parameters
        .Cast<IDbDataParameter>()
        .ToDictionary(x => x.ParameterName, x => x.Value, StringComparer.OrdinalIgnoreCase);

    public T Parameter<T>(string name) =>
      _parameters.TryGetValue(name, out var value) && value is not null
        ? (T)value
        : default;

    public bool HasParameter(string name)
      => _parameters.ContainsKey(name);

    public bool IsNull(string name) =>
      _parameters.TryGetValue(name, out var value) && value is null;
  }
}
