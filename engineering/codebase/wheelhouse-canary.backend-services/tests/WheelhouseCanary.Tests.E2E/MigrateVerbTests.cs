using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Npgsql;
using WheelhouseCanary.Api.Configurations;
using WoW.Two.Sdk.Beta.Be.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Beta.Be.Testing.Data.Migrations.Verbs;

namespace WheelhouseCanary.Tests.E2E;

/// <summary>
/// The image's <c>migrate</c> verbs as a deploy runs them — <c>dotnet WheelhouseCanary.Api.dll migrate</c> with
/// <c>Migrations__ConnectionString</c> and <c>DEPLOY_ENVIRONMENT</c> set — over the services <c>Program</c> builds, and the
/// deployed boot that only verifies. Each test owns a database on the fixture's server; the host never starts.
/// </summary>
[Collection(CanaryApiCollection.Name)]
public sealed class MigrateVerbTests(CanaryApiFixture fixture) : IAsyncLifetime
{
    /// <summary>Every migration the default set carries, in apply order.</summary>
    private static readonly string[] Migrations = ["001-baseline", "002-pings-created-at-index"];

    private readonly string database = $"canary_migrate_{Guid.NewGuid():N}";
    private readonly List<WebApplication> apps = [];

    /// <summary>The test's own database, unpooled so dropping it leaves no idle connection behind.</summary>
    private string ConnectionString =>
        new NpgsqlConnectionStringBuilder(fixture.ConnectionString) { Database = database, Pooling = false }.ConnectionString;

    [Fact]
    public async Task Migrate_ShouldCreateAndMigrateAMissingDatabase_ThenRerunWithoutAChange()
    {
        var verbs = Verbs(Image(MigrateKey()));

        var migrate = await verbs.AssertExitAsync(0);
        Default(migrate.Result).Applied.Select(applied => applied.Label).Should().Equal(Migrations);

        await verbs.AssertRerunChangesNothingAsync();

        var check = await verbs.AssertCheckWritesNothingAsync("--json");
        check.Exit.Should().Be(0, check.ToString());
        check.Output.Split('\n', StringSplitOptions.RemoveEmptyEntries)[^1].Should().StartWith(MigrateRunResult.ResultLinePrefix);
        Default(check.Result).State.Should().Be(MigrationLevelState.Current);
    }

    [Fact]
    public async Task Check_ShouldReadBehindAndWriteNothing_WhenTheDatabaseIsEmpty()
    {
        await CreateDatabaseAsync();

        var check = await Verbs(Image(MigrateKey())).AssertCheckWritesNothingAsync();

        check.Exit.Should().Be(1, check.ToString());
        Default(check.Result).State.Should().Be(MigrationLevelState.Behind);
    }

    [Fact]
    public async Task DeployedBoot_ShouldRefuseAndApplyNothing_UntilTheMigrateStepRan()
    {
        await CreateDatabaseAsync();
        var behind = Image(TargetSettings());

        await FluentActions.Awaiting(() => behind.ConfigureAsync()).Should().ThrowAsync<InvalidOperationException>();
        (await HasMigrationHistoryAsync()).Should().BeFalse("a deployed boot verifies levels and writes nothing");

        await Verbs(Image(TargetSettings())).AssertExitAsync(0);

        var current = Image(TargetSettings());
        await FluentActions.Awaiting(() => current.ConfigureAsync()).Should().NotThrowAsync();
    }

    /// <inheritdoc />
    public Task InitializeAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        foreach (var app in apps)
            await app.DisposeAsync();
        await ExecuteOnServerAsync($"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
    }

    /// <summary>The settings a deploy's migrate container carries: the migrate key alone, no settings file.</summary>
    private Dictionary<string, string?> MigrateKey() => new()
    {
        ["Migrations:ConnectionString"] = ConnectionString,
        ["DEPLOY_ENVIRONMENT"] = "prod",
        ["Logging:LogLevel:Default"] = "Warning",
    };

    /// <summary>The settings a deployed container reads from its target's settings file.</summary>
    private Dictionary<string, string?> TargetSettings() => new()
    {
        ["DatabaseSettings:ConnectionString"] = ConnectionString,
        ["Canary:Label"] = "prod",
        ["DEPLOY_ENVIRONMENT"] = "prod",
        ["Logging:LogLevel:Default"] = "Warning",
    };

    /// <summary>Builds the app as the image's <c>Program</c> does, in <c>Production</c>, without starting it.</summary>
    private WebApplication Image(Dictionary<string, string?> settings)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(Program).Assembly.GetName().Name,
            EnvironmentName = Environments.Production,
        });
        builder.Configuration.AddInMemoryCollection(settings);
        builder.Configure();
        var app = builder.Build();
        apps.Add(app);
        return app;
    }

    /// <summary>The verb harness over an app's services, snapshotting the test's database.</summary>
    private MigrateVerbHarness Verbs(WebApplication app) => new(app.Services, async ct =>
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(ct);
        return connection;
    });

    /// <summary>The default set's entry on a result line.</summary>
    private static MigrateSetResult Default(MigrateRunResult result) =>
        result.Sets.Should().ContainSingle(set => set.Name == MigrationConstants.DefaultSetName).Subject;

    private Task CreateDatabaseAsync() => ExecuteOnServerAsync($"CREATE DATABASE \"{database}\"");

    private async Task<bool> HasMigrationHistoryAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT to_regclass('public.migration_history') IS NOT NULL";
        return (bool)(await command.ExecuteScalarAsync())!;
    }

    /// <summary>Runs one statement on the fixture's own database, which stays as the server's maintenance connection.</summary>
    private async Task ExecuteOnServerAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }
}
