using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using Sproutest.EF.EntityFramework.QueryProviders;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Sproutest.EF.EntityFramework.InMemory
{
    public class InMemoryDbSet<T> : DbSet<T>, IQueryable<T>, IAsyncEnumerable<T>
      where T : class
  {
    private readonly ObservableCollection<T> _collection;

    public InMemoryDbSet(IEnumerable<T> entities)
    {
      _collection = new(entities ?? []);
    }

    public override IEntityType EntityType => throw new NotImplementedException();

    public Type ElementType { get => _collection.AsQueryable().ElementType; }

    public Expression Expression { get => _collection.AsQueryable().Expression; }

    public IQueryProvider Provider => new InMemoryAsyncQueryProviderWithMocks<T>(_collection.AsQueryable().Provider);

    public IEnumerator<T> GetEnumerator() =>
      _collection.AsQueryable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
      _collection.AsQueryable().GetEnumerator();

    public override IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
      new InMemoryDbAsyncEnumerator<T>(_collection.GetEnumerator());

    public override EntityEntry<T> Add(T entity)
    {
      _collection.Add(entity);
      return null;
    }

    public override void AddRange(IEnumerable<T> entitys)
    {
      foreach (var entity in entitys)
      {
        _collection.Add(entity);
      }
    }

    public override EntityEntry<T> Remove(T entity)
    {
      _collection.Remove(entity);
      return null;
    }

    public override void RemoveRange(IEnumerable<T> entitys)
    {
      foreach (var entity in entitys)
      {
        _collection.Remove(entity);
      }
    }

    public override EntityEntry<T> Update(T entity)
    {
      return null;
    }

    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "Using internal API to mock DbContext.Attach")]
    public override EntityEntry<T> Attach(T entity)
    {
      var runtimeEntity = new RuntimeEntityType(
        typeof(T).Name,
        typeof(T),
        false,
        null,
        null,
        null,
        ChangeTrackingStrategy.Snapshot,
        null,
        false,
        null);

      var internalEntityEntry = new InternalEntityEntry(
        new Mock<IStateManager>().Object,
        runtimeEntity,
        entity);

      return new Mock<EntityEntry<T>>(internalEntityEntry).Object;
    }

    public override ValueTask<T> FindAsync(object[] keyValues, CancellationToken cancellationToken)
    {
      return FindAsync(keyValues);
    }

    public override ValueTask<T> FindAsync(params object[] keyValues)
    {
      return new ValueTask<T>(_collection.FirstOrDefault());
    }
  }
}
