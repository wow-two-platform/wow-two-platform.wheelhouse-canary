using Microsoft.EntityFrameworkCore;
using WheelhouseCanary.Application.Pings.Repositories;
using WheelhouseCanary.Domain.Pings.Entities;
using WheelhouseCanary.Persistence.DataContexts;

namespace WheelhouseCanary.Persistence.Repositories;

/// <summary>Persists and fetches ping rows via EF Core.</summary>
/// <param name="db">The application context, mapping over the SQL-owned schema.</param>
public sealed class PingRepository(WheelhouseCanaryDbContext db) : IPingRepository
{
    /// <inheritdoc />
    public async Task AddAsync(PingEntity ping, CancellationToken ct)
    {
        db.Pings.Add(ping);
        await db.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PingEntity>> ListLatestAsync(int count, CancellationToken ct) =>
        await db.Pings.AsNoTracking()
            .OrderByDescending(ping => ping.CreatedAt)
            .ThenByDescending(ping => ping.Id)
            .Take(count)
            .ToListAsync(ct);
}
