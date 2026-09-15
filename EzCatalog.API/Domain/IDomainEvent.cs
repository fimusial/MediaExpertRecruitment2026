using System;

namespace EzCatalog.Domain;

public interface IDomainEvent
{
    string EntityType { get; }

    Guid EntityId { get; }
}
