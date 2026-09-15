using System;
using System.Linq;
using System.Threading.Tasks;
using EzCatalog.Application.Queries;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Application.Queries;

public sealed class GetProductsPageQueryHandlerTests : IAsyncDisposable
{
    private const int Limit = 10;

    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly GetProductsPageQueryHandler handler;

    public GetProductsPageQueryHandlerTests()
    {
        dbContext = database.CreateContext();
        handler = new GetProductsPageQueryHandler(new ProductQueryService(dbContext));
    }

    [Fact]
    public async Task Handle_WithoutCursor_ReturnsFirstPageOfProducts()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(15));

        // Act
        var result = await handler.Handle(new GetProductsPageQuery(null, Limit), TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Products.Select(product => product.Id).Should().Equal(
            Enumerable.Range(1, 10).Select(ProductTestData.SequentialId));
        result.NextCursor.Should().Be(ProductTestData.SequentialId(10));
    }

    [Fact]
    public async Task Handle_WithCursor_ReturnsProductsAfterCursor()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(15));
        var cursor = ProductTestData.SequentialId(10);

        // Act
        var result = await handler.Handle(new GetProductsPageQuery(cursor, Limit), TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(15);
        result.Products.Select(product => product.Id).Should().Equal(
            Enumerable.Range(11, 5).Select(ProductTestData.SequentialId));
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyCatalog_ReturnsEmptyPage()
    {
        // Act
        var result = await handler.Handle(new GetProductsPageQuery(null, Limit), TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Products.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();
}
