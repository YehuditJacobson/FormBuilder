using FluentValidation;
using FormBuilder.Application.Common.Behaviors;
using MediatR;

namespace FormBuilder.Application.Tests.Common;

public class ValidationBehaviorTests
{
    private sealed record Ping(string Name) : IRequest<string>;

    private sealed class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator() => RuleFor(ping => ping.Name).NotEmpty();
    }

    private static RequestHandlerDelegate<string> Next(string value = "pong")
        => () => Task.FromResult(value);

    [Fact]
    public async Task Passes_through_when_no_validators_are_registered()
    {
        var behavior = new ValidationBehavior<Ping, string>([]);

        var response = await behavior.Handle(new Ping(""), Next(), CancellationToken.None);

        response.Should().Be("pong");
    }

    [Fact]
    public async Task Passes_through_when_validation_succeeds()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator()]);

        var response = await behavior.Handle(new Ping("ok"), Next(), CancellationToken.None);

        response.Should().Be("pong");
    }

    [Fact]
    public async Task Throws_a_validation_exception_when_validation_fails()
    {
        var behavior = new ValidationBehavior<Ping, string>([new PingValidator()]);

        var act = () => behavior.Handle(new Ping(""), Next(), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
