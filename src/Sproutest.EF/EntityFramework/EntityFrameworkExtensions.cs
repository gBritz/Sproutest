using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Sproutest.EF.EntityFramework;

public static class EntityFrameworkExtensions
{
    public static InMemoryDbContextServiceBuilder<T> UseInMemoryDbContext<T>(this IServiceCollection services)
        where T : DbContext
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.RemoveAll<DbContextOptions<T>>();

        return new InMemoryDbContextServiceBuilder<T>(services)
            .AnalyseMappingsFrom(typeof(T).Assembly);
    }
}