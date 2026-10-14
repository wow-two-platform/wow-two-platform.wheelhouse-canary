using AwesomeAssertions;
using WheelhouseCanary.Application.Pings.Commands;
using WheelhouseCanary.Application.Pings.Validators;

namespace WheelhouseCanary.Tests.Unit;

public sealed class PingAddCommandValidatorTests
{
    private readonly PingAddCommandValidator validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("deploy check")]
    public void Validate_ShouldAccept_WhenNoteIsAbsentOrShort(string? note) =>
        validator.Validate(new PingAddCommand { Note = note }).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_ShouldAccept_WhenNoteIsAtTheLimit() =>
        validator.Validate(new PingAddCommand { Note = new string('a', PingAddCommand.MaxNoteLength) }).IsValid.Should().BeTrue();

    [Fact]
    public void Validate_ShouldReject_WhenNoteIsOverTheLimit() =>
        validator.Validate(new PingAddCommand { Note = new string('a', PingAddCommand.MaxNoteLength + 1) }).IsValid.Should().BeFalse();
}
