using System;

namespace EzCatalog.Domain.Products.DomainEvents;

public abstract record ProductEvent : IDomainEvent
{
    protected ProductEvent(ProductId id)
    {
        EntityId = id.Value;
    }

    public string EntityType { get; } = nameof(Product);

    public Guid EntityId { get; }
}
