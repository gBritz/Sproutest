using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Sproutest.Security;
using Sproutest.Seeding;

namespace Sproutest;

/// <summary>
/// Fábrica para criar novas instâncias da aplicação destino, a que irá ser testada.
/// </summary>
/// <typeparam name="TProgram">Tipo da classe Program.cs da api destino.</typeparam>
public class WebApiFactory<TProgram> : WebApplicationFactory<TProgram>
  where TProgram : class
{
    private readonly Dictionary<Type, Mock> _mocks = [];
    private HttpClient? _httpClient;

    /// <summary>
    /// Cliente http para acesso à api do projeto a ser testada.
    /// </summary>
    public HttpClient HttpClient => _httpClient ??= CreateClient();

    /// <summary>
    /// Semeador de dados para múltiplos ORMs.
    /// </summary>
    public Action<ISeedContext>? Seeder { get; set; }

    /// <summary>
    /// Injeta claims do usuário da requisição http.
    /// </summary>
    public UserClaims User { get; init; } = new();

    /// <summary>
    /// Serviços contidos na DI do projeto destino para serem subsituídos caso necessário no teste.
    /// </summary>
    public Action<IServiceCollection>? ConfigureServices { get; set; }

    /// <summary>
    /// Obtém mock do tipo especificado já criado e usado no programa.
    /// </summary>
    /// <typeparam name="T">Tipo mockado</typeparam>
    /// <returns>Mock do tipo especificado.</returns>
    /// <exception cref="Exception">Caso o mock tiver sido criado.</exception>
    public Mock<T> GetMock<T>()
      where T : class
    {
        var type = typeof(T);
        return _mocks.TryGetValue(type, out var value) && value is Mock<T> mock
          ? mock
          : throw new Exception($"There is no mock registered for type \"{type.Name}\"!");
    }

    /// <summary>
    /// Indica se há instância de mock registrado no contexto da aplicação.
    /// </summary>
    /// <typeparam name="T">Tipo da instância do mock</typeparam>
    /// <returns>True caso o mock tenha uma instância.</returns>
    public bool HasMock<T>()
      where T : class =>
      _mocks.ContainsKey(typeof(T));

    /// <summary>
    /// Obtém instância do Mock do tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo mockado.</typeparam>
    /// <returns>Instância do Mock pelo tipo escificado.</returns>
    /// <exception cref="Exception">Caso o mock tiver sido criado.</exception>
    public T GetMockedObject<T>()
      where T : class
    {
        return GetMock<T>().Object;
    }

    /// <summary>
    /// Limpar cache baseado no MemoryCache do dotnet.
    /// </summary>
    public void ClearCache()
    {
        var cache = Services.GetService<IMemoryCache>() as MemoryCache;
        cache?.Compact(1);
    }

    protected virtual void ConfigureServiceCollection(IServiceCollection services)
    {
    }

    /// <inheritdoc/>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // note: disable reload on change file watcher configuration to not occur timeout in testserver
        builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
          configBuilder.Sources.Where(s => s is FileConfigurationSource)
            .ToList()
            .ForEach(s => ((FileConfigurationSource)s).ReloadOnChange = false));

        builder.UseSetting("UseSwaggerUI", bool.FalseString);

        builder.ConfigureServices(services =>
        {
            ConfigureServiceCollection(services);

            ConfigureServices?.Invoke(services);
        });

        builder.UseEnvironment("Development");
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
                _httpClient = null;
            }
        }
    }

    /// <summary>
    /// Registra mock para consulta posterior.
    /// </summary>
    /// <typeparam name="T">Tipo do mock.</typeparam>
    /// <param name="mock">Instância do mock.</param>
    /// <returns>Mock do tipo.</returns>
    protected Mock<T> RegisterMock<T>(Mock<T> mock)
      where T : class
    {
        var key = typeof(T);
        return (_mocks.TryGetValue(key, out Mock? value) ? value : _mocks[key] = mock) as Mock<T> ?? mock;
    }
}