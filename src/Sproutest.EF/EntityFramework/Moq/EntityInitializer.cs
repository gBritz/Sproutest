using Sproutest.EF.EntityConfiguration;
using System.Reflection;

namespace Sproutest.EF.EntityFramework.Moq;

internal class EntityInitializer
{
    public static List<TBaseEntity> Run<TBaseEntity>(
        IEnumerable<TBaseEntity> entities,
        EntityGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        var visited = entities
          .Select(entity => graph.GetConfigurationFromEntityType(entity.GetType()).Id.WithRandomIdIfEmpty(entity))
          .ToHashSet();

        var current = new Stack<TBaseEntity>(visited);
        var newEntities = new List<TBaseEntity>();

        while (current.TryPop(out var entity))
        {
            var entityType = entity.GetType();
            var properties = entityType
              .GetProperties(BindingFlags.Instance | BindingFlags.Public)
              .Where(p => !p.PropertyType.IsPrimitive && p.PropertyType != typeof(string))
              .Where(p => p.PropertyType.IsAssignableTo(typeof(TBaseEntity)) ||
                          p.PropertyType.IsAssignableTo(typeof(IEnumerable<TBaseEntity>)));

            foreach (var property in properties)
            {
                var value = property.GetValue(entity);
                if (value is null)
                {
                    continue;
                }

                var entityKey = graph.GetConfigurationFromEntityType(value.GetType())?.Id;

                if (value is IEnumerable<TBaseEntity> newRelatedEntities)
                {
                    foreach (var newRelatedEntity in newRelatedEntities)
                    {
                        var relatedEntityKey = graph.GetConfigurationFromEntityType(newRelatedEntity.GetType()).Id;

                        if (relatedEntityKey.IsEmpty(newRelatedEntity) && visited.Add(relatedEntityKey.WithRandomIdIfEmpty(newRelatedEntity)))
                        {
                            newEntities.Add(newRelatedEntity);
                            current.Push(newRelatedEntity);
                        }
                    }
                }
                else if (value is TBaseEntity relatedEntity && entityKey.IsEmpty(relatedEntity) && visited.Add(entityKey.WithRandomIdIfEmpty(relatedEntity)))
                {
                    newEntities.Add(relatedEntity);
                    current.Push(relatedEntity);

                    var idValue = entityKey.ReadValueFrom(relatedEntity);

                    graph?.GetConfigurationFromEntityType(entityType)?
                        .GetRelationshipFromProperty(property)?
                        .PropertyForeignKey?
                        .SetValue(entity, idValue);
                }
            }
        }

        return newEntities;
    }
}