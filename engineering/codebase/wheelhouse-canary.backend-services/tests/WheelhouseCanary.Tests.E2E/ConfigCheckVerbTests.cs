using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WheelhouseCanary.Api.Configurations;
using WoW.Two.Sdk.Beta.Be.Foundation.Configuration.ConfigCheck;

namespace WheelhouseCanary.Tests.E2E;

/// <summary>
/// The image's <c>config check</c> as a deploy runs it — <c>dotnet WheelhouseCanary.Api.dll config check</c> with the
/// target's settings — over the services <c>Program</c> builds. The check reads no database, so the host never starts.
/// </summary>
public sealed class ConfigCheckVerbTests : IAsyncLifetime
{
    private readonly List<WebApplication> apps = [];

    [Fact]
    public async Task Check_ShouldExit0_WhenTheTargetSetsTheLabel()
    {
        var (exit, result) = await CheckAsync(TargetSettings(label: "dev"));

        exit.Should().Be(0);
        result.Invalid.Should().BeEmpty();
        result.Unknown.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public async Task Check_ShouldRefuseWithRequired_WhenTheLabelIsMissing(string? label)
    {
        var (exit, result) = await CheckAsync(TargetSettings(label));

        exit.Should().Be(1);
        result.Invalid.Should().ContainSingle().Which.Should().Match<ConfigCheckInvalid>(invalid =>
            invalid.Code == "required" && invalid.Setting == "Canary:Label");
    }

    [Fact]
    public async Task Check_ShouldRefuse_WhenAKeyUnderCanaryBindsToNoSetting()
    {
        var settings = TargetSettings(label: "dev");
        settings["Canary:Lable"] = "dev";

        var (exit, result) = await CheckAsync(settings);

        exit.Should().Be(1);
        result.Unknown.Should().Equal("Canary:Lable");
    }

    /// <inheritdoc />
    public Task InitializeAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        foreach (var app in apps)
            await app.DisposeAsync();
    }

    /// <summary>The settings a target's settings file carries; a null label leaves the key out.</summary>
    /// <remarks>No <c>DEPLOY_ENVIRONMENT</c>: a deployed host without a target file reads a legacy <c>appsettings.Local.json</c>, and a developer's own copy in the test output would fill the label.</remarks>
    private static Dictionary<string, string?> TargetSettings(string? label)
    {
        var settings = new Dictionary<string, string?>
        {
            ["DatabaseSettings:ConnectionString"] = "Host=localhost;Database=wheelhouse_canary;Username=canary",
            ["Logging:LogLevel:Default"] = "Warning",
        };
        if (label is not null)
            settings["Canary:Label"] = label;
        return settings;
    }

    /// <summary>Runs <c>config check</c> over an app built as the image's <c>Program</c> builds it, in <c>Production</c>.</summary>
    private async Task<(int Exit, ConfigCheckResult Result)> CheckAsync(Dictionary<string, string?> settings)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(Program).Assembly.GetName().Name,
            EnvironmentName = Environments.Production,
        });
        builder.Configuration.AddInMemoryCollection(settings);
        builder.Configure();
        var app = builder.Build();
        apps.Add(app);

        using var output = new StringWriter();
        using var error = new StringWriter();
        var exit = await ConfigCheckCommand.RunAsync(app.Services, "wheelhouse-canary", ["config", "check"], output, error);

        var result = ConfigCheckResult.FromOutput(output.ToString());
        result.Should().NotBeNull(error.ToString());
        return (exit, result!);
    }
}
