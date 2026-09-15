using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework.Models;
using EzCatalog.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EzCatalog.Infrastructure.EntityFramework.Adapters;

public class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext catalogDbContext;

    public ProductRepository(CatalogDbContext catalogDbContext)
    {
        this.catalogDbContext = catalogDbContext;
    }

    public async Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken)
    {
        var dbModel = await catalogDbContext
            .Products
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id.Value, cancellationToken);

        if (dbModel == null)
        {
            return null;
        }

        return Product.Create(
            dbModel.Id,
            dbModel.Sku,
            dbModel.Name,
            dbModel.PriceAmount,
            dbModel.PriceCurrency);
    }

    public async Task<Guid> AddAsync(Product product, CancellationToken cancellationToken)
    {
        if (await catalogDbContext.Products.AnyAsync(x => x.Sku == product.Sku.Value, cancellationToken))
        {
            throw new CreateFailedException($"Product with SKU {product.Sku.Value} already exists.");
        }

        var dbModel = new ProductDbModel
        {
            Id = product.Id.Value,
            Sku = product.Sku.Value,
            Name = product.Name.Value,
            PriceAmount = product.Price.Amount,
            PriceCurrency = product.Price.Currency.ToString(),
        };

        var added = await catalogDbContext.Products.AddAsync(dbModel, cancellationToken);
        return added.Entity.Id;
    }

    public async Task UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        var dbModel = await catalogDbContext
            .Products
            .SingleOrDefaultAsync(x => x.Id == product.Id.Value, cancellationToken);

        if (dbModel == null)
        {
            throw new UpdateFailedException($"Expected to update {product}, but DB model was not found.");
        }

        dbModel.Name = product.Name.Value;
        dbModel.PriceAmount = product.Price.Amount;
        dbModel.PriceCurrency = product.Price.Currency.ToString();
    }
}
