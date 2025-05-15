using System.Reflection;

namespace Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result
{
    public interface IRelatedPropertyInfo
    {
        public PropertyInfo? PropertyForeignKey { get; }
    }

    public interface IKeyIdentifier
    {
        EntityProperty Property { get; }

        TEntity WithRandomId<TEntity>(TEntity entity);

        TEntity WithRandomIdIfEmpty<TEntity>(TEntity entity);

        bool IsEmpty<TEntity>(TEntity entity);

        object? ReadValueFrom<TEntity>(TEntity entity);
    }

    public interface IConfigurationResult
    {
        IKeyIdentifier? Id { get; }

        string DisplayName { get; }

        Type EntityType { get; }

        KeyStatus Key { get; }

        string TableName { get; }

        IReadOnlyDictionary<PropertyInfo, EntityProperty> Properties { get; }

        IRelatedPropertyInfo? GetRelationshipFromProperty(PropertyInfo property);
    }
}
