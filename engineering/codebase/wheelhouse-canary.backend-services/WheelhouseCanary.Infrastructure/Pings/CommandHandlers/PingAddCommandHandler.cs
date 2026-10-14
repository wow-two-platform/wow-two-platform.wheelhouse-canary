using WheelhouseCanary.Application.Pings.Commands;
using WheelhouseCanary.Application.Pings.Models;
using WheelhouseCanary.Application.Pings.Repositories;
using WheelhouseCanary.Domain.Pings.Entities;
using WheelhouseCanary.Infrastructure.Pings.Extensions;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace WheelhouseCanary.Infrastructure.Pings.CommandHandlers;

/// <summary>Handles <see cref="PingAddCommand"/>.</summary>
public sealed class PingAddCommandHandler(IPingRepository repository)
    : ICommandHandler<PingAddCommand, AppResult<PingDto>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<PingDto>> HandleAsync(PingAddCommand request, CancellationToken ct)
    {
        // The field rules already ran in the pipeline; a blank note falls back to the default one.
        var ping = new PingEntity
        {
            Id = Guid.CreateVersion7(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? PingAddCommand.DefaultNote : request.Note.Trim(),
        };

        await repository.AddAsync(ping, ct);

        return AppResult<PingDto>.Ok(ping.ToDto());
    }
}
