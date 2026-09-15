using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Commands;
using EzCatalog.Application.Exceptions;
using EzCatalog.Application.RequestPipeline;
using EzCatalog.Domain.Products;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.Domain.Products.Exceptions;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.Infrastructure.EntityFramework.Models;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EzCatalog.UnitTests.Application.Commands;

public sealed class UpdateProductCommandHandlerTests : IAsyncDisposable
{
    private const string OriginalSku = "LAP-DEL-000001";
    private const string OriginalName = "Original name";
    private const decimal OriginalPriceAmount = 100.00m;
    private const Currency OriginalCurrency = Currency.PLN;

    private static readonly Guid ProductGuid = ProductTestData.DefaultId;

    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly IMediator mediator = Substitute.For<IMediator>();
    private readonly List<INotification> publishedNotifications = new List<INotification>();
    private readonly UpdateProductCommandHandler handler;

    public UpdateProductCommandHandlerTests()
    {
        dbContext = database.CreateContext();

        mediator
            .When(substitute => substitute.Publish(Arg.Any<INotification>(), Arg.Any<CancellationToken>()))
            .Do(call => publishedNotifications.Add(call.Arg<INotification>()));

        handler = new UpdateProductCommandHandler(
            NullLogger<UpdateProductCommandHandler>.Instance,
            mediator,
            new ProductRepository(dbContext));
    }

    public static TheoryData<decimal?, string?> IncompletePrices { get; } = new TheoryData<decimal?, string?>
    {
        { 25.00m, null },
        { null, "EUR" },
    };

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ThrowsNotFoundExceptionAndPublishesNothing()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductGuid, "New name", null, null);

        // Act
        var act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with Id {ProductGuid} was not found.");
        publishedNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithName_UpdatesOnlyName()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, "  New name  ", null, null);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(ProductTestData.CreateDbModel(
            ProductGuid, OriginalSku, "New name", OriginalPriceAmount, OriginalCurrency));
    }

    [Fact]
    public async Task Handle_WithName_PublishesProductNameChangedNotification()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, "New name", null, null);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        PublishedEvents<ProductNameChanged>().Should().ContainSingle()
            .Which.EntityId.Should().Be(ProductGuid);
        PublishedEvents<ProductPriceChanged>().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithPriceAmountAndCurrency_UpdatesOnlyPrice()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, null, 25.99m, "USD");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(ProductTestData.CreateDbModel(
            ProductGuid, OriginalSku, OriginalName, 25.99m, Currency.USD));
    }

    [Fact]
    public async Task Handle_WithPriceAmountAndCurrency_PublishesProductPriceChangedNotification()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, null, 25.99m, "USD");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        PublishedEvents<ProductPriceChanged>().Should().ContainSingle()
            .Which.EntityId.Should().Be(ProductGuid);
        PublishedEvents<ProductNameChanged>().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithNameAndPrice_UpdatesBothAndPublishesBothNotifications()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, "New name", 25.99m, "EUR");

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(ProductTestData.CreateDbModel(
            ProductGuid, OriginalSku, "New name", 25.99m, Currency.EUR));
        PublishedEvents<ProductNameChanged>().Should().ContainSingle();
        PublishedEvents<ProductPriceChanged>().Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithoutChanges_ReturnsUnitAndKeepsProductUnchanged()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, null, null, null);

        // Act
        var result = await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        result.Should().Be(Unit.Value);
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(OriginalProduct());
        PublishedEvents<ProductNameChanged>().Should().BeEmpty();
        PublishedEvents<ProductPriceChanged>().Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(IncompletePrices))]
    public async Task Handle_WithIncompletePrice_KeepsPriceUnchanged(decimal? priceAmount, string? priceCurrency)
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, null, priceAmount, priceCurrency);

        // Act
        await handler.Handle(command, TestContext.Current.CancellationToken);
        await CommitUnitOfWorkAsync();

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(OriginalProduct());
        PublishedEvents<ProductPriceChanged>().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithCommandViolatingDomainRules_ThrowsAndKeepsProductUnchanged()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, "New name", 0m, "PLN");

        // Act
        var act = () => handler.Handle(command, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
        dbContext.ChangeTracker.HasChanges().Should().BeFalse();
        publishedNotifications.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PublishesNotificationsWithGivenCancellationToken()
    {
        // Arrange
        await SeedOriginalProductAsync();
        var command = new UpdateProductCommand(ProductGuid, "New name", null, null);
        var cancellationToken = TestContext.Current.CancellationToken;

        // Act
        await handler.Handle(command, cancellationToken);

        // Assert
        await mediator.Received().Publish(Arg.Any<INotification>(), cancellationToken);
        await mediator.DidNotReceive().Publish(Arg.Any<INotification>(), Arg.Is<CancellationToken>(token => token != cancellationToken));
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();

    private static ProductDbModel OriginalProduct() =>
        ProductTestData.CreateDbModel(ProductGuid, OriginalSku, OriginalName, OriginalPriceAmount, OriginalCurrency);

    private Task SeedOriginalProductAsync() => database.SeedAsync(OriginalProduct());

    // The handler only stages changes; in the application UnitOfWorkBehavior saves them after the handler completes.
    private async Task CommitUnitOfWorkAsync() => await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    private IEnumerable<TEvent> PublishedEvents<TEvent>()
        where TEvent : ProductEvent
    {
        return publishedNotifications
            .OfType<DomainEventNotification<TEvent>>()
            .Select(notification => notification.DomainEvent);
    }
}
