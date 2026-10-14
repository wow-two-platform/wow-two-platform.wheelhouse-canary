namespace WheelhouseCanary.Api.Status;

/// <summary>Holds the canary's per-environment settings, filled by the environment's settings file.</summary>
public sealed class CanarySettings
{
    /// <summary>Holds the configuration section the settings bind from.</summary>
    public const string SectionName = "Canary";

    /// <summary>Gets or sets the label this environment reports, such as <c>dev</c>, <c>test</c> or <c>prod</c>; it proves the environment's settings file reached the app.</summary>
    public string Label { get; set; } = "";
}
