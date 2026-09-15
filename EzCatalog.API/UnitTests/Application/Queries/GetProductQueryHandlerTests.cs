using System;
using System.Threading.Tasks;
using EzCatalog.Application.Exceptions;
using EzCatalog.Application.Queries;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Queries;

public sealed class GetProductQueryHandlerTests : IAsyncDisposable
{
    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly GetProductQueryHandler handler;

    public GetProductQueryHandlerTests()
    {
        dbContext = database.CreateContext();
        handler = new GetProductQueryHandler(new ProductRepository(dbContext));
    }

    [Fact]
    public async Task Handle_WhenProductExists_ReturnsProductResult()
    {
        // Arrange
        var id = ProductTestData.SequentialId(2);
        await database.SeedAsync(
            ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1), sku: "SKU-OTHER"),
            ProductTestData.CreateDbModel(id: id, sku: "TV-SAM-000042", name: "Samsung TV", priceAmount: 2499.50m, currency: Currency.EUR));

        // Act
        var result = await handler.Handle(new GetProductQuery(id), TestContext.Current.CancellationToken);

        // Assert
        result.Should().Be(new ProductResult(id, "TV-SAM-000042", "Samsung TV", 2499.50m, "EUR"));
    }

    [Fact]
    public async Task Handle_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var missingId = ProductTestData.SequentialId(404);
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1)));

        // Act
        var act = () => handler.Handle(new GetProductQuery(missingId), TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with Id {missingId} was not found.");
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();
}
