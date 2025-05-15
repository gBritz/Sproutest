using Microsoft.EntityFrameworkCore;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration;
using Sencinet.DigitalJourney.TestingLibrary.EntityFramework.EntityConfiguration.Result;
using Sproutest.EF.Extensions;
using System.Collections.Concurrent;
using System.Reflection;

namespace Sproutest.EF.EntityConfiguration;

public class EntityGraph
{
    private static readonly ConcurrentDictionary<Assembly, EntityGraph> _entityGraphs = new();

    private readonly ICollection<IConfigurationResult> _configurations;

    private EntityGraph(ICollection<IConfigurationResult> configurations)
    {
        _configurations = configurations;
    }

    public required Type EntityBaseType { get; init; }

    public static EntityGraph? Analyse(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

        if (_entityGraphs.TryGetValue(assembly, out var graph))
        {
            return graph;
        }

        var entities = FindMapEntitiesFrom(assembly).ToArray();
        if (entities.Length == 0)
        {
            return null;
        }

        var validator = new DbContextConfigurationValidator(entities);
        var configurations = validator.Run();

        EntityGraph newGraph = new(configurations)
        {
            EntityBaseType = GetFirstEntityBaseUsage(assembly, configurations),
        };

        _entityGraphs.AddOrUpdate(assembly, newGraph, (a, g) =>
        {
            // TODO: pode ocorrer concorrência ao tentar executar testes assíncronos.
            //      obter a diferença
            return g;
        });

        return newGraph;
    }

    public IConfigurationResult? GetConfigurationFromEntityType(Type entityType) =>
        _configurations.FirstOrDefault(c => c.EntityType == entityType);

    private static Type GetFirstEntityBaseUsage(
        Assembly assembly,
        ICollection<IConfigurationResult> configurations)
    {
        var entityBaseRanking = configurations
            .Where(c => c.Id?.Property.Info?.ReflectedType is not null)
            .Select(c => c.Id!.Property.Info!.ReflectedType!)
            .GroupBy(k => k)
            .Select(k => new
            {
                EntityName = k.Key.Name,
                EntityType = k.Key,
                Count = k.Count(),
            })
            .OrderByDescending(_ => _.Count);

        var topOne = entityBaseRanking.FirstOrDefault();

        if (topOne is null)
        {
            throw new InvalidOperationException($"Not found entity base type from assembly {assembly.FullName}.");
        }

        return topOne.EntityType;
    }

    private static IEnumerable<EntityTypeInfo> FindMapEntitiesFrom(Assembly assembly)
    {
        var mappingType = typeof(IEntityTypeConfiguration<>);

        return assembly
            .GetClassesByGenericInterface(mappingType)
            .Select(type => new EntityTypeInfo
            {
                EntityType = type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == mappingType)
                    .GetGenericArguments()
                    .Single(),
                ConfigurationType = type,
            });
    }
}