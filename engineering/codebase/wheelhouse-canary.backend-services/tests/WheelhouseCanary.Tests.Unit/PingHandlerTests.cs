using AwesomeAssertions;
using WheelhouseCanary.Application.Pings.Commands;
using WheelhouseCanary.Application.Pings.Queries;
using WheelhouseCanary.Application.Pings.Repositories;
using WheelhouseCanary.Domain.Pings.Entities;
using WheelhouseCanary.Infrastructure.Pings.CommandHandlers;
using WheelhouseCanary.Infrastructure.Pings.QueryHandlers;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace WheelhouseCanary.Tests.Unit;

public sealed class PingHandlerTests
{
    private readonly InMemoryPingRepository repository = new();

    [Fact]
    public async Task Add_ShouldRecordTheTrimmedNote_WhenANoteIsGiven()
    {
        var handler = new PingAddCommandHandler(repository);

        var result = await handler.HandleAsync(new PingAddCommand { Note = "  hello  " }, CancellationToken.None);

        var ping = repository.Pings.Should().ContainSingle().Subject;
        ping.Note.Should().Be("hello");
        result.Should().BeOfType<AppResult<Application.Pings.Models.PingDto>.Success>()
            .Which.Data.Id.Should().Be(ping.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task Add_ShouldRecordTheDefaultNote_WhenTheNoteIsBlank(string? note)
    {
        var handler = new PingAddCommandHandler(repository);

        await handler.HandleAsync(new PingAddCommand { Note = note }, CancellationToken.None);

        repository.Pings.Should().ContainSingle().Which.Note.Should().Be(PingAddCommand.DefaultNote);
    }

    [Fact]
    public async Task List_ShouldAskForTheLatestTwenty()
    {
        var handler = new PingListQueryHandler(repository);

        await handler.HandleAsync(new PingListQuery(), CancellationToken.None);

        repository.RequestedCount.Should().Be(20);
    }

    private sealed class InMemoryPingRepository : IPingRepository
    {
        public List<PingEntity> Pings { get; } = [];

        public int RequestedCount { get; private set; }

        public Task AddAsync(PingEntity ping, CancellationToken ct)
        {
            ping.CreatedAt = DateTimeOffset.UnixEpoch;
            Pings.Add(ping);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PingEntity>> ListLatestAsync(int count, CancellationToken ct)
        {
            RequestedCount = count;
            return Task.FromResult<IReadOnlyList<PingEntity>>([.. Pings.Take(count)]);
        }
    }
}
