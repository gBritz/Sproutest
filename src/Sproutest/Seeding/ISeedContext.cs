using System.Linq.Expressions;

namespace Sproutest.Seeding;

public interface ISeedContext
{
    T LastOrDefault<T>()
      where T : class;

    T Last<T>()
      where T : class;

    T Find<T>(Expression<Func<T, bool>> criteria)
      where T : class;

    T FindOrDefault<T>(Expression<Func<T, bool>> criteria)
      where T : class;

    IEnumerable<T> Filter<T>(Expression<Func<T, bool>> criteria)
      where T : class;

    void Add<T>(T instance)
      where T : class;

    void AddRange<T>(params T[] instances)
      where T : class;

    IEnumerable<T> GetInstances<T>()
      where T : class;

    /*
    void AddQueryResult<T>(Action<SqlQueryResultCollection<T>> query)
      where T : class;

    void AddMultipleQuery(Action<SqlMultipleQueryResultCollection> query);
    */
}