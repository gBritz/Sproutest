using Microsoft.EntityFrameworkCore;
using Moq;
using Sproutest.EF.EntityConfiguration;
using Sproutest.EF.EntityFramework.Moq;
using Sproutest.Seeding;

namespace Sproutest.EF.EntityFramework;

internal class DbContextFactoryConfiguration
{
    public bool InitializeRelatedEntities { get; set; } = false;

    public EntityGraph? Graph { get; set; }
}

internal class DbContextFactory
{
    private readonly DbContextFactoryConfiguration _configuration;

    public DbContextFactory(DbContextFactoryConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public TDbContext Create<TDbContext>(
      Func<Mock<TDbContext>> mockDbContextActivator,
      Action<ISeedContext>? method = null)
      where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(mockDbContextActivator, nameof(mockDbContextActivator));

        var instances = new MemoryContainer();

        method?.Invoke(instances);

        var mockDbContext = mockDbContextActivator();

        var contextBuilder = new MockDbContextBuilder<TDbContext>(mockDbContext)
          .EmptyAll();

        contextBuilder.AddValues(instances.GetItems());

        // contextBuilder.AddQueryResults(instances.GetQueryResults());

        if (_configuration.Graph is not null)
        {
            MockDbContextBuilder<TDbContext>.DynamicCallToAddEntityIdGenerator(
                contextBuilder,
                _configuration.Graph.EntityBaseType,
                _configuration.InitializeRelatedEntities,
                _configuration.Graph);
        }
        else
        {
            contextBuilder.MockResultOFSaveChanges(() => 0);
        }

        // contextBuilder.AddTransaction();

        contextBuilder.AddDatabase();

        // contextBuilder.FakeChangeTracker();

        return contextBuilder.Build();
    }
}