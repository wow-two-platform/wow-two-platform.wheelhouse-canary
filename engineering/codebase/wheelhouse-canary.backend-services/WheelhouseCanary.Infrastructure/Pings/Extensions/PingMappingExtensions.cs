using WheelhouseCanary.Application.Pings.Models;
using WheelhouseCanary.Domain.Pings.Entities;

namespace WheelhouseCanary.Infrastructure.Pings.Extensions;

/// <summary>Maps ping entities to their application models.</summary>
public static class PingMappingExtensions
{
    /// <summary>Maps <paramref name="ping"/> to its <see cref="PingDto"/>.</summary>
    public static PingDto ToDto(this PingEntity ping) => new(ping.Id, ping.Note, ping.CreatedAt);
}
