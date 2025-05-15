using Castle.DynamicProxy.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Sproutest.Extensions;

internal static class IServiceCollectionExtensions
  {
    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(
      this IServiceCollection services)
      where TService : class
      where TImplementation : class, TService
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));

      var foundImplementations = services
        .Where(d => d.ServiceType.GetAllInterfaces().Contains(typeof(TService)))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddSingleton<TService, TImplementation>();
    }

    public static IServiceCollection ReplaceScoped<TService, TImplementation>(
      this IServiceCollection services)
      where TService : class
      where TImplementation : class, TService
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));

      var foundImplementations = services
        .Where(d => d.ServiceType.GetAllInterfaces().Contains(typeof(TService)))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddScoped<TService, TImplementation>();
    }

    public static IServiceCollection ReplaceTransient<TService, TImplementation>(
      this IServiceCollection services,
      Func<IServiceProvider, TImplementation> implementationFactory)
      where TService : class
      where TImplementation : class, TService
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));
      ArgumentNullException.ThrowIfNull(implementationFactory, nameof(implementationFactory));

      var foundImplementations = services
        .Where(d => d.ServiceType ==typeof(TService))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddTransient<TService, TImplementation>(implementationFactory);
    }

    public static IServiceCollection ReplaceSingleton<TService, TImplementation>(
      this IServiceCollection services,
      Func<IServiceProvider, TImplementation> implementationFactory)
      where TService : class
      where TImplementation : class, TService
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));
      ArgumentNullException.ThrowIfNull(implementationFactory, nameof(implementationFactory));

      var foundImplementations = services
        .Where(d => d.ServiceType.GetAllInterfaces().Contains(typeof(TService)))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddSingleton<TService, TImplementation>(implementationFactory);
    }

    public static IServiceCollection ReplaceOnlyAbstractionSingleton<TService, TImplementation>(
      this IServiceCollection services,
      Func<IServiceProvider, TImplementation> implementationFactory)
      where TService : class
      where TImplementation : class, TService
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));
      ArgumentNullException.ThrowIfNull(implementationFactory, nameof(implementationFactory));

      var foundImplementations = services
        .Where(d => d.ServiceType.IsInterface && d.ServiceType.GetAllInterfaces().Contains(typeof(TService)))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddSingleton<TService, TImplementation>(implementationFactory);
    }

    public static IServiceCollection ReplaceSingleton<TService>(
      this IServiceCollection services,
      TService instance)
      where TService : class
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));

      var foundImplementations = services
        .Where(d => d.ServiceType.GetAllInterfaces().Contains(typeof(TService)))
        .ToArray();

      foreach (var implementation in foundImplementations)
      {
        services.Remove(implementation);
      }

      return services.AddSingleton(instance);
    }

    public static IServiceCollection RemoveAllServices<TService>(
      this IServiceCollection services)
    {
      ArgumentNullException.ThrowIfNull(services, nameof(services));

      var serviceType = typeof(TService);
      var descriptorsToRemove = services.Where(d => d.ServiceType == serviceType).ToArray();

      foreach (var serviceDescriptor in descriptorsToRemove)
      {
        services.Remove(serviceDescriptor);
      }

      return services;
    }
  }