using AwesomeAssertions;
using WheelhouseCanary.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace WheelhouseCanary.Tests.Migrations;

/// <summary>The real embedded migrations over a fresh PostgreSQL: apply, idempotency, and the Apply → Rollback → Apply round trip.</summary>
[Collection(MigratorCollection.Name)]
public sealed class MigrationTests(MigratorPostgresFixture fixture)
{
    private static readonly string[] Names = ["baseline", "pings-created-at-index"];

    private MigratorHarness CreateMigrator(Action<MigrationOptions>? configure = null) =>
        MigratorHarness.CreatePostgres(fixture.ConnectionString, typeof(WheelhouseCanaryDbContext).Assembly, configure);

    [Fact]
    public async Task ApplyPending_ShouldCreateThePingsSchemaAndBeIdempotent_WhenTheDatabaseIsFresh()
    {
        await fixture.ResetAsync();
        await using var migrator = CreateMigrator();

        var applied = Unwrap(await migrator.Runner.ApplyPendingAsync("startup", CancellationToken.None));

        applied.Should().Equal("001-baseline", "002-pings-created-at-index");
        (await migrator.HasTableAsync("pings")).Should().BeTrue();
        (await migrator.HasIndexAsync("ix_pings_created_at")).Should().BeTrue();
        var history = await migrator.ReadHistoryAsync();
        history.Select(row => row.Ordinal).Should().Equal(1, 2);
        history.Select(row => row.Name).Should().Equal(Names);
        history.Should().OnlyContain(row => row.AppliedBy == "startup" && row.Checksum.Length == 64);

        Unwrap(await migrator.Runner.ApplyPendingAsync("startup", CancellationToken.None)).Should().BeEmpty();
        (await migrator.ReadHistoryAsync()).Should().HaveCount(2);
    }

    [Fact]
    public async Task Rollback_ShouldInvertEachMigrationInTurn_WhenRolledBackOneByOne()
    {
        await fixture.ResetAsync();
        await using var migrator = CreateMigrator(options => options.AllowRollback = true);
        Unwrap(await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None));
        await InsertPingAsync(migrator);

        // The latest migration (the index) goes first: the index is gone, the table and its row stay.
        Unwrap(await migrator.Runner.RollbackAsync(targetOrdinal: null, CancellationToken.None))
            .Should().Equal("002-pings-created-at-index");
        (await migrator.HasIndexAsync("ix_pings_created_at")).Should().BeFalse();
        (await migrator.HasTableAsync("pings")).Should().BeTrue();
        (await CountPingsAsync(migrator)).Should().Be(1);

        // The baseline goes next: the table is gone and the history is empty.
        Unwrap(await migrator.Runner.RollbackAsync(targetOrdinal: null, CancellationToken.None))
            .Should().Equal("001-baseline");
        (await migrator.HasTableAsync("pings")).Should().BeFalse();
        (await migrator.ReadHistoryAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task ApplyPending_ShouldRestoreTheSchema_WhenEveryMigrationWasRolledBack()
    {
        await fixture.ResetAsync();
        await using var migrator = CreateMigrator(options => options.AllowRollback = true);
        Unwrap(await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None));
        Unwrap(await migrator.Runner.RollbackAsync(targetOrdinal: 0, CancellationToken.None))
            .Should().Equal("002-pings-created-at-index", "001-baseline");

        var reapplied = Unwrap(await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None));

        reapplied.Should().Equal("001-baseline", "002-pings-created-at-index");
        (await migrator.HasTableAsync("pings")).Should().BeTrue();
        (await migrator.HasIndexAsync("ix_pings_created_at")).Should().BeTrue();
        var status = Unwrap(await migrator.Runner.GetStatusAsync(CancellationToken.None));
        status.Applied.Select(applied => applied.Ordinal).Should().Equal(1, 2);
        status.Pending.Should().BeEmpty();
        status.Drifted.Should().BeEmpty();
    }

    private static async Task InsertPingAsync(MigratorHarness migrator)
    {
        await using var connection = await migrator.OpenConnectionAsync();
        await using var insert = connection.CreateCommand();
        insert.CommandText = "INSERT INTO pings (id, note, created_at) VALUES (gen_random_uuid(), 'kept', now())";
        await insert.ExecuteNonQueryAsync();
    }

    private static async Task<long> CountPingsAsync(MigratorHarness migrator)
    {
        await using var connection = await migrator.OpenConnectionAsync();
        await using var count = connection.CreateCommand();
        count.CommandText = "SELECT count(*) FROM pings";
        return (long)(await count.ExecuteScalarAsync())!;
    }

    /// <summary>Returns a migrator result's value, failing the test with the migrator's own message otherwise.</summary>
    private static T Unwrap<T>(Result<T> result) where T : notnull =>
        result is Result<T>.Success success
            ? success.Value
            : throw new Xunit.Sdk.XunitException(((Result<T>.Failure)result).Error.Message);
}
