using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Queries;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Xunit;

namespace EzCatalog.UnitTests.Infrastructure.EntityFramework.Adapters;

public sealed class ProductQueryServiceTests : IAsyncDisposable
{
    private const int Limit = 10;

    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly ProductQueryService queryService;

    public ProductQueryServiceTests()
    {
        dbContext = database.CreateContext();
        queryService = new ProductQueryService(dbContext);
    }

    [Fact]
    public async Task GetProductsPageAsync_WithEmptyCatalog_ReturnsEmptyPage()
    {
        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Products.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsPageAsync_WithFewerProductsThanLimit_ReturnsAllProductsWithoutCursor()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(Limit - 1));

        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(Limit - 1);
        IdsOf(result).Should().Equal(SequentialIds(1, Limit - 1));
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsPageAsync_WithExactlyLimitProducts_ReturnsAllProductsWithoutCursor()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(Limit));

        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(Limit);
        IdsOf(result).Should().Equal(SequentialIds(1, Limit));
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsPageAsync_WithMoreProductsThanLimit_ReturnsFirstPageAndCursorToLastReturnedProduct()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(Limit + 1));

        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(Limit + 1);
        IdsOf(result).Should().Equal(SequentialIds(1, Limit));
        result.NextCursor.Should().Be(ProductTestData.SequentialId(Limit));
    }

    [Fact]
    public async Task GetProductsPageAsync_ReturnsProductsOrderedById()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(5).Reverse().ToArray());

        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        IdsOf(result).Should().Equal(SequentialIds(1, 5));
    }

    [Fact]
    public async Task GetProductsPageAsync_WithCursor_ReturnsProductsAfterCursorAndTotalCountOfAllProducts()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(15));

        // Act
        var result = await queryService.GetProductsPageAsync(
            ProductTestData.SequentialId(3), Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(15);
        IdsOf(result).Should().Equal(SequentialIds(4, 10));
        result.NextCursor.Should().Be(ProductTestData.SequentialId(13));
    }

    [Fact]
    public async Task GetProductsPageAsync_WithCursorOfLastProduct_ReturnsEmptyPageWithoutCursor()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(5));

        // Act
        var result = await queryService.GetProductsPageAsync(
            ProductTestData.SequentialId(5), Limit, TestContext.Current.CancellationToken);

        // Assert
        result.TotalCount.Should().Be(5);
        result.Products.Should().BeEmpty();
        result.NextCursor.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsPageAsync_WithCursorNotMatchingAnyProduct_ReturnsProductsWithGreaterIds()
    {
        // Arrange
        await database.SeedAsync(
            ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(2), sku: ProductTestData.SequentialSku(2)),
            ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(4), sku: ProductTestData.SequentialSku(4)),
            ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(6), sku: ProductTestData.SequentialSku(6)));

        // Act
        var result = await queryService.GetProductsPageAsync(
            ProductTestData.SequentialId(3), Limit, TestContext.Current.CancellationToken);

        // Assert
        IdsOf(result).Should().Equal(ProductTestData.SequentialId(4), ProductTestData.SequentialId(6));
    }

    [Fact]
    public async Task GetProductsPageAsync_WhenFollowingCursors_ReturnsEveryProductExactlyOnce()
    {
        // Arrange
        const int productCount = (Limit * 2) + 5;
        await database.SeedAsync(ProductTestData.CreateDbModels(productCount));
        var pageSizes = new List<int>();
        var collectedIds = new List<Guid>();
        Guid? cursor = null;

        // Act
        do
        {
            var page = await queryService.GetProductsPageAsync(cursor, Limit, TestContext.Current.CancellationToken);
            pageSizes.Add(page.Products.Count());
            collectedIds.AddRange(IdsOf(page));
            cursor = page.NextCursor;
        }
        while (cursor is not null);

        // Assert
        pageSizes.Should().Equal(Limit, Limit, 5);
        collectedIds.Should().Equal(SequentialIds(1, productCount));
    }

    [Fact]
    public async Task GetProductsPageAsync_MapsProductsToResults()
    {
        // Arrange
        var id = ProductTestData.SequentialId(1);
        await database.SeedAsync(
            ProductTestData.CreateDbModel(id: id, sku: "TV-SAM-000042", name: "Samsung TV", priceAmount: 2499.50m, currency: Currency.EUR));

        // Act
        var result = await queryService.GetProductsPageAsync(null, Limit, TestContext.Current.CancellationToken);

        // Assert
        result.Products.Should().ContainSingle()
            .Which.Should().Be(new ProductResult(id, "TV-SAM-000042", "Samsung TV", 2499.50m, "EUR"));
    }

    [Fact]
    public async Task GetProductsPageAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModels(1));

        // Act
        var act = () => queryService.GetProductsPageAsync(null, Limit, new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();

    private static IEnumerable<Guid> IdsOf(ProductsPageResult page) => page.Products.Select(product => product.Id);

    private static IEnumerable<Guid> SequentialIds(int start, int count) =>
        Enumerable.Range(start, count).Select(ProductTestData.SequentialId);
}
