using WheelhouseCanary.Application.Pings.Commands;

namespace WheelhouseCanary.Api.Pings.Requests;

/// <summary>Represents the add-ping request body.</summary>
public sealed record PingAddApiRequest
{
    /// <summary>Gets the note to record; omit it to record the default note.</summary>
    public string? Note { get; init; }
}

/// <summary>Extends <see cref="PingAddApiRequest"/> for wire mapping.</summary>
public static class PingAddApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="PingAddCommand"/>.</summary>
    public static PingAddCommand ToCommand(this PingAddApiRequest request) => new() { Note = request.Note };
}
