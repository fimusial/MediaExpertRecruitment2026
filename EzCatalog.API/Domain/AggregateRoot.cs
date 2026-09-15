using System.Collections.Generic;

namespace EzCatalog.Domain;

public abstract class AggregateRoot
{
    protected IList<IDomainEvent> DomainEvents { get; } = new List<IDomainEvent>();

    public IReadOnlyCollection<IDomainEvent> GetPublishedDomainEvents() => DomainEvents.AsReadOnly();
}
