using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace WheelhouseCanary.Tests.Migrations;

/// <summary>xUnit collection over the SDK <see cref="MigratorPostgresFixture"/>; its reset drops the schema, so every test applies from scratch.</summary>
[CollectionDefinition(Name)]
public sealed class MigratorCollection : ICollectionFixture<MigratorPostgresFixture>
{
    /// <summary>The collection name every migrator test class joins.</summary>
    public const string Name = "postgres-migrator";
}
