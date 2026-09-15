using System;
using EzCatalog.Application.Queries;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Queries;

public sealed class GetProductQueryValidatorTests
{
    private readonly GetProductQueryValidator validator = new GetProductQueryValidator();

    [Fact]
    public void Validate_WithNonEmptyId_ReturnsNoErrors()
    {
        // Arrange
        var query = new GetProductQuery(ProductTestData.DefaultId);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyId_ReturnsIdError()
    {
        // Arrange
        var query = new GetProductQuery(Guid.Empty);

        // Act
        var result = validator.Validate(query);

        // Assert
        result.Errors.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            PropertyName = nameof(GetProductQuery.Id),
            ErrorMessage = "ProductId cannot be empty.",
        });
    }
}
