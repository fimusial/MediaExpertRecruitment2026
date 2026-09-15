using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.Infrastructure.EntityFramework.Models;
using EzCatalog.Infrastructure.Exceptions;
using EzCatalog.UnitTests.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EzCatalog.UnitTests.Infrastructure.EntityFramework.Adapters;

public sealed class ProductRepositoryTests : IAsyncDisposable
{
    private static readonly Guid ProductGuid = ProductTestData.DefaultId;

    private readonly InMemoryCatalogDatabase database = new InMemoryCatalogDatabase();
    private readonly CatalogDbContext dbContext;
    private readonly ProductRepository repository;

    public ProductRepositoryTests()
    {
        dbContext = database.CreateContext();
        repository = new ProductRepository(dbContext);
    }

    [Fact]
    public async Task GetAsync_WhenProductExists_ReturnsProductMappedFromDbModel()
    {
        // Arrange
        await database.SeedAsync(
            ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1), sku: "SKU-OTHER"),
            ProductTestData.CreateDbModel(id: ProductGuid, sku: "TV-SAM-000042", name: "Samsung TV", priceAmount: 2499.50m, currency: Currency.EUR));

        // Act
        var product = await repository.GetAsync(ProductId.Create(ProductGuid), TestContext.Current.CancellationToken);

        // Assert
        product.Should().NotBeNull();
        product!.Id.Value.Should().Be(ProductGuid);
        product.Sku.Value.Should().Be("TV-SAM-000042");
        product.Name.Value.Should().Be("Samsung TV");
        product.Price.Should().Be(new Money(2499.50m, Currency.EUR));
    }

    [Fact]
    public async Task GetAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1)));

        // Act
        var product = await repository.GetAsync(ProductId.Create(ProductGuid), TestContext.Current.CancellationToken);

        // Assert
        product.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_DoesNotTrackLoadedEntity()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductGuid));

        // Act
        await repository.GetAsync(ProductId.Create(ProductGuid), TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.Entries().Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductGuid));

        // Act
        var act = () => repository.GetAsync(ProductId.Create(ProductGuid), new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task AddAsync_WithNewProduct_ReturnsProductId()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        var id = await repository.AddAsync(product, TestContext.Current.CancellationToken);

        // Assert
        id.Should().Be(ProductGuid);
    }

    [Fact]
    public async Task AddAsync_WithNewProduct_StagesMappedEntityWithoutSaving()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(
            id: ProductGuid, sku: "TV-SAM-000042", name: "Samsung TV", priceAmount: 2499.50m, currency: Currency.USD);

        // Act
        await repository.AddAsync(product, TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.Entries<ProductDbModel>().Should().ContainSingle()
            .Which.Should().Match<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<ProductDbModel>>(
                entry => entry.State == EntityState.Added);
        (await database.CountProductsAsync()).Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_WithNewProduct_PersistsMappedEntityOnSaveChanges()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1), sku: "SKU-OTHER"));
        var product = ProductTestData.CreateProduct(
            id: ProductGuid, sku: "TV-SAM-000042", name: "Samsung TV", priceAmount: 2499.50m, currency: Currency.USD);

        // Act
        await repository.AddAsync(product, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(new ProductDbModel
        {
            Id = ProductGuid,
            Sku = "TV-SAM-000042",
            Name = "Samsung TV",
            PriceAmount = 2499.50m,
            PriceCurrency = "USD",
        });
    }

    [Fact]
    public async Task AddAsync_WhenSkuAlreadyExists_ThrowsCreateFailedExceptionAndStagesNothing()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1), sku: "LAP-DEL-000001"));
        var product = ProductTestData.CreateProduct(id: ProductGuid, sku: "LAP-DEL-000001");

        // Act
        var act = () => repository.AddAsync(product, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<CreateFailedException>()
            .WithMessage("Product with SKU LAP-DEL-000001 already exists.");
        dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task AddAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        var act = () => repository.AddAsync(product, new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_PersistsNameAndPriceOnSaveChanges()
    {
        // Arrange
        await database.SeedAsync(
            ProductTestData.CreateDbModel(id: ProductGuid, sku: "LAP-DEL-000001", name: "Old name", priceAmount: 100m, currency: Currency.PLN));
        var product = await repository.GetAsync(ProductId.Create(ProductGuid), TestContext.Current.CancellationToken);
        product!.UpdateName(ProductName.Create("New name"));
        product.UpdatePrice(new Money(25.99m, Currency.EUR));

        // Act
        await repository.UpdateAsync(product, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored.Should().BeEquivalentTo(new ProductDbModel
        {
            Id = ProductGuid,
            Sku = "LAP-DEL-000001",
            Name = "New name",
            PriceAmount = 25.99m,
            PriceCurrency = "EUR",
        });
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_DoesNotPersistBeforeSaveChanges()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductGuid, name: "Old name"));
        var product = ProductTestData.CreateProduct(id: ProductGuid, name: "New name");

        // Act
        await repository.UpdateAsync(product, TestContext.Current.CancellationToken);

        // Assert
        dbContext.ChangeTracker.HasChanges().Should().BeTrue();
        var stored = await database.FindProductAsync(ProductGuid);
        stored!.Name.Should().Be("Old name");
    }

    [Fact]
    public async Task UpdateAsync_WithDifferentSku_KeepsStoredSku()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductGuid, sku: "LAP-DEL-000001"));
        var product = ProductTestData.CreateProduct(id: ProductGuid, sku: "CHANGED-SKU");

        // Act
        await repository.UpdateAsync(product, TestContext.Current.CancellationToken);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Assert
        var stored = await database.FindProductAsync(ProductGuid);
        stored!.Sku.Should().Be("LAP-DEL-000001");
    }

    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ThrowsUpdateFailedException()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductTestData.SequentialId(1)));
        var product = ProductTestData.CreateProduct(id: ProductGuid);

        // Act
        var act = () => repository.UpdateAsync(product, TestContext.Current.CancellationToken);

        // Assert
        await act.Should().ThrowAsync<UpdateFailedException>()
            .WithMessage($"Expected to update Product {ProductGuid}, but it was not found.");
        dbContext.ChangeTracker.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await database.SeedAsync(ProductTestData.CreateDbModel(id: ProductGuid));
        var product = ProductTestData.CreateProduct(id: ProductGuid, name: "New name");

        // Act
        var act = () => repository.UpdateAsync(product, new CancellationToken(canceled: true));

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
        (await database.FindProductAsync(ProductGuid))!.Name.Should().Be(ProductTestData.DefaultName);
    }

    public ValueTask DisposeAsync() => dbContext.DisposeAsync();
}
