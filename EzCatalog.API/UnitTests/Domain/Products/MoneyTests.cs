using System;
using System.Globalization;
using EzCatalog.Domain.Products;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Domain.Products;

public sealed class MoneyTests
{
    [Fact]
    public void Constructor_SetsAmountAndCurrency()
    {
        // Act
        var money = new Money(12.50m, Currency.EUR);

        // Assert
        money.Amount.Should().Be(12.50m);
        money.Currency.Should().Be(Currency.EUR);
    }

    [Theory]
    [InlineData(10.00, 2.50, 12.50)]
    [InlineData(10.00, 0.00, 10.00)]
    [InlineData(10.00, -2.50, 7.50)]
    public void Add_WithDecimal_ReturnsNewMoneyWithSumAndSameCurrency(decimal amount, decimal addend, decimal expected)
    {
        // Arrange
        var money = new Money(amount, Currency.PLN);

        // Act
        var result = money.Add(addend);

        // Assert
        result.Should().Be(new Money(expected, Currency.PLN));
    }

    [Fact]
    public void Add_WithDecimal_DoesNotModifyOriginal()
    {
        // Arrange
        var money = new Money(10.00m, Currency.PLN);

        // Act
        var result = money.Add(5.00m);

        // Assert
        result.Should().NotBeSameAs(money);
        money.Amount.Should().Be(10.00m);
    }

    [Theory]
    [InlineData(10.00, 3, 30.00)]
    [InlineData(10.00, 0.5, 5.00)]
    [InlineData(10.00, 0, 0.00)]
    public void Multiply_WithDecimal_ReturnsNewMoneyWithProductAndSameCurrency(decimal amount, decimal factor, decimal expected)
    {
        // Arrange
        var money = new Money(amount, Currency.USD);

        // Act
        var result = money.Multiply(factor);

        // Assert
        result.Should().Be(new Money(expected, Currency.USD));
    }

    [Fact]
    public void Multiply_WithDecimal_DoesNotModifyOriginal()
    {
        // Arrange
        var money = new Money(10.00m, Currency.USD);

        // Act
        var result = money.Multiply(2m);

        // Assert
        result.Should().NotBeSameAs(money);
        money.Amount.Should().Be(10.00m);
    }

    [Fact]
    public void Add_WithMoneyOfSameCurrency_ReturnsSum()
    {
        // Arrange
        var money = new Money(10.00m, Currency.EUR);
        var other = new Money(2.25m, Currency.EUR);

        // Act
        var result = money.Add(other);

        // Assert
        result.Should().Be(new Money(12.25m, Currency.EUR));
    }

    [Fact]
    public void Add_WithMoneyOfDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money = new Money(10.00m, Currency.EUR);
        var other = new Money(2.25m, Currency.PLN);

        // Act
        var act = () => money.Add(other);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("When adding Money objects, currencies must match.");
    }

    [Fact]
    public void Multiply_WithMoneyOfSameCurrency_ReturnsProduct()
    {
        // Arrange
        var money = new Money(10.00m, Currency.PLN);
        var other = new Money(1.5m, Currency.PLN);

        // Act
        var result = money.Multiply(other);

        // Assert
        result.Should().Be(new Money(15.00m, Currency.PLN));
    }

    [Fact]
    public void Multiply_WithMoneyOfDifferentCurrency_ThrowsInvalidOperationException()
    {
        // Arrange
        var money = new Money(10.00m, Currency.PLN);
        var other = new Money(1.5m, Currency.USD);

        // Act
        var act = () => money.Multiply(other);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("When multiplying Money object, currencies must match.");
    }

    [Theory]
    [InlineData("", "12.50 PLN")]
    [InlineData("pl-PL", "12,50 PLN")]
    public void ToString_ReturnsAmountFormattedInCurrentCultureFollowedByCurrency(string cultureName, string expected)
    {
        // Arrange
        var money = new Money(12.50m, Currency.PLN);
        using var cultureScope = new CultureScope(CultureInfo.GetCultureInfo(cultureName));

        // Act
        var result = money.ToString();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        // Arrange
        var first = new Money(12.50m, Currency.PLN);
        var second = new Money(12.50m, Currency.PLN);

        // Act & Assert
        first.Should().Be(second);
    }

    [Fact]
    public void Equals_WithSameAmountAndDifferentCurrency_ReturnsFalse()
    {
        // Arrange
        var first = new Money(12.50m, Currency.PLN);
        var second = new Money(12.50m, Currency.EUR);

        // Act & Assert
        first.Should().NotBe(second);
    }
}
