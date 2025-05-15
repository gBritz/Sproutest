using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Moq;
using Sproutest.EF.EntityConfiguration;
using Sproutest.EF.Extensions;
using Sproutest.Extensions;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.Moq;

public class MockDbContextBuilder<T> :
    MockDbContextBuilderBase
    where T : DbContext
{
    private static readonly MethodInfo AddEntityIdGeneratorMethod = typeof(MockDbContextBuilder<T>)
        .GetMethod(nameof(MockDbContextBuilder<T>.AddEntityIdGenerator), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new MissingMethodException(typeof(MockDbContextBuilder<T>).Name, nameof(MockDbContextBuilder<T>.AddEntityIdGenerator));
    private static readonly Type _dbContextType = typeof(T);

    private readonly Mock<T> _mock;

    public MockDbContextBuilder()
        : this(new())
    {
    }

    public MockDbContextBuilder(Mock<T> mock)
        : base(typeof(T))
    {
        _mock = mock ?? throw new ArgumentNullException(nameof(mock));
        _mock.DefaultValueProvider = new FallbackDbSetValueProvider();
    }

    public Mock<T> Mock => _mock;

    public static void DynamicCallToAddEntityIdGenerator(
        MockDbContextBuilderBase instance,
        Type baseEntityType,
        bool initializeRelatedEntities,
        //EntityFillerBase filler,
        EntityGraph graph)
    {
        ArgumentNullException.ThrowIfNull(baseEntityType, nameof(baseEntityType));
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        MethodInfo genericAddEntityIdGeneratorMethod = AddEntityIdGeneratorMethod.MakeGenericMethod(baseEntityType);
        genericAddEntityIdGeneratorMethod.Invoke(instance, [
            initializeRelatedEntities,
            //filler,
            graph]);
    }

    public MockDbContextBuilder<T> EmptyAll()
    {
        var properties = GetAllDbSetsProperties();

        foreach (var property in properties)
        {
            var lambda = _dbContextType.MakeExpressionByProperty(property);
            var genericTypeOfProperty = property.PropertyType.GenericTypeArguments[0];
            var emptyArray = CreateEmptyArrayOf(genericTypeOfProperty);
            CreateDbSetOfType(_mock, lambda, genericTypeOfProperty, emptyArray);
        }

        return this;
    }

    public T Build() => _mock.Object;

    public void AddValues(IEnumerable<KeyValuePair<Type, IEnumerable<dynamic>>> entities)
    {
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        foreach (var entityValues in entities)
        {
            var expression = MakeExpressionByType(entityValues.Key);
            var castedValues = DynamicArrayCast(entityValues.Value, entityValues.Key);

            CreateDbSetOfType(_mock, expression, entityValues.Key, castedValues);
        }
    }

    public MockDbContextBuilder<T> AddEntityIdGenerator<TBaseEntity>(
        bool initializeRelatedEntities,
        EntityGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        var dbSetProperties = GetAllDbSetsProperties()
            .ToDictionary(x => x.PropertyType.GenericTypeArguments.First(), x => x);

        Func<int> mockSaveChanges = () =>
        {
            var entities = dbSetProperties.Values
                .Select(property => property.GetValue(_mock.Object))
                .SelectMany(dbSet => dbSet as IEnumerable<TBaseEntity> ?? [])
                .Select(entity => graph.GetConfigurationFromEntityType(entity.GetType()).Id.WithRandomIdIfEmpty(entity))
                .ToArray();
            var totalNewEntities = 0;

            // note: propagar alteração pelos relacionamentos
            if (initializeRelatedEntities)
            {
                EntityInitializer
                    //.Run(entities, filler, graph)
                    .Run(entities, graph)
                    .ForEach(newEntity =>
                    {
                        if (dbSetProperties.TryGetValue(newEntity.GetType(), out var dbSetProperty))
                        {
                            // TODO: tipar o método add em uma constante.
                            var dbSet = dbSetProperty.GetValue(_mock.Object);
                            var add = dbSet.GetType().GetMethod(nameof(DbSet<object>.Add));
                            add.Invoke(dbSet, [newEntity]);
                        }
                        totalNewEntities++;
                    });
            }

            return entities.Length + totalNewEntities;
        };

        MockResultOFSaveChanges(mockSaveChanges);

        return this;
    }

    public MockDbContextBuilder<T> MockResultOFSaveChanges(Func<int> resultValue)
    {
        ArgumentNullException.ThrowIfNull(resultValue, nameof(resultValue));

        _mock
            .Setup(x => x.SaveChanges())
            .Returns(resultValue);

        _mock
            .Setup(x => x.SaveChanges(It.IsAny<bool>()))
            .Returns(resultValue);

        Func<Task<int>> mockSaveChangesAsync = () => resultValue().AsTask();

        _mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(mockSaveChangesAsync);

        _mock
            .Setup(x => x.SaveChangesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Returns(mockSaveChangesAsync);

        return this;
    }

    // todo: descobrir motivo disso.
    public MockDbContextBuilder<T> AddDatabase()
    {
        _mock.SetupGet(x => x.Database)
            .Returns(new Mock<DatabaseFacade>(_mock.Object).Object);

        return this;
    }

    public MockDbContextBuilder<T> FakeChangeTracker()
    {
        // TODO: ver se tem como fazer isso...
        var mockChangeTracker = new Mock<ChangeTracker>([
            _mock.Object,
    null,
    null,
    null,
    null]);
        mockChangeTracker.Setup(_ => _.Entries())
            .Returns([]);
        // IDbContextServices
        _mock.Setup(_ => _.ChangeTracker)
            .Returns(mockChangeTracker.Object);

        return this;
    }

    private class FallbackDbSetValueProvider : DefaultValueProvider
    {
        private static readonly Type DbSetTypeDefinition = typeof(Microsoft.EntityFrameworkCore.DbSet<>);

        protected override object GetDefaultValue(Type type, Mock mock)
        {
            if (IsDbSet(type))
            {
                var entityType = type.GetGenericArguments()[0];
                var dbContextType = mock.GetType().GetGenericArguments()[0];

                return MockDbSetOfType(mock, dbContextType, null, entityType, null);
            }

            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool IsDbSet(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition() == DbSetTypeDefinition;
    }
}