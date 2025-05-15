using System.Linq.Expressions;

namespace Sproutest.Seeding;

public class MemoryContainer : ISeedContext
{
    private readonly Dictionary<Type, object> instancesByType = [];

    //private readonly SqlQueryContext queryContext = new();

    public void Add<T>(T instance)
      where T : class =>
      AddRange(instance);

    public void AddRange<T>(params T[] instances)
      where T : class
    {
        var type = typeof(T);
        List<T> list;

        if (!instancesByType.ContainsKey(type))
        {
            list = new List<T>();
            instancesByType.Add(type, list);
        }
        else
        {
            list = (List<T>)instancesByType[type];
        }

        // TODO: inserir random id from based type
        /*
        if (instances is BaseEntity[] entities)
        {
          entities
            .Where(entity => entity.Id == Guid.Empty)
            .ForEach(entity => entity.WithRandomId());
        }
        */

        list.AddRange(instances);
    }

    public void Remove<T>(T instance) =>
      GetList<T>()?.Remove(instance);

    public T LastOrDefault<T>()
      where T : class
    {
        var list = GetList<T>();
        return list != null ? list.LastOrDefault() : default;
    }

    public T Last<T>()
      where T : class
    {
        return LastOrDefault<T>()
          ?? throw new InvalidOperationException($"An instance of \"{typeof(T).Name}\" was not added.");
    }

    public T Find<T>(Expression<Func<T, bool>> criteria)
      where T : class =>
      Filter(criteria).FirstOrDefault()
        ?? throw new InvalidOperationException($"An instance of \"{typeof(T).Name}\" was not added.");

    public T FindOrDefault<T>(Expression<Func<T, bool>> criteria)
      where T : class =>
      Filter(criteria).FirstOrDefault();

    public IEnumerable<T> Filter<T>(Expression<Func<T, bool>> criteria)
      where T : class
    {
        ArgumentNullException.ThrowIfNull(criteria, nameof(criteria));

        var compiled = criteria.Compile();
        var list = GetList<T>();
        return list != null ? list.Where(compiled) : Enumerable.Empty<T>();
    }

    public IEnumerable<T> GetInstances<T>()
      where T : class =>
      GetList<T>() ?? Enumerable.Empty<T>();

    public IEnumerable<KeyValuePair<Type, IEnumerable<dynamic>>> GetItems()
    {
        foreach (var item in instancesByType)
        {
            yield return new KeyValuePair<Type, IEnumerable<dynamic>>(item.Key, item.Value as IEnumerable<dynamic>);
        }
    }

    /*
    public void AddQueryResult<T>(Action<SqlQueryResultCollection<T>> query)
      where T : class =>
      queryContext.Add(query);

    public void AddMultipleQuery(Action<SqlMultipleQueryResultCollection> query) =>
      queryContext.AddMultiples(query);

    public SqlQueryContext GetQueryResults() => queryContext;
    */

    private List<T> GetList<T>()
    {
        List<T> list = null;
        var type = typeof(T);

        if (instancesByType.ContainsKey(type))
        {
            list = (List<T>)instancesByType[typeof(T)];
        }

        return list;
    }
}