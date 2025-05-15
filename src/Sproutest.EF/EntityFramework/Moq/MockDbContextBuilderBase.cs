using Microsoft.EntityFrameworkCore;
using Moq;
using Sproutest.EF.EntityFramework.InMemory;
using System.Linq.Expressions;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.Moq
{
    public abstract class MockDbContextBuilderBase
  {
    private static readonly MethodInfo _createDbSet = typeof(MockDbContextBuilderBase).GetMethod(nameof(MockDbContextBuilderBase.CreateDbSet), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo _emptyArray = typeof(Array).GetMethod(nameof(Array.Empty), BindingFlags.Public | BindingFlags.Static);
    private static readonly MethodInfo _castEnumerable = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast), BindingFlags.Public | BindingFlags.Static);
    private static readonly MethodInfo _toArrayEnumerable = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), BindingFlags.Public | BindingFlags.Static);
    private static readonly Type _dbSetType = typeof(DbSet<>);
    private readonly Type _dbContextType;

    public MockDbContextBuilderBase(Type dbContextType)
    {
      _dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
    }

    public static object MockDbSetOfType(
      Mock mock,
      Type dbContextType,
      Expression expression,
      Type type,
      object values)
    {
      ArgumentNullException.ThrowIfNull(mock, nameof(mock));
      ArgumentNullException.ThrowIfNull(dbContextType, nameof(dbContextType));
      ArgumentNullException.ThrowIfNull(type, nameof(type));

      var createDbSetMethod = _createDbSet.MakeGenericMethod(dbContextType, type);
      return createDbSetMethod.Invoke(null, new object[] { mock, expression, values });
    }

    protected static object CreateEmptyArrayOf(Type type)
    {
      var method = _emptyArray.MakeGenericMethod(type);
      return method.Invoke(null, null);
    }

    protected static object DynamicArrayCast(object array, Type destType)
    {
      var castMethod = _castEnumerable.MakeGenericMethod(destType);
      var toArrayMethod = _toArrayEnumerable.MakeGenericMethod(destType);

      var castedArray = castMethod.Invoke(null, new[] { array });
      var valuesAsArray = toArrayMethod.Invoke(null, new[] { castedArray });

      return valuesAsArray;
    }

    protected Expression MakeExpressionByType(Type type)
    {
      ArgumentNullException.ThrowIfNull(type, nameof(type));

      var dbSetOfType = _dbSetType.MakeGenericType(type);

      var propertyInfo = _dbContextType
          .GetProperties()
          .Where(p => p.CanRead &&
                      p.GetMethod.IsVirtual &&
                      p.GetMethod.ReturnType == dbSetOfType)
          .FirstOrDefault();

      return propertyInfo == null ? null : _dbContextType.MakeExpressionByProperty(propertyInfo);
    }

    protected IEnumerable<PropertyInfo> GetAllDbSetsProperties()
    {
      return _dbContextType
          .GetProperties(BindingFlags.Instance | BindingFlags.Public)
          .Where(p => p.CanRead &&
                      p.GetMethod.IsVirtual &&
                      p.GetMethod.ReturnType.IsGenericType &&
                      p.GetMethod.ReturnType.GetGenericTypeDefinition() == _dbSetType);
    }

    protected void CreateDbSetOfType<T>(
      Mock<T> mock,
      Expression expression,
      Type type,
      object values)
      where T : DbContext
    {
      ArgumentNullException.ThrowIfNull(mock, nameof(mock));
      ArgumentNullException.ThrowIfNull(type, nameof(type));

      var createDbSetMethod = _createDbSet.MakeGenericMethod(_dbContextType, type);
      createDbSetMethod.Invoke(this, new object[] { mock, expression, values });
    }

    private static DbSet<TModel> CreateDbSet<TDbContext, TModel>(
      Mock<TDbContext> mock,
      Expression<Func<TDbContext, DbSet<TModel>>> expression,
      TModel[] results)
      where TDbContext : DbContext
      where TModel : class
    {
      var dbSet = new InMemoryDbSet<TModel>(results);

      if (expression != null)
      {
        // note: was found DbSet of type property in DbContext
        mock
          .Setup(expression)
          .Returns(dbSet);
      }

      mock
        .Setup(c => c.Set<TModel>())
        .Returns(dbSet);

      mock
        .Setup(c => c.Add(It.IsAny<TModel>()))
        .Callback<TModel>(entity => dbSet.Add(entity));

      mock
        .Setup(c => c.Remove(It.IsAny<TModel>()))
        .Callback<TModel>(entity => dbSet.Remove(entity));

      mock
        .Setup(c => c.FindAsync<TModel>(It.IsAny<object[]>()))
        .Returns((object[] args) => dbSet.FindAsync(args));

      mock
        .Setup(c => c.Attach(It.IsAny<TModel>()))
        .Returns<TModel>(dbSet.Attach);

      return dbSet;
    }
  }
}
