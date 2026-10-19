using WoW.Two.Sdk.Beta.Be.Data.Abstractions;

namespace WheelhouseCanary.Domain.Pings.Entities;

/// <summary>Represents one ping — a note and the instant it was recorded, the row a deployment proves it can write and read back.</summary>
public sealed class PingEntity : IKeyedEntity<Guid>, IHasTableName, ICreationAuditable
{
    /// <summary>Holds the table this entity maps to.</summary>
    public static string TableName => "pings";

    /// <inheritdoc />
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the note recorded with the ping.</summary>
    public required string Note { get; set; }

    /// <summary>Gets or sets when the ping was recorded (UTC); the SDK audit interceptor stamps it on insert.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
