namespace EzCatalog.Domain.Products.DomainEvents;

public record ProductPriceChanged : ProductEvent
{
    public ProductPriceChanged(ProductId productId)
        : base(productId)
    {
    }
}
