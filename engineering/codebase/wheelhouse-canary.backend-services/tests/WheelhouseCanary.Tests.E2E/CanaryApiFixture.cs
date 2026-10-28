using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using WoW.Two.Sdk.Beta.Be.Testing;
using WoW.Two.Sdk.Beta.Be.Testing.Containers.Postgres;
using WoW.Two.Sdk.Beta.Be.Testing.MultiHost;

namespace WheelhouseCanary.Tests.E2E;

/// <summary>Boots the real API and its SQL migrations over an isolated PostgreSQL container.</summary>
public sealed class CanaryApiFixture : MultiHostFixture, IAsyncLifetime
{
    /// <summary>Holds the environment label the test host reports.</summary>
    public const string Label = "e2e";

    /// <summary>Holds the public host the test target allows, beside <c>localhost</c>, as a deployed target's <c>AllowedHosts</c> does.</summary>
    public const string PublicHost = "canary.example.test";

    private readonly PostgresFixture postgres;

    public CanaryApiFixture()
    {
        postgres = AddSharedFixture(new PostgresFixture(new PostgreSqlBuilder().WithImage("postgres:18-alpine").Build()));
        Host = AddHost(new WebApiTestHost<Program>
        {
            ConfigureConfigurationHook = configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DatabaseSettings:ConnectionString"] = postgres.ConnectionString,
                ["DB_CONNECTION"] = postgres.ConnectionString,
                ["Canary:Label"] = Label,
                ["AllowedHosts"] = PublicHost + ";localhost",
                ["Logging:LogLevel:Default"] = "Warning",
            }),
        });
    }

    /// <summary>Gets the booted API host.</summary>
    public WebApiTestHost<Program> Host { get; }

    /// <summary>Gets the container's connection string; its server also holds the migrate tests' own databases.</summary>
    public string ConnectionString => postgres.ConnectionString;

    /// <inheritdoc />
    protected override ValueTask InitializeStateAsync(CancellationToken cancellationToken = default) =>
        postgres.InitializeRespawnerAsync(cancellationToken);

    Task IAsyncLifetime.InitializeAsync() => StartAsync().AsTask();

    Task IAsyncLifetime.DisposeAsync() => DisposeAsync().AsTask();
}

/// <summary>Shares one <see cref="CanaryApiFixture"/> across the whole E2E run.</summary>
[CollectionDefinition(Name)]
public sealed class CanaryApiCollection : ICollectionFixture<CanaryApiFixture>
{
    /// <summary>The collection name every E2E class joins.</summary>
    public const string Name = "canary-api";
}
