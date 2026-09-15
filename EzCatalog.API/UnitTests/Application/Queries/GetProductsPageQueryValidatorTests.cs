using System;
using EzCatalog.Application.Queries;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Queries;

public sealed class GetProductsPageQueryValidatorTests
{
    private const int ValidLimit = 20;

    private readonly GetProductsPageQueryValidator validator = new GetProductsPageQueryValidator();

    [Fact]
    public void Validate_WithoutCursor_ReturnsNoErrors()
    {
        // Arrange
        var query = new GetProductsPageQuery(null, ValidLimit);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNonEmptyCursor_ReturnsNoErrors()
    {
        // Arrange
        var query = new GetProductsPageQuery(ProductTestData.DefaultId, ValidLimit);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCursor_ReturnsCursorError()
    {
        // Arrange
        var query = new GetProductsPageQuery(Guid.Empty, ValidLimit);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(GetProductsPageQuery.Cursor),
            ErrorMessage = "ProductId cannot be empty.",
        });
    }

    [Theory]
    [InlineData(10)]
    [InlineData(55)]
    [InlineData(100)]
    public void Validate_WithLimitWithinRange_ReturnsNoErrors(int limit)
    {
        // Arrange
        var query = new GetProductsPageQuery(null, limit);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(9)]
    [InlineData(101)]
    [InlineData(int.MaxValue)]
    public void Validate_WithLimitOutOfRange_ReturnsLimitError(int limit)
    {
        // Arrange
        var query = new GetProductsPageQuery(null, limit);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(GetProductsPageQuery.Limit),
            ErrorCode = "InclusiveBetweenValidator",
        });
    }
}
