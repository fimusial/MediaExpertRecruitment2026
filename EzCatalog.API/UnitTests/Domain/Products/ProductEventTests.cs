using System;
using EzCatalog.Domain.Products;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Domain.Products;

public sealed class ProductEventTests
{
    public static TheoryData<string, Func<ProductId, ProductEvent>> EventFactories { get; } = new TheoryData<string, Func<ProductId, ProductEvent>>
    {
        { nameof(ProductCreated), id => new ProductCreated(id) },
        { nameof(ProductNameChanged), id => new ProductNameChanged(id) },
        { nameof(ProductPriceChanged), id => new ProductPriceChanged(id) },
    };

    [Theory]
    [MemberData(nameof(EventFactories))]
    public void Constructor_SetsEntityIdFromProductId(string eventName, Func<ProductId, ProductEvent> createEvent)
    {
        // Arrange
        var productId = ProductId.Create(ProductTestData.DefaultId);

        // Act
        var domainEvent = createEvent(productId);

        // Assert
        domainEvent.GetType().Name.Should().Be(eventName);
        domainEvent.EntityId.Should().Be(ProductTestData.DefaultId);
    }

    [Theory]
    [MemberData(nameof(EventFactories))]
    public void EntityType_IsProduct(string eventName, Func<ProductId, ProductEvent> createEvent)
    {
        // Arrange
        var productId = ProductId.Create(ProductTestData.DefaultId);

        // Act
        var domainEvent = createEvent(productId);

        // Assert
        domainEvent.GetType().Name.Should().Be(eventName);
        domainEvent.EntityType.Should().Be("Product");
    }

    [Fact]
    public void Equals_WithSameTypeAndProductId_ReturnsTrue()
    {
        // Arrange
        var productId = ProductId.Create(ProductTestData.DefaultId);

        // Act & Assert
        new ProductNameChanged(productId).Should().Be(new ProductNameChanged(productId));
    }

    [Fact]
    public void Equals_WithDifferentTypeAndSameProductId_ReturnsFalse()
    {
        // Arrange
        var productId = ProductId.Create(ProductTestData.DefaultId);

        // Act & Assert
        new ProductNameChanged(productId).Should().NotBe(new ProductPriceChanged(productId));
    }
}
