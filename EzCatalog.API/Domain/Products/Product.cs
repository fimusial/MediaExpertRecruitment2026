using System;
using EzCatalog.Domain.Products.DomainEvents;
using EzCatalog.Domain.Products.Exceptions;

namespace EzCatalog.Domain.Products;

public class Product : AggregateRoot
{
    public Product(ProductId id, Sku sku, ProductName productName, Money price)
    {
        ThrowIfPriceInvalid(price);

        Id = id;
        Sku = sku;
        Name = productName;
        Price = price;

        DomainEvents.Add(new ProductCreated(Id));
    }

    public ProductId Id { get; }

    public Sku Sku { get; }

    public ProductName Name { get; private set; }

    public Money Price { get; private set; }

    public static Product Create(Guid id, string sku, string name, decimal priceAmount, string priceCurrency)
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
    }
}
