namespace EzCatalog.Domain.Products.DomainEvents;

public record ProductCreated : ProductEvent
{
    public ProductCreated(ProductId productId)
        : base(productId)
    {
    }
}
