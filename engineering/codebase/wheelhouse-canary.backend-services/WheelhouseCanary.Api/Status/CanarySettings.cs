namespace WheelhouseCanary.Api.Status;

/// <summary>Holds the canary's per-environment settings bound from <c>Canary</c>, filled by the environment's settings file.</summary>
public sealed class CanarySettings
{
    /// <summary>Holds the configuration section the settings bind from.</summary>
    public const string SectionName = "Canary";

    /// <summary>Gets the label this environment reports, such as <c>dev</c>, <c>test</c> or <c>prod</c>; it proves the environment's settings file reached the app. Required: <c>config check</c> refuses a missing or blank one with code <c>required</c>.</summary>
    public string Label { get; init; } = "";
}
