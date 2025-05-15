using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Sproutest.Seeding;

namespace Sproutest;

/// <summary>
/// Utilitário para testes nos endpoints.
/// </summary>
public abstract class WebApiTestBase<TFactory, TEntryPoint> : IDisposable
    where TEntryPoint : class
    where TFactory : WebApiFactory<TEntryPoint>, new()
{
    /// <summary>
    /// Cliente http para acesso à api do projeto a ser testada.
    /// </summary>
    public HttpClient HttpClient => Api.HttpClient;

    /// <summary>
    /// Serviços contidos na DI do projeto destino para serem subsituídos caso necessário no teste.
    /// </summary>
    public Action<IServiceCollection> ConfigureServices { set => Api.ConfigureServices = value; }

    /// <summary>
    /// Semeador de dados para múltiplos ORMs.
    /// </summary>
    public Action<ISeedContext> Seeder { set => Api.Seeder = value; }

    /// <summary>
    /// Obter o provedor se serviços.
    /// </summary>
    public IServiceProvider Services => Api.Services;

    /// <summary>
    /// Fábrica de aplicação destino, ver mais em <see cref="WebApplicationFactory{Program}"/>
    /// </summary>
    public TFactory Api { get; } = new();

    /// <summary>
    /// Libera recursos de teste.
    /// </summary>
    public void Dispose() => Api.Dispose();
}