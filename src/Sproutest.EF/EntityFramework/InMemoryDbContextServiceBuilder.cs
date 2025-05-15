using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Sproutest.EF.EntityConfiguration;
using Sproutest.Seeding;
using System.Reflection;

namespace Sproutest.EF.EntityFramework;

/// <summary>
/// Represents builder to configure DbContext in memory mocking virtual DbSet properties.
/// </summary>
/// <param name="services">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
public sealed class InMemoryDbContextServiceBuilder<T>(IServiceCollection services)
    where T : DbContext
{
    private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly DbContextFactoryConfiguration _configuration = new();
    private Func<IServiceProvider, Mock<T>>? _mockDbContexActivator = null;
    private Func<IServiceProvider, object[]>? _parametersFactory = null;

    public InMemoryDbContextServiceBuilder<T> WithMockActivator(
        Func<IServiceProvider, Mock<T>> mockActivator)
    {
        _mockDbContexActivator = mockActivator;
        return this;
    }

    public InMemoryDbContextServiceBuilder<T> WithParameters(
        Func<IServiceProvider, object[]> parametersFactory)
    {
        _parametersFactory = parametersFactory;
        return this;
    }

    /// TODO:...
    /// <summary>
    /// Informar propriedades das entidades que foram alteradas no EF para uso do método ChangeTracker.Entries.
    /// </summary>
    /// public Action<DbTrackEntities>? DbTrackChanges { get; set; }

    /// <summary>
    /// Adding seeder to create data into mocked DbSet in memory.
    /// </summary>
    /// <param name="seederAcesssor">To retrieve created instance.</param>
    /// <returns><see cref="InMemoryDbContextServiceBuilder{T}"/></returns>
    public InMemoryDbContextServiceBuilder<T> WithSeeder(
      //Func<IServiceProvider, Mock<T>> mockDbContextActivator,
      Action<ISeedContext>? seederAcesssor)
    {
        //ArgumentNullException.ThrowIfNull(mockDbContextActivator, nameof(mockDbContextActivator));
        //ArgumentNullException.ThrowIfNull(seederAcesssor, nameof(seederAcesssor));

        _services.RemoveAll<T>();

        UpdateConfiguration();

        _services.AddSingleton<DbContextFactory>();
        _services.AddSingleton<T>(provider =>
          provider.GetRequiredService<DbContextFactory>().Create<T>(
            _mockDbContexActivator is not null ? () => _mockDbContexActivator(provider) : () => DefaultMockDbContexActivator(provider),
            seederAcesssor ?? (seeder => { })));

        return this;
    }

    public InMemoryDbContextServiceBuilder<T> AnalyseMappingsFrom(Assembly assembly)
    {
        var graph = EntityGraph.Analyse(assembly);

        if (graph is not null)
        {
            _configuration.Graph = graph;
            _configuration.InitializeRelatedEntities = true;
            UpdateConfiguration();
        }

        return this;
    }

    /// <summary>
    /// Cria nova instância mockada de <see cref="T"/>.
    /// </summary>
    /// <param name="services">Provedor de serviços</param>
    /// <returns>Mock de <see cref="T"/></returns>
    private Mock<T> DefaultMockDbContexActivator(IServiceProvider provider)
    {
        object[] parameters = _parametersFactory?.Invoke(provider) ?? [];
        Mock<T> mockMesaDbContext = new(parameters);

        //var mockMesaDbContext = RegisterMock<DefaultContext>(new(
        //  new Mock<T>(new DbContextOptions<T>()));

        /*
        ICollection<EntityEntry> trackerEntriesResult = []; // note: precisa sempre retornar alguma coisa porque sempre é consultado o contexto do EF para saber se houve alteração para registrar no log. Ocorre exception se não mockar resultado, mesmo que vazio.

        if (DbTrackChanges != null)
        {
            Dictionary<Type, EntityChange> types = [];
            DbTrackChanges(new DbTrackEntities(types));
            trackerEntriesResult = EntityEntryMocker.MockFor(types);
        }

        //mockMesaDbContext.Setup(_ => _.GetChangeTrackerEntries())
        //   .Returns(trackerEntriesResult);
        */

        return mockMesaDbContext;
    }

    private void UpdateConfiguration()
    {
        _services.RemoveAll<DbContextFactoryConfiguration>();
        _services.AddSingleton<DbContextFactoryConfiguration>(_configuration);
    }
}