using System;
using System.Linq;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework.Models;

namespace EzCatalog.UnitTests.Common;

public static class ProductTestData
{
    public const string DefaultSku = "LAP-DEL-000001";
    public const string DefaultName = "Dell Laptop Pro 14 16GB/512GB";
    public const decimal DefaultPriceAmount = 4999.99m;
    public const Currency DefaultCurrency = Currency.PLN;

    public static Guid DefaultId { get; } = new Guid("0198a8e2-4f5b-7c3d-9e1f-2a3b4c5d6e7f");

    public static Product CreateProduct(
        Guid? id = null,
        string sku = DefaultSku,
        string name = DefaultName,
        decimal priceAmount = DefaultPriceAmount,
        Currency currency = DefaultCurrency)
    {
        return new Product(
            ProductId.Create(id ?? DefaultId),
            Sku.Create(sku),
            ProductName.Create(name),
            new Money(priceAmount, currency));
    }

    public static ProductDbModel CreateDbModel(
        Guid? id = null,
        string sku = DefaultSku,
        string name = DefaultName,
        decimal priceAmount = DefaultPriceAmount,
        Currency currency = DefaultCurrency)
    {
        return new ProductDbModel
        {
            Id = id ?? DefaultId,
            Sku = sku,
            Name = name,
            PriceAmount = priceAmount,
            PriceCurrency = currency.ToString(),
        };
    }

    public static ProductDbModel[] CreateDbModels(int count)
    {
        return Enumerable.Range(1, count)
            .Select(sequence => CreateDbModel(
                id: SequentialId(sequence),
                sku: SequentialSku(sequence),
                name: $"Product {sequence}"))
            .ToArray();
    }

    public static Guid SequentialId(int sequence)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sequence);
        return new Guid(sequence, 0, 0, new byte[8]);
    }

    public static string SequentialSku(int sequence) => $"SKU-{sequence:D6}";
}
