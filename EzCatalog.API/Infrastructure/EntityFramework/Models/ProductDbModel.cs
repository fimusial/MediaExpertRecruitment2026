using System;

namespace EzCatalog.Infrastructure.EntityFramework.Models;

public class ProductDbModel
{
    public Guid Id { get; set; }

    public string Sku { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal PriceAmount { get; set; }

    public string PriceCurrency { get; set; } = string.Empty;
}
