using System.Linq;
using EzCatalog.Application.Commands;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Commands;

public sealed class AddProductCommandValidatorTests
{
    private const string CurrencyError = "Currency must be one of: PLN, EUR, USD.";

    private readonly AddProductCommandValidator validator = new AddProductCommandValidator();

    [Theory]
    [InlineData("LAP-DEL-000001", "Dell Laptop", 4999.99, "PLN")]
    [InlineData("abcd", "  X  ", 0.01, "EUR")]
    [InlineData("A1-B2-C3", "Ekspres De'Longhi", 100, "USD")]
    public void Validate_WithValidCommand_ReturnsNoErrors(string sku, string name, decimal priceAmount, string priceCurrency)
    {
        // Arrange
        var command = new AddProductCommand(sku, name, priceAmount, priceCurrency);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "SKU cannot be empty.")]
    [InlineData("  ", "SKU cannot be empty.")]
    [InlineData("ABC", "Normalized SKU must be between 4 and 32 characters.")]
    [InlineData("AB--CD", "SKU may contain only letters, digits and single hyphens, and cannot start or end with a hyphen.")]
    public void Validate_WithInvalidSku_ReturnsSkuErrorFromDomainRules(string? sku, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand() with { Sku = sku! };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(AddProductCommand.Sku),
            ErrorMessage = expectedMessage,
        });
    }

    [Theory]
    [InlineData(null, "Product name cannot be empty.")]
    [InlineData("", "Product name cannot be empty.")]
    [InlineData("Lap\ttop", "Product name cannot contain control characters.")]
    public void Validate_WithInvalidName_ReturnsNameErrorFromDomainRules(string? name, string expectedMessage)
    {
        // Arrange
        var command = CreateValidCommand() with { Name = name! };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(AddProductCommand.Name),
            ErrorMessage = expectedMessage,
        });
    }

    [Fact]
    public void Validate_WithNameLongerThanMaxLength_ReturnsNameError()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = new string('a', 501) };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(AddProductCommand.Name),
            ErrorMessage = "Normalized product name must be between 1 and 500 characters.",
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-1000)]
    public void Validate_WithNonPositivePriceAmount_ReturnsPriceAmountError(decimal priceAmount)
    {
        // Arrange
        var command = CreateValidCommand() with { PriceAmount = priceAmount };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(AddProductCommand.PriceAmount),
            ErrorMessage = "Product price must be positive.",
        });
    }

    [Theory]
    [InlineData("GBP")]
    [InlineData("pln")]
    [InlineData("1")]
    [InlineData(" PLN")]
    public void Validate_WithUnknownCurrency_ReturnsCurrencyError(string priceCurrency)
    {
        // Arrange
        var command = CreateValidCommand() with { PriceCurrency = priceCurrency };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(AddProductCommand.PriceCurrency),
            ErrorMessage = CurrencyError,
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_WithMissingCurrency_ReturnsNotEmptyError(string? priceCurrency)
    {
        // Arrange
        var command = CreateValidCommand() with { PriceCurrency = priceCurrency! };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Should().NotBeEmpty()
            .And.OnlyContain(error => error.PropertyName == nameof(AddProductCommand.PriceCurrency))
            .And.Contain(error => error.ErrorCode == "NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithAllPropertiesInvalid_ReturnsErrorForEachProperty()
    {
        // Arrange
        var command = new AddProductCommand("-", " ", 0m, "GBP");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.Errors.Select(error => error.PropertyName).Should().BeEquivalentTo(
            nameof(AddProductCommand.Sku),
            nameof(AddProductCommand.Name),
            nameof(AddProductCommand.PriceAmount),
            nameof(AddProductCommand.PriceCurrency));
    }

    private static AddProductCommand CreateValidCommand() =>
        new AddProductCommand("LAP-DEL-000001", "Dell Laptop", 4999.99m, "PLN");
}
