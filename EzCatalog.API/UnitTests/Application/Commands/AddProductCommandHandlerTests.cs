using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Commands;
using EzCatalog.Application.Ports;
using EzCatalog.Application.RequestPipeline;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.Domain.Products.Exceptions;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.Infrastructure.Exceptions;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EzCatalog.UnitTests.Application.Commands;

public sealed class AddProductCommandHandlerTests : IAsyncDisposable
{
    private static readonly Guid GeneratedId = ProductTestData.DefaultId;

    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly List<INotification> publishedNotifications = new List<INotification>();
    private readonly AddProductCommandHandler handler;

    public AddProductCommandHandlerTests()
    {
        dbContext = database.CreateContext();

        mediator
            .When(substitute => substitute.Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>()))
            .Do(call => publishedNotifications.Add(call.Arg<INotification>()));

        var guidProvider = Substitute.For<IGuidProvider>();
        guidProvider.GetNewGuid().Returns(GeneratedId);

        handler = new AddProductCommandHandler(
            NullLogger<AddProductCommandHandler>.Instance,
            mediator,
            new ProductRepository(dbContext),
            guidProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsGeneratedId()
    {
        // Arrange
        var command = new AddProductCommand("LAP-DEL-000001", "Dell Laptop", 4999.99m, "PLN");

        // Act
        var id = await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        id.Should().Be(GeneratedId);
    }

    [Fact]
    public async Task Handle_WithValidCommand_AddsProductWithNormalizedValues()
    {
        // Arrange
        var command = new AddProductCommand(" lap-del-000001 ", "  Dell Laptop  ", 4999.99m, "EUR");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        var stored = await database.FindProductAsync(GeneratedId);
        stored.Should().BeEquivalentTo(new
        {
            Id = GeneratedId,
            Sku = "LAP-DEL-000001",
            Name = "Dell Laptop",
            PriceAmount = 4999.99m,
            PriceCurrency = "EUR",
        });
    }

    [Fact]
    public async Task Handle_WithValidCommand_PublishesProductCreatedNotification()
    {
        // Arrange
        var command = new AddProductCommand("LAP-DEL-000001", "Dell Laptop", 4999.99m, "PLN");
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        publishedNotifications.Should().ContainSingle()
            .Which.Should().BeOfType<DomainEventNotification<ProductCreated>>()
            .Which.DomainEvent.EntityId.Should().Be(GeneratedId);
        await mediator.Received(1).Publish(Arg.Any<INotification>(), cancellationToken);
    }

    [Fact]
    public async Task Handle_WhenSkuAlreadyExists_ThrowsCreateFailedExceptionAndPublishesNothing()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1), sku: "LAP-DEL-000001"));
        var command = new AddProductCommand("lap-del-000001", "Another Laptop", 10m, "PLN");

        // Act
        var act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>();
        publishedNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCommandViolatingDomainRules_ThrowsAndDoesNotAddProductOrPublish()
    {
        // Arrange
        var command = new AddProductCommand("LAP-DEL-000001", "Dell Laptop", 0m, "PLN");

        // Act
        var act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
        dbContext.ChangeTracker.HasChanges().Should().BeFalse();
        publishedNotifications.Should().BeEmpty();
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();

    // The handler only stages changes; in the application UnitOfWorkBehavior saves them after the handler completes.
    private async Task CommitUnitOfWorkAsync() => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);
}
