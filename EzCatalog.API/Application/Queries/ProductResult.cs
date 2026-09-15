using System;
using EzCatalog.Domain.Products;

namespace EzCatalog.Application.Queries;

public record ProductResult(
    Guid Id,
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency)
{
    public static ProductResult FromProduct(Product product)
    {
        return new ProductResult(
            product.Id.Value,
            product.Sku.Value,
            product.Name.Value,
            product.Price.Amount,
            product.Price.Currency.ToString());
    }
}
