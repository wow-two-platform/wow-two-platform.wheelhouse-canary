using WheelhouseCanary.Application;
using WoW.Two.Sdk.Beta.Be.Data;
using WoW.Two.Sdk.Beta.Be.Meta;

namespace WheelhouseCanary.Api.Configurations;

/// <summary>Extends the host builder and the web application for startup wiring.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    /// <param name="builder">The web application builder to configure.</param>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        // Lay the SDK boot floor before any product seam; the Vue app is served from the same host. It reads the settings
        // file a target mounts (appsettings.Target.json) and, on a local run, this machine's (appsettings.Local.json)
        // before the options below; neither sits in the image or reaches tests.
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

    /// <summary>Runs startup tasks, then configures middleware and endpoints.</summary>
    /// <param name="app">The built web application to configure.</param>
    public static async Task<WebApplication> ConfigureAsync(this WebApplication app)
    {
        // A local run (Development, no DEPLOY_ENVIRONMENT) creates the database and applies pending SQL migrations; a
        // deployed boot only verifies levels read-only and refuses to start behind, since the deploy's migrate step applies.
        await app.Services.MigrateBespokeOnStartupAsync(app.Lifetime.ApplicationStopping);

        // SDK pipeline: secure headers, compression, the SPA bundle, /health (database readiness) and the SPA fallback.
        app.UseApiDefaults();
        app.MapControllers();

        return app;
    }
}
