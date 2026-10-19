using Microsoft.EntityFrameworkCore;
using WheelhouseCanary.Domain.Pings.Entities;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore;

namespace WheelhouseCanary.Persistence.DataContexts;

/// <summary>The WheelhouseCanary database context — a pure mapper over the SQL-owned schema, on snake_case columns.</summary>
/// <remarks>Schema is owned by <c>Migrations/NNN-name/Apply.sql</c>; EF never creates or alters it.</remarks>
/// <param name="options">The context options, configured by the SDK Postgres bundle (Npgsql data source and snake_case naming).</param>
public sealed class WheelhouseCanaryDbContext(DbContextOptions<WheelhouseCanaryDbContext> options) : AppDbContextBase(options)
{
    /// <summary>Gets the recorded pings.</summary>
    public DbSet<PingEntity> Pings => Set<PingEntity>();
}
