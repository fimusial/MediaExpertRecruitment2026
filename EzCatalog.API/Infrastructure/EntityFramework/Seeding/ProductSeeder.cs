using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using EzCatalog.Domain.Products;
using EzCatalog.Infrastructure.EntityFramework.Models;
using Microsoft.EntityFrameworkCore;

namespace EzCatalog.Infrastructure.EntityFramework.Seeding;

[SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Generates dummy data for testing.")]
public class ProductSeeder
{
    private static readonly ProductTemplate[] Templates = new[]
    {
        new ProductTemplate(
            "LAP",
            "Laptop",
            new[] { "Dell", "Lenovo", "HP", "Asus", "Acer" },
            new[] { "Pro 14", "Air 13", "Vivo 15", "Zen 16" },
            new[] { "8GB/256GB", "16GB/512GB", "32GB/1TB" },
            1999,
            9999),
        new ProductTemplate(
            "TV",
            "TV",
            new[] { "Samsung", "LG", "Sony", "Philips" },
            new[] { "LED 43\"", "QLED 55\"", "OLED 65\"", "Mini LED 75\"" },
            new[] { "Full HD", "4K Smart", "8K Smart" },
            999,
            14999),
        new ProductTemplate(
            "PHN",
            "Smartphone",
            new[] { "Samsung", "Xiaomi", "Motorola", "Nokia" },
            new[] { "Lite 5G", "Plus 5G", "Ultra 5G" },
            new[] { "128GB Black", "256GB Silver", "512GB Blue" },
            699,
            6999),
        new ProductTemplate(
            "HDP",
            "Headphones",
            new[] { "Sony", "JBL", "Bose", "Sennheiser" },
            new[] { "Wireless ANC", "True Wireless", "Over-Ear" },
            new[] { "Black", "White", "Navy" },
            99,
            1999),
        new ProductTemplate(
            "FRG",
            "Fridge",
            new[] { "Bosch", "Beko", "Electrolux", "Whirlpool" },
            new[] { "Combi 186cm", "No Frost 201cm", "Side-by-Side" },
            new[] { "Inox", "White", "Black" },
            1499,
            7999),
        new ProductTemplate(
            "CFM",
            "Coffee Machine",
            new[] { "De'Longhi", "Philips", "Siemens", "Krups" },
            new[] { "Automatic Espresso", "Capsule", "Drip" },
            new[] { "Black", "Silver" },
            199,
            4999),
    };

    private static readonly Currency[] Currencies = Enum.GetValues<Currency>();

    private readonly CatalogDbContext dbContext;
    private readonly IGuidProvider guidProvider;

    public ProductSeeder(CatalogDbContext dbContext, IGuidProvider guidProvider)
    {
        this.dbContext = dbContext;
        this.guidProvider = guidProvider;
    }

    public async Task SeedAsync(int productCount, CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(productCount);

        var usedSkus = await dbContext.Products
            .Select(product => product.Sku)
            .ToHashSetAsync(cancellationToken);

        var products = Enumerable.Range(0, productCount)
            .Select(_ => ToDbModel(CreateProduct(usedSkus)))
            .ToList();

        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateUniqueSku(ProductTemplate template, string brand, HashSet<string> usedSkus)
    {
        var brandCode = new string(brand.Where(char.IsAsciiLetterOrDigit).Take(3).ToArray()).ToUpperInvariant();

        string sku;
        do
        {
            sku = $"{template.SkuPrefix}-{brandCode}-{Random.Shared.Next(0, 1_000_000):D6}";
        }
        while (!usedSkus.Add(sku));

        return sku;
    }

    private static ProductDbModel ToDbModel(Product product) => new ProductDbModel
    {
        Id = product.Id.Value,
        Sku = product.Sku.Value,
        Name = product.Name.Value,
        PriceAmount = product.Price.Amount,
        PriceCurrency = product.Price.Currency.ToString(),
    };

    private static T Pick<T>(IReadOnlyList<T> items) => items[Random.Shared.Next(items.Count)];

    private Product CreateProduct(HashSet<string> usedSkus)
    {
        var template = Pick(Templates);
        var brand = Pick(template.Brands);

        var id = ProductId.Create(guidProvider.GetNewGuid());
        var sku = Sku.Create(GenerateUniqueSku(template, brand, usedSkus));
        var name = ProductName.Create($"{brand} {template.Category} {Pick(template.Models)} {Pick(template.Variants)}");
        var price = new Money(Random.Shared.Next(template.MinPrice, template.MaxPrice + 1) - 0.01m, Pick(Currencies));

        return Product.New(id, sku, name, price);
    }
}
