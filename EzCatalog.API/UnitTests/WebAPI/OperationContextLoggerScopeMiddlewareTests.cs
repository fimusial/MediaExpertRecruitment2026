using System;
using System.Threading.Tasks;
using EzCatalog.Application.Contexts;
using EzCatalog.Application.Ports;
using EzCatalog.Infrastructure.Adapters;
using EzCatalog.WebAPI;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EzCatalog.UnitTests.WebAPI;

public sealed class OperationContextLoggerScopeMiddlewareTests : IDisposable
{
    private readonly ServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton<IGuidProvider, GuidProvider>()
        .AddSingleton<IDateTimeProvider, DateTimeProvider>()
        .AddScoped<IOperationContext, OperationContext>()
        .BuildServiceProvider();

    private readonly IServiceScope requestScope;

    public OperationContextLoggerScopeMiddlewareTests()
    {
        requestScope = serviceProvider.CreateScope();
    }

    [Fact]
    public async Task Invoke_WithCorrelationIdHeader_ContinuesWithTheCallersCorrelationId()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var context = CreateHttpContext();
        context.Request.Headers[OperationContextLoggerScopeMiddleware.CorrelationIdHeaderName] = correlationId.ToString();

        // Act
        await InvokeAsync(context);

        // Assert
        OperationContextOf(context).CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task Invoke_WithoutCorrelationIdHeader_LeavesCorrelationIdUnset()
    {
        // Arrange
        var context = CreateHttpContext();

        // Act
        await InvokeAsync(context);

        // Assert
        OperationContextOf(context).CorrelationId.Should().BeNull();
    }

    [Fact]
    public async Task Invoke_WithMalformedCorrelationIdHeader_IgnoresItAndCallsTheNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        context.Request.Headers[OperationContextLoggerScopeMiddleware.CorrelationIdHeaderName] = "not-a-guid";
        var nextCalled = false;

        // Act
        var act = () => InvokeAsync(context, () => nextCalled = true);

        // Assert
        await act.Should().NotThrowAsync();
        nextCalled.Should().BeTrue();
        OperationContextOf(context).CorrelationId.Should().BeNull();
    }

    public void Dispose()
    {
        requestScope.Dispose();
        serviceProvider.Dispose();
    }

    private static IOperationContext OperationContextOf(HttpContext context) =>
        context.RequestServices.GetRequiredService<IOperationContext>();

    private static Task InvokeAsync(HttpContext context, Action? onNext = null)
    {
        var middleware = new OperationContextLoggerScopeMiddleware(_ =>
        {
            onNext?.Invoke();
            return Task.CompletedTask;
        });

        return middleware.Invoke(context, context.RequestServices);
    }

    private DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            RequestServices = requestScope.ServiceProvider,
        };
    }
}
