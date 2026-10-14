using WheelhouseCanary.Application.Pings.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace WheelhouseCanary.Application.Pings.Commands;

/// <summary>Represents a command to record a ping.</summary>
public sealed record PingAddCommand : ICommand<AppResult<PingDto>>
{
    /// <summary>Holds the note a ping carries when the caller sends none.</summary>
    public const string DefaultNote = "ping";

    /// <summary>Holds the longest note a ping may carry.</summary>
    public const int MaxNoteLength = 200;

    /// <summary>Gets the note to record, or <c>null</c> for <see cref="DefaultNote"/>.</summary>
    public string? Note { get; init; }
}
