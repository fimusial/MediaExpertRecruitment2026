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
    public void New_WithValidArguments_SetsProperties()
    {
        // Arrange
        var id = ProductId.Create(ProductGuid);
        var sku = Sku.Create("LAP-DEL-000001");
        var name = ProductName.Create("Dell Laptop");
        var price = new Money(4999.99m, Currency.PLN);

        // Act
        var product = Product.New(id, sku, name, price);

        // Assert
        product.Id.Should().Be(id);
        product.Sku.Should().Be(sku);
        product.Name.Should().Be(name);
        product.Price.Should().Be(price);
    }

    [Fact]
    public void New_WithValidArguments_PublishesSingleProductCreatedEvent()
    {
        // Act
        var product = Product.New(
            ProductId.Create(ProductGuid),
            Sku.Create(ProductTestData.DefaultSku),
            ProductName.Create(ProductTestData.DefaultName),
            new Money(ProductTestData.DefaultPriceAmount, ProductTestData.DefaultCurrency));

        // Assert
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>()
            .Which.EntityId.Should().Be(ProductGuid);
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void New_WithNonPositivePrice_ThrowsInvalidPriceException(decimal amount)
    {
        // Arrange
        var price = new Money(amount, Currency.PLN);

        // Act
        var act = () => Product.New(
            ProductId.Create(ProductGuid),
            Sku.Create(ProductTestData.DefaultSku),
            ProductName.Create(ProductTestData.DefaultName),
            price);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {price}. It must be positive.");
    }

    [Fact]
    public void New_WithSmallestPositivePrice_CreatesProduct()
    {
        // Act
        var product = ProductTestData.CreateProduct(priceAmount: 0.01m);

        // Assert
        product.Price.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Transfer_WithValidPrimitives_CreatesProductWithNormalizedValues()
    {
        // Act
        var product = Product.Rehydrate(ProductGuid, " lap-del-000001 ", "  Dell Laptop  ", 4999.99m, "EUR");

        // Assert
        product.Id.Value.Should().Be(ProductGuid);
        product.Sku.Value.Should().Be("LAP-DEL-000001");
        product.Name.Value.Should().Be("Dell Laptop");
        product.Price.Should().Be(new Money(4999.99m, Currency.EUR));
    }

    [Fact]
    public void Transfer_WithValidPrimitives_PublishesNoDomainEvents()
    {
        // Act
        var product = CreateTransferredProduct();

        // Assert
        product.GetPublishedDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void Transfer_WithEmptyId_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => Product.Rehydrate(Guid.Empty, "LAP-DEL-000001", "Dell Laptop", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Transfer_WithInvalidSku_ThrowsInvalidSkuException()
    {
        // Act
        var act = () => Product.Rehydrate(ProductGuid, "-invalid-", "Dell Laptop", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<InvalidSkuException>();
    }

    [Fact]
    public void Transfer_WithInvalidName_ThrowsInvalidProductNameException()
    {
        // Act
        var act = () => Product.Rehydrate(ProductGuid, "LAP-DEL-000001", " ", 4999.99m, "PLN");

        // Assert
        act.Should().Throw<InvalidProductNameException>();
    }

    [Theory]
    [InlineData(0.00)]
    [InlineData(-0.01)]
    public void Transfer_WithNonPositivePrice_ThrowsInvalidPriceException(decimal amount)
    {
        // Act
        var act = () => Product.Rehydrate(ProductGuid, "LAP-DEL-000001", "Dell Laptop", amount, "PLN");

        // Assert
        act.Should().Throw<InvalidPriceException>();
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("pln")]
    [InlineData("")]
    public void Transfer_WithUnknownCurrency_ThrowsArgumentException(string currency)
    {
        // Act
        var act = () => Product.Rehydrate(ProductGuid, "LAP-DEL-000001", "Dell Laptop", 4999.99m, currency);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateName_ChangesName()
    {
        // Arrange
        var product = CreateTransferredProduct();
        var newName = ProductName.Create("New name");

        // Act
        product.UpdateName(newName);

        // Assert
        product.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_PublishesSingleProductNameChangedEvent()
    {
        // Arrange
        var product = CreateTransferredProduct();

        // Act
        product.UpdateName(ProductName.Create("New name"));

        // Assert
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductNameChanged>()
            .Which.EntityId.Should().Be(ProductGuid);
    }

    [Fact]
    public void UpdateName_OnNewProduct_AppendsEventAfterProductCreated()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        product.UpdateName(ProductName.Create("New name"));

        // Assert
        product.GetPublishedDomainEvents().Should().SatisfyRespectively(
            first => first.Should().BeOfType<ProductCreated>(),
            second => second.Should().BeOfType<ProductNameChanged>());
    }

    [Fact]
    public void UpdateName_DoesNotChangeOtherProperties()
    {
        // Arrange
        var product = CreateTransferredProduct();
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
        var product = CreateTransferredProduct();
        var newPrice = new Money(25.99m, Currency.EUR);

        // Act
        product.UpdatePrice(newPrice);

        // Assert
        product.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_WithPositivePrice_PublishesSingleProductPriceChangedEvent()
    {
        // Arrange
        var product = CreateTransferredProduct();

        // Act
        product.UpdatePrice(new Money(25.99m, Currency.EUR));

        // Assert
        product.GetPublishedDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<ProductPriceChanged>()
            .Which.EntityId.Should().Be(ProductGuid);
    }

    [Fact]
    public void UpdatePrice_OnNewProduct_AppendsEventAfterProductCreated()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        product.UpdatePrice(new Money(25.99m, Currency.EUR));

        // Assert
        product.GetPublishedDomainEvents().Should().SatisfyRespectively(
            first => first.Should().BeOfType<ProductCreated>(),
            second => second.Should().BeOfType<ProductPriceChanged>());
    }

    [Fact]
    public void UpdatePrice_WithPositivePrice_DoesNotChangeOtherProperties()
    {
        // Arrange
        var product = CreateTransferredProduct();
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
        var product = CreateTransferredProduct();
        var newPrice = new Money(amount, Currency.PLN);

        // Act
        var act = () => product.UpdatePrice(newPrice);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {newPrice}. It must be positive.");
    }

    [Fact]
    public void UpdatePrice_WithNonPositivePrice_KeepsPriceAndPublishesNoEvent()
    {
        // Arrange
        var product = CreateTransferredProduct();
        var originalPrice = product.Price;

        // Act
        var act = () => product.UpdatePrice(new Money(0m, Currency.PLN));

        // Assert
        act.Should().Throw<InvalidPriceException>();
        product.Price.Should().Be(originalPrice);
        product.GetPublishedDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void ToString_ReturnsTypeNameAndId()
    {
        // Arrange
        var product = CreateTransferredProduct();

        // Act
        var result = product.ToString();

        // Assert
        result.Should().Be($"Product {ProductGuid}");
    }

    [Theory]
    [InlineData(0.001)]
    [InlineData(19.999)]
    public void New_WithPriceBelowSmallestUnit_ThrowsInvalidPriceException(decimal amount)
    {
        // Arrange
        var price = new Money(amount, Currency.PLN);

        // Act
        var act = () => ProductTestData.CreateProduct(priceAmount: amount);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {price}. It must have at most 2 decimal places.");
    }

    [Fact]
    public void New_WithPriceAboveMaxAmount_ThrowsInvalidPriceException()
    {
        // Arrange
        var amount = Product.PriceMaxAmount + 1m;
        var price = new Money(amount, Currency.PLN);

        // Act
        var act = () => ProductTestData.CreateProduct(priceAmount: amount);

        // Assert
        act.Should().Throw<InvalidPriceException>()
            .WithMessage($"Invalid Product price: {price}. It must not exceed {Product.PriceMaxAmount}.");
    }

    [Fact]
    public void New_WithPriceAtMaxAmount_CreatesProduct()
    {
        // Act
        var product = ProductTestData.CreateProduct(priceAmount: Product.PriceMaxAmount);

        // Assert
        product.Price.Amount.Should().Be(Product.PriceMaxAmount);
    }

    [Fact]
    public void New_WithTrailingZerosInPrice_CreatesProduct()
    {
        // Act
        var product = ProductTestData.CreateProduct(priceAmount: 10.500m);

        // Assert
        product.Price.Amount.Should().Be(10.5m);
    }

    [Fact]
    public void UpdatePrice_WithTooManyDecimalPlaces_ThrowsInvalidPriceExceptionAndKeepsPrice()
    {
        // Arrange
        var product = CreateTransferredProduct();
        var originalPrice = product.Price;

        // Act
        var act = () => product.UpdatePrice(new Money(12.345m, Currency.PLN));

        // Assert
        act.Should().Throw<InvalidPriceException>();
        product.Price.Should().Be(originalPrice);
        product.GetPublishedDomainEvents().Should().BeEmpty();
    }

    private static Product CreateTransferredProduct() =>
        Product.Rehydrate(ProductGuid, "LAP-DEL-000001", "Dell Laptop", 100.00m, "PLN");
}
