using EzCatalog.Domain;
using MediatR;

namespace EzCatalog.Application.RequestPipeline;

public record DomainEventNotification<T>(T DomainEvent) : INotification
    where T : IDomainEvent
{
}
