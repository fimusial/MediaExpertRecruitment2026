namespace EzCatalog.Domain.Products.DomainEvents;

public record ProductNameChanged : ProductEvent
{
    public ProductNameChanged(ProductId productId)
        : base(productId)
    {
    }
}
