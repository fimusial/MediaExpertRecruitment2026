using System;
using System.Threading.Tasks;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace EzCatalog.UnitTests.Common;

public sealed class InMemoryCatalogDatabase
{
    private readonly DbContextOptions<CatalogDbContext> options;

    public InMemoryCatalogDatabase()
    {
        options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase($"EzCatalogTests-{Guid.NewGuid()}", new InMemoryDatabaseRoot())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    public CatalogDbContext CreateContext() => new CatalogDbContext(options);

    public async Task SeedAsync(params ProductDbModel[] products)
    {
        await using var context = CreateContext();
        context.Products.AddRange(products);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public async Task<ProductDbModel?> FindProductAsync(Guid id)
    {
        await using var context = CreateContext();
        return await context.Products.AsNoTracking().SingleOrDefaultAsync(
            product => product.Id == id,
            TestContext.Current.CancellationToken);
    }

    public async Task<int> CountProductsAsync()
    {
        await using var context = CreateContext();
        return await context.Products.CountAsync(TestContext.Current.CancellationToken);
    }
}
