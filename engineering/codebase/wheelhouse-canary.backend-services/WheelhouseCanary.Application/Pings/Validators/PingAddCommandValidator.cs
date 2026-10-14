using FluentValidation;
using WheelhouseCanary.Application.Pings.Commands;

namespace WheelhouseCanary.Application.Pings.Validators;

/// <summary>Validates <see cref="PingAddCommand"/>.</summary>
public sealed class PingAddCommandValidator : AbstractValidator<PingAddCommand>
{
    /// <summary>Configures the add-ping field rules.</summary>
    public PingAddCommandValidator()
    {
        RuleFor(command => command.Note)
            .MaximumLength(PingAddCommand.MaxNoteLength);
    }
}
