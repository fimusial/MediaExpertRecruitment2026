using System;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.Domain.Products.Exceptions;

namespace EzCatalog.Domain.Products;

public class Product : AggregateRoot
{
    public const int PriceMaxDecimalPlaces = 2;

    public const decimal PriceMaxAmount = 9_999_999_999_999_999.99m;

    private Product(ProductId id, Sku sku, ProductName name, Money price)
    {
        ThrowIfPriceInvalid(price);

        Id = id;
        Sku = sku;
        Name = name;
        Price = price;
    }

    public ProductId Id { get; }

    public Sku Sku { get; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public static Product New(ProductId id, Sku sku, ProductName name, Money price)
    {
        var product = new Product(id, sku, name, price);
        product.DomainEvents.Add(new ProductCreated(id));
        return product;
    }

    public static Product Rehydrate(Guid id, string sku, string name, decimal priceAmount, string priceCurrency)
    {
        return new Product(
                ProductId.Create(id),
                Sku.Create(sku),
                ProductName.Create(name),
                new Money(priceAmount, Enum.Parse<Currency>(priceCurrency)));
    }

    public void UpdateName(ProductName newName)
    {
        Name = newName;
        DomainEvents.Add(new ProductNameChanged(Id));
    }

    public void UpdatePrice(Money newPrice)
    {
        ThrowIfPriceInvalid(newPrice);

        Price = newPrice;
        DomainEvents.Add(new ProductPriceChanged(Id));
    }

    public override string ToString()
    {
        return $"{nameof(Product)} {Id}";
    }

    private static void ThrowIfPriceInvalid(Money price)
    {
        if (price.Amount <= 0.0m)
        {
            throw new InvalidPriceException($"Invalid Product price: {price}. It must be positive.");
        }

        if (price.Amount > PriceMaxAmount)
        {
            throw new InvalidPriceException($"Invalid Product price: {price}. It must not exceed {PriceMaxAmount}.");
        }

        if (decimal.Round(price.Amount, PriceMaxDecimalPlaces) != price.Amount)
        {
            throw new InvalidPriceException(
                $"Invalid Product price: {price}. It must have at most {PriceMaxDecimalPlaces} decimal places.");
        }
    }
}
