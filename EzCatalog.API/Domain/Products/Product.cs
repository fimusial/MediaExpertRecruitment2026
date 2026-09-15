using EzCatalog.Domain.Products.DomainEvents;

namespace EzCatalog.Domain.Products;

public class Product : AggregateRoot
{
    public Product(ProductId id, Sku sku, ProductName productName, Money price)
    {
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

    public void UpdateName(ProductName newName)
    {
        Name = newName;
        DomainEvents.Add(new ProductNameChanged(Id));
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice;
        DomainEvents.Add(new ProductPriceChanged(Id));
    }
}
