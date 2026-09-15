using System;
using EzCatalog.Domain.Products;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Domain.Products;

public sealed class ProductIdTests
{
    private static readonly Guid SomeGuid = new Guid("0198a8e2-4f5b-7c3d-9e1f-2a3b4c5d6e7f");

    [Fact]
    public void Create_WithNonEmptyGuid_ReturnsIdWithValue()
    {
        // Act
        var id = ProductId.Create(SomeGuid);

        // Assert
        id.Value.Should().Be(SomeGuid);
    }

    [Fact]
    public void Create_WithEmptyGuid_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => ProductId.Create(Guid.Empty);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("value")
            .WithMessage("Guid value for ProductId cannot be empty.*");
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        // Arrange
        var id = ProductId.Create(SomeGuid);

        // Act
        var result = id.ToString();

        // Assert
        result.Should().Be("0198a8e2-4f5b-7c3d-9e1f-2a3b4c5d6e7f");
    }

    [Fact]
    public void Equals_WithSameGuid_ReturnsTrue()
    {
        // Arrange
        var first = ProductId.Create(SomeGuid);
        var second = ProductId.Create(SomeGuid);

        // Act & Assert
        first.Should().Be(second);
    }
}
