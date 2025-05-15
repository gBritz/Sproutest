using System.Linq.Expressions;
using System.Reflection;

namespace Sproutest.EF.Extensions;

/*
[Obsolete]
public abstract class EntityKeyBase
{
    private readonly Type _typeEntity;

    public Type TypeEntity => _typeEntity;

    public EntityKeyBase(Type typeEntity)
    {
        _typeEntity = typeEntity ?? throw new ArgumentNullException(nameof(typeEntity));
    }
}

[Obsolete]
public class EntityKey<T> : EntityKeyBase
{

    private readonly MethodInfo _getIdMember;
    private readonly MethodInfo _setIdMember;

    public EntityKey()
        : this("Id")
    { }

    public EntityKey(Expression<Func<T, object>> expression)
        : this(expression.GetPropertyFromExpression())
    { }

    public EntityKey(string propertyIdName)
        : this(GetPropertyFromName(propertyIdName))
    {
    }

    protected EntityKey(PropertyInfo property)
        : base(typeof(T))
    {
        ArgumentNullException.ThrowIfNull(property, nameof(property));

        _getIdMember = property.GetGetMethod()
            ?? throw new MissingMemberException($"Cannot get method {property.Name} from {TypeEntity.Name}.");
        _setIdMember = property.GetSetMethod(true)
            ?? throw new MissingMemberException($"Cannot get method {property.Name} from {TypeEntity.Name}.");
    }

    public T WithRandomId(T entity)
    {
        return WithId(entity, Guid.NewGuid());
    }

    public T WithRandomIdIfEmpty(T entity)
    {
        return IdIsEmpty(entity)
            ? WithRandomId(entity)
            : entity;
    }

    public bool IdIsEmpty(T entity)
    {
        return GetIdValue(entity) == Guid.Empty;
    }

    public object ReadIdValue(T entity)
    {
        return GetIdValue(entity);
    }

    public T WithId(T entity, object id)
    {
        _setIdMember.Invoke(entity, [id]);
        return entity;
    }

    public T AddToReadOnlyColletion<TItem>(
        T entity,
        Expression<Func<T, IReadOnlyCollection<TItem>>> expression,
        TItem item)
    {
        var collection = expression.Compile()
            .Invoke(entity);

        if (collection is ICollection<TItem> modifiableCollection && !modifiableCollection.IsReadOnly)
        {
            modifiableCollection.Add(item);
            return entity;
        }

        var propertyInfo = expression.GetPropertyFromExpression();

        var nameInCamelCase = propertyInfo.Name.ToCamelCase();
        var nameWithUnderscore = $"_{nameInCamelCase}";
        var flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;

        var backingField = TypeEntity.GetField(propertyInfo.Name, flags)
            ?? TypeEntity.GetField(nameWithUnderscore, flags)
            ?? throw new InvalidOperationException($"Could not find a field named '{nameInCamelCase}' or '{nameWithUnderscore}'.");

        var backingColletion = backingField.GetValue(entity);

        if (backingColletion is ICollection<TItem> modifiableBackingCollection && !modifiableBackingCollection.IsReadOnly)
        {
            modifiableBackingCollection.Add(item);
            return entity;
        }

        throw new InvalidOperationException($"'{backingField.Name}' is not a modifiabled colletion.");
    }

    public IEnumerable<T> GetAllRelatedEntities(T entity)
    {
        var test = new List<T>();
        if (GetIdValue(entity) == Guid.Empty)
        {
            test.Add(entity);
        }

        var visited = new HashSet<T> { WithRandomIdIfEmpty(entity) };
        var current = new Stack<T>(visited);

        while (current.TryPop(out var currentEntity))
        {
            var properties = currentEntity.GetType()
              .GetProperties(BindingFlags.Instance | BindingFlags.Public)
              .Where(p => p.PropertyType.IsAssignableTo(TypeEntity) ||
                          p.PropertyType.IsAssignableTo(typeof(IEnumerable)));

            foreach (var property in properties)
            {
                var value = property.GetValue(currentEntity);

                if (value is T newEntity && visited.Add(WithRandomIdIfEmpty(newEntity)))
                {
                    test.Add(newEntity);
                    current.Push(newEntity);
                }
                else if (value is IEnumerable<T> newEntities)
                {
                    newEntities
                      .Where(e => visited.Add(WithRandomIdIfEmpty(e)))
                      .ToList()
                      .ForEach(e =>
                      {
                          test.Add(e);
                          current.Push(e);
                      });
                }
            }
        }

        return test;
    }

    private Guid GetIdValue(T instance)
    {
        return (Guid)_getIdMember.Invoke(instance, null);
    }

    private static PropertyInfo GetPropertyFromName(string propertyName)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));

        var type = typeof(T);
        return type.GetProperty(propertyName)
            ?? throw new MissingMemberException(type.Name, propertyName);
    }
}
*/

internal static class EntityExtensions
{
    /*
    public static TEntity SetProperty<TEntity, TValue>(this TEntity entity, Expression<Func<TEntity, TValue>> expression, TValue value)
    {
        var propertyInfo = GetPropertyFromExpression(expression);
        propertyInfo.SetValue(entity, value);
        return entity;
    }

    public static TEntity SetAsNull<TEntity, TValue>(this TEntity entity, Expression<Func<TEntity, TValue>> expression)
    {
        var propertyInfo = GetPropertyFromExpression(expression);
        propertyInfo.SetValue(entity, null);
        return entity;
    }
    */

    internal static PropertyInfo GetPropertyFromExpression<TEntity, TValue>(this Expression<Func<TEntity, TValue>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression, nameof(expression));

        MemberExpression? member = null;

        if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression member1)
        {
            member = member1;
        }
        else if (expression.Body is MemberExpression member2)
        {
            member = member2;
        }

        if (member is not null && member.Member is PropertyInfo propertyInfo)
        {
            return propertyInfo;
        }

        throw new ArgumentException($"Expression '{expression}' is not a property.", nameof(expression));
    }
}