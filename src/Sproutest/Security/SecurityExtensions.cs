//using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Sproutest.Security;

public static class SecurityExtensions
{
    /// <summary>
    /// Mock jwt bearer authentication token.
    /// </summary>
    /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
    /// <param name="user">User to create claims</param>
    /// <param name="claimsChanger">Method to change claims</param>
    /// <returns>Instance of <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection UseMockedJwtBearerAuhentication(
      this IServiceCollection services,
      UserClaims user,
      Action<List<Claim>>? claimsChanger = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        // TODO: entender de qual pacote percente a classe JwtBearerOptions.
        /*
        //Microsoft.AspNet.Authentication.JwtBearer
        services.PostConfigureAll<JwtBearerOptions>(config =>
        {
            var baseOnMessageReceived = config.Events?.OnMessageReceived;

            if (config.Events is null)
            {
                config.Events = new JwtBearerEvents();
            }

            config.Events.OnMessageReceived = context =>
            {
                baseOnMessageReceived?.Invoke(context);

                var claims = new List<Claim>
                {
                    new("jti", "token"),
                    new("iss", AuthenticationSchemas.Bearer),
                    new(ClaimTypes.Email, user.Email ?? string.Empty),
                    new(ClaimTypes.Name, user.Username ?? string.Empty),
                };

                claimsChanger?.Invoke(claims);

                context.Principal = new(new ClaimsIdentity(claims, "Mocked"));
                context.Success();

                return Task.CompletedTask;
            };
        });
        */

        return services;
    }
}
