using System;

namespace EzCatalog.Application.Queries;

public record ProductResult(
    Guid Id,
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency)
{
}
