using WheelhouseCanary.Application.Pings.Models;
using WheelhouseCanary.Application.Pings.Queries;
using WheelhouseCanary.Application.Pings.Repositories;
using WheelhouseCanary.Infrastructure.Pings.Extensions;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace WheelhouseCanary.Infrastructure.Pings.QueryHandlers;

/// <summary>Handles <see cref="PingListQuery"/>.</summary>
public sealed class PingListQueryHandler(IPingRepository repository)
    : IQueryHandler<PingListQuery, AppResult<IReadOnlyList<PingDto>>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<IReadOnlyList<PingDto>>> HandleAsync(PingListQuery request, CancellationToken ct)
    {
        var pings = await repository.ListLatestAsync(PingListQuery.Count, ct);

        return AppResult<IReadOnlyList<PingDto>>.Ok([.. pings.Select(ping => ping.ToDto())]);
    }
}
