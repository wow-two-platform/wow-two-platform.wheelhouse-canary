using WheelhouseCanary.Application;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace WheelhouseCanary.Api.Configurations;

/// <summary>Extends the host builder and the web application for startup wiring.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    /// <param name="builder">The web application builder to configure.</param>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        // The per-environment settings file the runner mounts (deploy.yml settings.file); absent in dev and tests.
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // Lay the SDK boot floor before any product seam; the Vue app is served from the same host.
        builder.AddApiDefaults(options =>
        {
            options.ServiceName = "wheelhouse-canary";
            options.EnableOutputCache = false;
            options.EnableRateLimiting = false;
            options.EnableOtlpExporters = false;
            options.EnableHttpsRedirection = !builder.Environment.IsDevelopment();
            options.SpaHosting = _ => { };
            options.TrustProxiesFrom(builder.Configuration.GetSection("Deployment"));

            // Scan the Application assembly's FluentValidation validators so the SDK IValidator<T> adapter can resolve them.
            options.ValidatorAssemblies.Add(typeof(ApplicationAssembly).Assembly);
        });

        return builder
            .AddPostgresDatabase()
            .AddMediator()
            .AddPings()
            .AddCanarySettings()
            .AddControllers();
    }

    /// <summary>Applies pending migrations, then configures middleware and endpoints.</summary>
    /// <param name="app">The built web application to configure.</param>
    public static async Task<WebApplication> ConfigureAsync(this WebApplication app)
    {
        // Create the database if missing and apply pending SQL migrations before serving — advisory-locked.
        await app.Services.MigrateBespokeOnStartupAsync(app.Lifetime.ApplicationStopping);

        // SDK pipeline: secure headers, compression, the SPA bundle, /health (database readiness) and the SPA fallback.
        app.UseApiDefaults();
        app.MapControllers();

        return app;
    }
}
