using System.Net;
using System.Text.Json;
using AwesomeAssertions;

namespace WheelhouseCanary.Tests.E2E;

[Collection(CanaryApiCollection.Name)]
public sealed class SystemStatusTests(CanaryApiFixture fixture)
{
    [Fact]
    public async Task Health_ShouldReturn200_WhenTheDatabaseAnswers()
    {
        using var client = fixture.Host.CreateClient();

        using var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Status_ShouldReturn200_WithServiceVersionAndLabel()
    {
        using var client = fixture.Host.CreateClient();

        using var response = await client.GetAsync("/api/system/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var status = body.RootElement.GetProperty("data");
        status.GetProperty("service").GetString().Should().Be("wheelhouse-canary");
        status.GetProperty("status").GetString().Should().Be("ok");
        status.GetProperty("version").GetString().Should().NotBeNullOrWhiteSpace().And.NotBe("unknown");
        status.GetProperty("label").GetString().Should().Be(CanaryApiFixture.Label);
    }
}
