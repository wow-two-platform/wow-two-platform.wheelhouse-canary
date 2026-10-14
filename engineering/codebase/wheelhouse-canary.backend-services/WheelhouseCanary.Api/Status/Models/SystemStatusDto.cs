namespace WheelhouseCanary.Api.Status.Models;

/// <summary>Represents the liveness and identity of the service.</summary>
public sealed record SystemStatusDto
{
    /// <summary>Gets the service name.</summary>
    public required string Service { get; init; }

    /// <summary>Gets the liveness state.</summary>
    public required string Status { get; init; }

    /// <summary>Gets the informational version of the running build.</summary>
    public required string Version { get; init; }

    /// <summary>Gets the label this environment's settings file gives the canary, empty when it sets none.</summary>
    public required string Label { get; init; }
}
