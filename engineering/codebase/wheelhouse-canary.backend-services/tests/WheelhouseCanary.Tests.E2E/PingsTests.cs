using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;

namespace WheelhouseCanary.Tests.E2E;

[Collection(CanaryApiCollection.Name)]
public sealed class PingsTests(CanaryApiFixture fixture) : IAsyncLifetime
{
    /// <summary>Holds the instant the test host's fake clock reads when a test starts; the SDK audit interceptor stamps pings from that clock.</summary>
    private DateTimeOffset start;

    public async Task InitializeAsync()
    {
        await fixture.ResetAsync();
        start = fixture.Host.Clock.GetUtcNow();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_ShouldReturnAnEmptyList_WhenNoPingWasRecorded()
    {
        using var client = fixture.Host.CreateClient();

        var pings = await ListAsync(client);

        pings.Should().BeEmpty();
    }

    [Fact]
    public async Task Add_ShouldReturn201_AndTheListShouldHoldThePing_WhenANoteIsGiven()
    {
        using var client = fixture.Host.CreateClient();

        using var created = await client.PostAsJsonAsync("/api/pings", new { note = "deploy check" });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        using var body = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var ping = body.RootElement.GetProperty("data");
        ping.GetProperty("note").GetString().Should().Be("deploy check");
        ping.GetProperty("createdAt").GetDateTimeOffset().Should().Be(start);

        var pings = await ListAsync(client);
        pings.Should().ContainSingle().Which.GetProperty("note").GetString().Should().Be("deploy check");
    }

    [Fact]
    public async Task Add_ShouldRecordTheDefaultNote_WhenTheBodyIsEmpty()
    {
        using var client = fixture.Host.CreateClient();

        using var created = await client.PostAsJsonAsync("/api/pings", new { });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var pings = await ListAsync(client);
        pings.Should().ContainSingle().Which.GetProperty("note").GetString().Should().Be("ping");
    }

    [Fact]
    public async Task Add_ShouldReturn400_WhenTheNoteIsTooLong()
    {
        using var client = fixture.Host.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/pings", new { note = new string('a', 201) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ListAsync(client)).Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ShouldReturnTheLatestTwentyNewestFirst_WhenMoreWereRecorded()
    {
        using var client = fixture.Host.CreateClient();
        for (var number = 1; number <= 25; number++)
        {
            using var created = await client.PostAsJsonAsync("/api/pings", new { note = $"ping {number}" });
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            fixture.Host.Clock.Advance(TimeSpan.FromSeconds(1));
        }

        var pings = await ListAsync(client);

        pings.Should().HaveCount(20);
        pings.Select(ping => ping.GetProperty("note").GetString())
            .Should().Equal(Enumerable.Range(6, 20).Reverse().Select(number => $"ping {number}"));
    }

    private static async Task<List<JsonElement>> ListAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/pings");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return [.. body.RootElement.GetProperty("data").EnumerateArray().Select(element => element.Clone())];
    }
}
