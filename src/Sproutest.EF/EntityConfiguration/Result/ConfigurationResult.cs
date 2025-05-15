using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result
{
    public abstract class IdentifierBase : IKeyIdentifier
    {
        private readonly MethodInfo _getIdMember;
        private readonly MethodInfo _setIdMember;

        public IdentifierBase(EntityProperty property)
        {
            ArgumentNullException.ThrowIfNull(property, nameof(property));
            ArgumentNullException.ThrowIfNull(property.Info, nameof(property.Info));

            Property = property;

            _getIdMember = property.Info.GetGetMethod() ??
                throw new MissingMemberException($"Cannot get method {property.Info.Name} from {property.Info.ReflectedType?.Name}.");

            _setIdMember = property.Info.SetMethod ??
                property.Info.GetSetMethod(true) ??
                throw new MissingMemberException($"Cannot get method {property.Info.Name} from {property.Info.ReflectedType?.Name}.");
        }

        public EntityProperty Property { get; init; }

        public abstract bool IsEmpty<TEntity>(TEntity entity);

        public object? ReadValueFrom<TEntity>(TEntity entity)
        {
            return _getIdMember.Invoke(entity, null);
        }

        public virtual TEntity WithId<TEntity>(TEntity entity, object id)
        {
            _setIdMember.Invoke(entity, [id]);
            return entity;
        }

        public abstract TEntity WithRandomId<TEntity>(TEntity entity);

        public virtual TEntity WithRandomIdIfEmpty<TEntity>(TEntity entity)
        {
            return IsEmpty(entity)
                ? WithRandomId(entity)
                : entity;
        }
    }

    public class GuidIdentifier : IdentifierBase
    {
        public GuidIdentifier(EntityProperty property) : base(property)
        {
        }

        public override TEntity WithRandomId<TEntity>(TEntity entity)
        {
            return WithId(entity, Guid.NewGuid());
        }

        public override bool IsEmpty<TEntity>(TEntity entity)
        {
            return GetIdValue(entity) == Guid.Empty;
        }

        private Guid GetIdValue<TEntity>(TEntity entity)
        {
            return (Guid?)ReadValueFrom(entity) ?? Guid.Empty;
        }
    }

    public class Int32Identifier : IdentifierBase
    {
        private int _sequence = 1;

        public Int32Identifier(EntityProperty property) : base(property)
        {
        }

        public override TEntity WithRandomId<TEntity>(TEntity entity)
        {
            return WithId(entity, _sequence++);
        }

        public override bool IsEmpty<TEntity>(TEntity entity)
        {
            return GetIdValue(entity) == 0;
        }

        private int GetIdValue<TEntity>(TEntity entity)
        {
            return (int?)ReadValueFrom(entity) ?? 0;
        }
    }

    public class Int64Identifier : IdentifierBase
    {
        private long _sequence = 1;

        public Int64Identifier(EntityProperty property) : base(property)
        {
        }

        public override TEntity WithRandomId<TEntity>(TEntity entity)
        {
            return WithId(entity, _sequence++);
        }

        public override bool IsEmpty<TEntity>(TEntity entity)
        {
            return GetIdValue(entity) == 0;
        }

        private long GetIdValue<TEntity>(TEntity entity)
        {
            return (long?)ReadValueFrom(entity) ?? 0;
        }
    }

    public static class KeyIdentifierFactory
    {
        private static readonly Dictionary<Type, Func<EntityProperty, IKeyIdentifier>> _factories = new()
        {
            { typeof(Guid), property => new GuidIdentifier(property) },
            { typeof(int), property => new Int32Identifier(property) },
            { typeof(long), property => new Int64Identifier(property) },
        };

        public static void Add<T>(Func<EntityProperty, IKeyIdentifier> factoryMethod)
        {
            ArgumentNullException.ThrowIfNull(factoryMethod, nameof(factoryMethod));

            _factories.TryAdd(typeof(T), factoryMethod);
        }

        public static void Replace<T>(Func<EntityProperty, IKeyIdentifier> factoryMethod)
        {
            ArgumentNullException.ThrowIfNull(factoryMethod, nameof(factoryMethod));

            var type = typeof(T);
            _factories.Remove(type);
            _factories.Add(type, factoryMethod);
        }

        public static IKeyIdentifier CreateInstanceOf(EntityProperty property)
        {
            if (!_factories.TryGetValue(property.PropertyType, out var factory))
            {
                throw new NotSupportedException($"Not registered factory to type {property.PropertyType.Name}.");
            }

            return factory(property);
        }
    }

    [DebuggerDisplay("{DisplayName}")]
    public class ConfigurationResult<T> : IConfigurationResult
    {
        private readonly Dictionary<PropertyInfo, EntityProperty> _properties = new();

        public ConfigurationResult(Type entityType)
        {
            EntityType = entityType;
        }

        public IKeyIdentifier? Id { get; private set; } = null;

        public string DisplayName => EntityType.Name;

        public Type EntityType { get; }

        public string TableName { get; private set; }

        public KeyStatus Key { get; private set; }

        public IReadOnlyDictionary<PropertyInfo, EntityProperty> Properties => _properties;

        public string[] NonMappedKeys { get; private set; }

        internal void SetTableName(string name)
            => TableName = name;

        internal EntityProperty AddKey<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            ArgumentNullException.ThrowIfNull(propertyExpression, nameof(propertyExpression));

            Key = KeyStatus.Mapped;

            var property = GetOrAddProperty(propertyExpression);
            property.IsKey = true;

            Id = KeyIdentifierFactory.CreateInstanceOf(property);

            return property;
        }

        internal void AddKey(string[] keys)
        {
            ArgumentNullException.ThrowIfNull(keys, nameof(keys));

            if (!keys.Any())
            {
                throw new ArgumentException("Should contain at least one key.", nameof(keys));
            }

            Key = KeyStatus.NotMapped;
            NonMappedKeys = keys;
        }

        internal void HasNoKey()
        {
            Key = KeyStatus.None;
            NonMappedKeys = null;
        }

        internal EntityProperty Configure<TProperty>(Expression<Func<T, TProperty>> propertyExpression, bool isReference = false)
        {
            var property = GetOrAddProperty(propertyExpression);
            property.IsConfigured = true;
            property.IsReference = isReference;
            return property;
        }

        internal EntityProperty Configure(string propertyName, bool isReference = false)
        {
            var property = GetOrAddProperty(propertyName);
            property.IsConfigured = true;
            property.IsReference = isReference;
            return property;
        }

        internal EntityProperty Configure(Type propertyType, string propertyName, bool isReference = false)
        {
            var property = GetOrAddProperty(propertyName, new EntityProperty(propertyType));
            property.ColumnName = propertyName; // note: não tenho certeza se é necessário ou se é atribuído em outro momento.
            property.IsConfigured = true;
            property.IsReference = isReference;
            return property;
        }

        internal void Ignore<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var property = GetOrAddProperty(propertyExpression);
            property.IsIgnored = true;
        }

        internal PropertyInfo? GetProperty(string propertyName)
        {
            var criteria = Properties.Where(p => p.Key.Name == propertyName);
            return criteria.Any() ? criteria.First().Key : null;
        }

        private EntityProperty GetOrAddProperty<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            var memberExpression = propertyExpression.Body;

            if (memberExpression is UnaryExpression unaryExpression && unaryExpression.NodeType == ExpressionType.Convert)
            {
                memberExpression = unaryExpression.Operand;
            }

            if (memberExpression is not MemberExpression member || member.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException($"Expression '{propertyExpression}' is not a valid property.", nameof(propertyExpression));
            }

            if (Properties.TryGetValue(propertyInfo, out var property))
            {
                return property;
            }

            var newProperty = new EntityProperty(propertyInfo);
            _properties[propertyInfo] = newProperty;
            return newProperty;
        }

        private EntityProperty GetOrAddProperty(string propertyName, EntityProperty? defaultProperty = null)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            var propertyInfo = EntityType.GetProperty(propertyName, bindingFlags);

            if (propertyInfo is null)
            {
                if (defaultProperty is not null)
                {
                    return defaultProperty;
                }

                throw new ArgumentException($"\"{EntityType.FullName}\" does not have a property named \"{propertyName}\".", nameof(propertyName));
            }

            if (propertyInfo.DeclaringType != propertyInfo.ReflectedType)
            {
                propertyInfo = propertyInfo.DeclaringType.GetProperty(propertyName, bindingFlags);
            }

            var newProperty = new EntityProperty(propertyInfo);
            _properties[propertyInfo] = newProperty;
            return newProperty;
        }

        public IRelatedPropertyInfo? GetRelationshipFromProperty(PropertyInfo property)
            => _properties.TryGetValue(property, out var propertyInfo) ? propertyInfo : null;
    }
}
