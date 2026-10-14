namespace WheelhouseCanary.Application.Pings.Models;

/// <summary>Represents one recorded ping.</summary>
/// <param name="Id">The ping's identifier.</param>
/// <param name="Note">The note recorded with the ping.</param>
/// <param name="CreatedAt">When the ping was recorded (UTC).</param>
public sealed record PingDto(Guid Id, string Note, DateTimeOffset CreatedAt);
