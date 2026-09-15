using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using EzCatalog.Application.Queries;
using EzCatalog.Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace EzCatalog.Infrastructure.EntityFramework.Adapters;

public class ProductQueryService : IProductQueryService
{
    private readonly CatalogDbContext catalogDbContext;

    public ProductQueryService(CatalogDbContext catalogDbContext)
    {
        this.catalogDbContext = catalogDbContext;
    }

    public async Task<ProductsPageResult> GetProductsPageAsync(Guid? cursor, int limit, CancellationToken cancellationToken)
    {
        var totalCount = await catalogDbContext.Products.CountAsync(cancellationToken);

        var products = await catalogDbContext
            .Products
            .OrderBy(x => x.Id)
            .Where(x => cursor == null || x.Id > cursor)
            .Take(limit + 1)
            .Select(x => ProductResult.FromProduct(
                Product.Rehydrate(x.Id, x.Sku, x.Name, x.PriceAmount, x.PriceCurrency)))
            .ToListAsync(cancellationToken);

        Guid? nextCursor = null;
        if (products.Count > limit)
        {
            products.RemoveAt(products.Count - 1);
            nextCursor = products.Last().Id;
        }

        return new ProductsPageResult(totalCount, nextCursor, products);
    }
}
