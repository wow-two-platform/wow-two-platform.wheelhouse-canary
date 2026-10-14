using WheelhouseCanary.Application.Pings.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace WheelhouseCanary.Application.Pings.Queries;

/// <summary>Represents a query for the latest pings, newest first.</summary>
public sealed record PingListQuery : IQuery<AppResult<IReadOnlyList<PingDto>>>
{
    /// <summary>Holds how many pings the query returns.</summary>
    public const int Count = 20;
}
