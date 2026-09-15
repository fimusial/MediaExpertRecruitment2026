using System;
using EzCatalog.Domain.Products;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.Domain.Products.Exceptions;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Domain.Products;

public sealed class ProductTests
{
    private static readonly Guid ProductGuid = ProductTestData.DefaultId;

    [Fact]
    public void Constructor_WithValidArguments_SetsProperties()
    {
        // Arrange
        var id = ProductId.Create(ProductGuid);
        var sku = Sku.Create("LAP-DEL-000001");
        var name = ProductName.Create("Dell Laptop");
        var price = new Money(4999.99m, Currency.PLN);

        // Act
        var product = new Product(id, sku, name, price);

        // Assert
        product.Id.Should().Be(id);
        product.Sku.Should().Be(sku);
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
    }

    [Fact]
    public void Constructor_WithValidArguments_PublishesSingleProductCreatedEvent()
    {
        // Act
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Assert
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>()
            .Which.EntityId.Should().Be(ProductGuid);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Constructor_WithNonPositivePrice_ThrowsInvalidPriceException(decimal amount)
    {
        // Arrange
        var price = new Money(amount, Currency.PLN);

        // Act
        var act = () => new Product(
            ProductId.Create(ProductGuid),
            Sku.Create(ProductTestData.DefaultSku),
            ProductName.Create(ProductTestData.DefaultName),
            price);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {price}. It must be positive.");
    }

    [Fact]
    public void Constructor_WithSmallestPositivePrice_CreatesProduct()
    {
        // Act
        var product = ProductTestData.CreateProduct(priceAmount: 0.01m);

        // Assert
        product.Price.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Create_WithValidPrimitives_CreatesProductWithNormalizedValues()
    {
        // Act
        var product = Product.Create(ProductGuid, " lap-del-000001 ", "  Dell Laptop  ", 4999.99m, "EUR");

        // Assert
        product.Id.Value.Should().Be(ProductGuid);
        product.Sku.Value.Should().Be("LAP-DEL-000001");
        product.Name.Value.Should().Be("Dell Laptop");
        product.Price.Should().Be(new Money(4999.99m, Currency.EUR));
    }

    [Fact]
    public void Create_WithValidPrimitives_PublishesSingleProductCreatedEvent()
    {
        // Act
        var product = Product.Create(ProductGuid, "LAP-DEL-000001", "Dell Laptop", 4999.99m, "USD");

        // Assert
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>()
            .Which.EntityId.Should().Be(ProductGuid);
    }

    [Fact]
    public void Create_WithEmptyId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => Product.Create(Guid.Empty, "LAP-DEL-000001", "Dell Laptop", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_WithInvalidSku_ThrowsInvalidSkuException()
    {
        // Act
        var act = () => Product.Create(ProductGuid, "-invalid-", "Dell Laptop", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<InvalidSkuException>();
    }

    [Fact]
    public void Create_WithInvalidName_ThrowsInvalidProductNameException()
    {
        // Act
        var act = () => Product.Create(ProductGuid, "LAP-DEL-000001", " ", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<InvalidProductNameException>();
    }

    [Fact]
    public void Create_WithNonPositivePrice_ThrowsInvalidPriceException()
    {
        // Act
        var act = () => Product.Create(ProductGuid, "LAP-DEL-000001", "Dell Laptop", 0m, "PLN");

        // Assert
        act.Should().Throw<InvalidPriceException>();
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("pln")]
    [InlineData("")]
    public void Create_WithUnknownCurrency_ThrowsArgumentException(string currency)
    {
        // Act
        var act = () => Product.Create(ProductGuid, "LAP-DEL-000001", "Dell Laptop", 4999.99m, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_ChangesName()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(name: "Old name");
        var newName = ProductName.Create("New name");

        // Act
        product.UpdateName(newName);

        // Assert
        product.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_AppendsProductNameChangedEvent()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        product.UpdateName(ProductName.Create("New name"));

        // Assert
        product.GetPublishedDomainEvents().Should().SatisfyRespectively(
            first => first.Should().BeOfType<ProductCreated>(),
            second => second.Should().BeOfType<ProductNameChanged>()
                .Which.EntityId.Should().Be(ProductGuid));
    }

    [Fact]
    public void UpdateName_DoesNotChangeOtherProperties()
    {
        // Arrange
        var product = ProductTestData.CreateProduct();
        var (id, sku, price) = (product.Id, product.Sku, product.Price);

        // Act
        product.UpdateName(ProductName.Create("New name"));

        // Assert
        product.Id.Should().Be(id);
        product.Sku.Should().Be(sku);
        product.Price.Should().Be(price);
    }

    [Fact]
    public void UpdatePrice_WithPositivePrice_ChangesPrice()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(priceAmount: 100m, currency: Currency.PLN);
        var newPrice = new Money(25.99m, Currency.EUR);

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_WithPositivePrice_AppendsProductPriceChangedEvent()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        product.UpdatePrice(new Money(25.99m, Currency.EUR));

        // Assert
        product.GetPublishedDomainEvents().Should().SatisfyRespectively(
            first => first.Should().BeOfType<ProductCreated>(),
            second => second.Should().BeOfType<ProductPriceChanged>()
                .Which.EntityId.Should().Be(ProductGuid));
    }

    [Fact]
    public void UpdatePrice_WithPositivePrice_DoesNotChangeOtherProperties()
    {
        // Arrange
        var product = ProductTestData.CreateProduct();
        var (id, sku, name) = (product.Id, product.Sku, product.Name);

        // Act
        product.UpdatePrice(new Money(25.99m, Currency.EUR));

        // Assert
        product.Id.Should().Be(id);
        product.Sku.Should().Be(sku);
        product.Name.Should().Be(name);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    public void UpdatePrice_WithNonPositivePrice_ThrowsInvalidPriceException(decimal amount)
    {
        // Arrange
        var product = ProductTestData.CreateProduct();
        var newPrice = new Money(amount, Currency.PLN);

        // Act
        var act = () => product.UpdatePrice(newPrice);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {newPrice}. It must be positive.");
    }

    [Fact]
    public void UpdatePrice_WithNonPositivePrice_KeepsPriceAndDoesNotAppendEvent()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(priceAmount: 100m);
        var originalPrice = product.Price;

        // Act
        var act = () => product.UpdatePrice(new Money(0m, Currency.PLN));

        // Assert
        act.Should().Throw<InvalidPriceException>();
        product.Price.Should().Be(originalPrice);
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>();
    }

    [Fact]
    public void ToString_ReturnsTypeNameAndId()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        var result = product.ToString();

        // Assert
        result.Should().Be($"Product {ProductGuid}");
    }
}
