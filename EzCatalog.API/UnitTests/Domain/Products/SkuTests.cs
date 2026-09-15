using EzCatalog.Domain.Products;
using EzCatalog.Domain.Products.Exceptions;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Domain.Products;

public sealed class SkuTests
{
    private const string EmptyError = "SKU cannot be empty.";
    private const string LengthError = "Normalized SKU must be between 4 and 32 characters.";
    private const string PatternError = "SKU may contain only letters, digits and single hyphens, and cannot start or end with a hyphen.";

    [Theory]
    [InlineData("ABCD")]
    [InlineData("1234")]
    [InlineData("AB-CD")]
    [InlineData("LAP-DEL-000123")]
    [InlineData("A1-B2-C3")]
    public void TryCreate_WithValidValue_ReturnsTrueAndSku(string value)
    {
        // Act
        var result = Sku.TryCreate(value, out var sku, out var error);

        // Assert
        result.Should().BeTrue();
        sku.Should().NotBeNull();
        sku!.Value.Should().Be(value);
        error.Should().BeNull();
    }

    [Theory]
    [InlineData("abcd", "ABCD")]
    [InlineData("  AB-CD  ", "AB-CD")]
    [InlineData("\tlap-del-1\n", "LAP-DEL-1")]
    public void TryCreate_WithNonNormalizedValue_TrimsAndUppercases(string value, string expected)
    {
        // Act
        var result = Sku.TryCreate(value, out var sku, out _);

        // Assert
        result.Should().BeTrue();
        sku!.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void TryCreate_WithNullOrWhiteSpace_ReturnsFalseAndEmptyError(string? value)
    {
        // Act
        var result = Sku.TryCreate(value, out var sku, out var error);

        // Assert
        result.Should().BeFalse();
        sku.Should().BeNull();
        error.Should().Be(EmptyError);
    }

    [Theory]
    [InlineData("ABC")]
    [InlineData("  ABC  ")]
    public void TryCreate_WithNormalizedValueShorterThanMinLength_ReturnsFalseAndLengthError(string value)
    {
        // Act
        var result = Sku.TryCreate(value, out var sku, out var error);

        // Assert
        result.Should().BeFalse();
        sku.Should().BeNull();
        error.Should().Be(LengthError);
    }

    [Fact]
    public void TryCreate_WithValueOfMinLength_ReturnsTrue()
    {
        // Arrange
        var value = new string('A', Sku.MinLength);

        // Act
        var result = Sku.TryCreate(value, out _, out _);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryCreate_WithValueOfMaxLength_ReturnsTrue()
    {
        // Arrange
        var value = new string('A', Sku.MaxLength);

        // Act
        var result = Sku.TryCreate(value, out _, out _);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryCreate_WithValueLongerThanMaxLength_ReturnsFalseAndLengthError()
    {
        // Arrange
        var value = new string('A', Sku.MaxLength + 1);

        // Act
        var result = Sku.TryCreate(value, out var sku, out var error);

        // Assert
        result.Should().BeFalse();
        sku.Should().BeNull();
        error.Should().Be(LengthError);
    }

    [Theory]
    [InlineData("-ABCD")]
    [InlineData("ABCD-")]
    [InlineData("AB--CD")]
    [InlineData("AB_CD")]
    [InlineData("AB CD")]
    [InlineData("AB.CD")]
    [InlineData("ĄBĆD")]
    public void TryCreate_WithCharactersNotMatchingPattern_ReturnsFalseAndPatternError(string value)
    {
        // Act
        var result = Sku.TryCreate(value, out var sku, out var error);

        // Assert
        result.Should().BeFalse();
        sku.Should().BeNull();
        error.Should().Be(PatternError);
    }

    [Fact]
    public void Create_WithValidValue_ReturnsNormalizedSku()
    {
        // Act
        var sku = Sku.Create(" lap-del-000123 ");

        // Assert
        sku.Value.Should().Be("LAP-DEL-000123");
    }

    [Theory]
    [InlineData(null, EmptyError)]
    [InlineData("ABC", LengthError)]
    [InlineData("AB--CD", PatternError)]
    public void Create_WithInvalidValue_ThrowsInvalidSkuExceptionWithError(string? value, string expectedError)
    {
        // Act
        var act = () => Sku.Create(value);

        // Assert
        act.Should().Throw<InvalidSkuException>().WithMessage(expectedError);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        var sku = Sku.Create("AB-CD");

        // Act
        var result = sku.ToString();

        // Assert
        result.Should().Be("AB-CD");
    }

    [Fact]
    public void Equals_WithSameNormalizedValue_ReturnsTrue()
    {
        // Arrange
        var first = Sku.Create("ab-cd");
        var second = Sku.Create(" AB-CD ");

        // Act & Assert
        first.Should().Be(second);
    }
}
