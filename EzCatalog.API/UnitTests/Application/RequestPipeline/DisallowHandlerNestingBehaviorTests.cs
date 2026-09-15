using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.RequestPipeline;
using FluentAssertions;
using MediatR;
using Xunit;

namespace EzCatalog.UnitTests.Application.RequestPipeline;

public sealed class DisallowHandlerNestingBehaviorTests
{
    private readonly DisallowHandlerNestingBehavior<IRequest<string>, string> behavior =
        new DisallowHandlerNestingBehavior<IRequest<string>, string>();

    [Fact]
    public async Task Handle_WithoutNesting_ReturnsHandlerResult()
    {
        // Act
        var result = await HandleAsync(() => Task.FromResult("handled"));

        // Assert
        result.Should().Be("handled");
    }

    [Fact]
    public async Task Handle_WhenNested_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => HandleAsync(() => HandleAsync(() => Task.FromResult("nested")));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Nesting handlers is not allowed.");
    }

    [Fact]
    public async Task Handle_AfterAPreviousRequestCompleted_IsAllowedAgain()
    {
        // Arrange
        await HandleAsync(() => Task.FromResult("first"));

        // Act
        var result = await HandleAsync(() => Task.FromResult("second"));

        // Assert
        result.Should().Be("second");
    }

    [Fact]
    public async Task Handle_AfterAPreviousRequestFailed_IsAllowedAgain()
    {
        // Arrange
        var failing = () => HandleAsync(() => Task.FromException<string>(new InvalidOperationException("handler failed")));
        await failing.Should().ThrowAsync<InvalidOperationException>().WithMessage("handler failed");

        // Act
        var result = await HandleAsync(() => Task.FromResult("after failure"));

        // Assert
        result.Should().Be("after failure");
    }

    private Task<string> HandleAsync(Func<Task<string>> next)
    {
        return behavior.Handle(
            new FakeRequest(),
            _ => next(),
            TestContext.Current.CancellationToken);
    }

    private sealed class FakeRequest : IRequest<string>
    {
    }
}
