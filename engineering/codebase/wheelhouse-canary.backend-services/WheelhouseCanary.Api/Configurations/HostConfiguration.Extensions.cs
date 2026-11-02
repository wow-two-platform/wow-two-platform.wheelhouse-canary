using WheelhouseCanary.Api.Status;
using WheelhouseCanary.Application.Pings.Repositories;
using WheelhouseCanary.Infrastructure;
using WheelhouseCanary.Persistence.DataContexts;
using WheelhouseCanary.Persistence.Repositories;
using WoW.Two.Sdk.Beta.Be.Data;
using WoW.Two.Sdk.Beta.Be.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Beta.Be.Foundation.Configuration.ConfigCheck;
using WoW.Two.Sdk.Beta.Be.Foundation.Options;
using WoW.Two.Sdk.Beta.Be.Mediator;
using WoW.Two.Sdk.Beta.Be.Mediator.ExceptionHandling;
using WoW.Two.Sdk.Beta.Be.Mediator.Logging;
using WoW.Two.Sdk.Beta.Be.Mediator.Validation;
using WoW.Two.Sdk.Beta.Be.Observability.HealthChecks;
using WoW.Two.Sdk.Beta.Be.Web.Json;

namespace WheelhouseCanary.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Registers the Postgres host floor for <see cref="WheelhouseCanaryDbContext"/>, its readiness probe and the migration version stamp.</summary>
    private static WebApplicationBuilder AddPostgresDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddPostgresPersistence<WheelhouseCanaryDbContext>(builder.Configuration, options =>
            options.ConfigureMigrations = migrations =>
                migrations.Version = MigrationVersionMapper.ToVersion(typeof(HostConfiguration).Assembly));
        builder.Services.AddHealthChecks().AddDatabaseReadinessCheck<WheelhouseCanaryDbContext>();
        return builder;
    }

    /// <summary>Registers the mediator over the handler-bearing assembly and its pipeline interceptors.</summary>
    private static WebApplicationBuilder AddMediator(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediator(typeof(InfrastructureAssembly).Assembly);

        // Order matters: exception-to-result, then logging, then validation before the handler.
        builder.Services.AddMediatorExceptionMappingInterceptor();
        builder.Services.AddMediatorLoggingInterceptor();
        builder.Services.AddMediatorValidatingInterceptor();
        return builder;
    }

    /// <summary>Registers the pings domain — its repository.</summary>
    private static WebApplicationBuilder AddPings(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IPingRepository, PingRepository>();
        return builder;
    }

    /// <summary>Binds the canary's own per-environment settings and declares <c>Canary</c> its root, so <c>config check</c> and a deployed boot refuse a missing label or an unknown key.</summary>
    private static WebApplicationBuilder AddCanarySettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedSettings<CanarySettings>(builder.Configuration, CanarySettings.SectionName, rules => rules
            .ValidateSetting(nameof(CanarySettings.Label), "required",
                settings => !string.IsNullOrWhiteSpace(settings.Label),
                $"{CanarySettings.SectionName}:{nameof(CanarySettings.Label)} is required: the label this environment reports, such as dev or prod."));
        builder.Services.AddSettingsRoot(CanarySettings.SectionName);
        return builder;
    }

    /// <summary>Registers controllers on the SDK wire contract.</summary>
    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithSdkJson();
        return builder;
    }
}
