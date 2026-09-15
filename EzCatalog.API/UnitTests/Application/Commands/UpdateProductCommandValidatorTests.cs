using System;
using System.Linq;
using EzCatalog.Application.Commands;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Commands;

public sealed class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator validator = new UpdateProductCommandValidator();

    [Fact]
    public void Validate_WithAllPropertiesProvidedAndValid_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, "New name", 19.99m, "EUR");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithOnlyIdProvided_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, null, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ReturnsIdError()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.Empty, null, null, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(UpdateProductCommand.Id),
            ErrorMessage = "ProductId cannot be empty.",
        });
    }

    [Theory]
    [InlineData("", "Product name cannot be empty.")]
    [InlineData("   ", "Product name cannot be empty.")]
    [InlineData("Lap\ntop", "Product name cannot contain control characters.")]
    public void Validate_WithInvalidName_ReturnsNameError(string name, string expectedMessage)
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, name, null, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(UpdateProductCommand.Name),
            ErrorMessage = expectedMessage,
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Validate_WithNonPositivePriceAmount_ReturnsPriceAmountError(decimal priceAmount)
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, priceAmount, "PLN");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(UpdateProductCommand.PriceAmount),
            ErrorMessage = "Product price must be positive.",
        });
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("usd")]
    public void Validate_WithUnknownCurrency_ReturnsCurrencyError(string priceCurrency)
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, 10m, priceCurrency);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(UpdateProductCommand.PriceCurrency),
            ErrorMessage = "Currency must be one of: PLN, EUR, USD.",
        });
    }

    [Fact]
    public void Validate_WithEmptyCurrency_ReturnsCurrencyErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, 10m, string.Empty);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().NotBeEmpty()
            .And.OnlyContain(error => error.PropertyName == nameof(UpdateProductCommand.PriceCurrency))
            .And.Contain(error => error.ErrorCode == "NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithPriceAmountWithoutCurrency_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, 10m, null);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithCurrencyWithoutPriceAmount_ReturnsNoErrors()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductTestData.DefaultId, null, null, "EUR");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithAllPropertiesInvalid_ReturnsErrorForEachProperty()
    {
        // Arrange
        var command = new UpdateProductCommand(Guid.Empty, " ", -1m, "GBP");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Select(error => error.PropertyName).Should().BeEquivalentTo(
            nameof(UpdateProductCommand.Id),
            nameof(UpdateProductCommand.Name),
            nameof(UpdateProductCommand.PriceAmount),
            nameof(UpdateProductCommand.PriceCurrency));
    }
}
