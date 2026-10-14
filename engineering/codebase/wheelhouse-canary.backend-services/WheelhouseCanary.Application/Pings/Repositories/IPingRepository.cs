using WheelhouseCanary.Domain.Pings.Entities;

namespace WheelhouseCanary.Application.Pings.Repositories;

/// <summary>Defines the contract for persisting and reading ping rows.</summary>
public interface IPingRepository
{
    /// <summary>Persists <paramref name="ping"/>; its creation timestamp is set on return.</summary>
    Task AddAsync(PingEntity ping, CancellationToken ct);

    /// <summary>Gets the newest <paramref name="count"/> pings, newest first.</summary>
    Task<IReadOnlyList<PingEntity>> ListLatestAsync(int count, CancellationToken ct);
}
