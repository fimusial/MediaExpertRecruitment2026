using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Domain;
using MediatR;

namespace EzCatalog.Application.RequestPipeline;

public static class DomainEventNotificationDispatcher
{
    public static async Task DispatchDomainEventsAsync(this IMediator mediator, IEnumerable<AggregateRoot> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            await mediator.DispatchDomainEventsAsync(entity, cancellationToken);
        }
    }

    public static async Task DispatchDomainEventsAsync(this IMediator mediator, AggregateRoot entity, CancellationToken cancellationToken)
    {
        foreach (var @event in entity.GetPublishedDomainEvents())
        {
            await mediator.Publish(@event.ToNotification(), cancellationToken);
        }
    }
}
