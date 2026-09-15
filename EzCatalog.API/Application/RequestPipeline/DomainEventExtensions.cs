using System;
using EzCatalog.Domain;
using MediatR;

namespace EzCatalog.Application.RequestPipeline;

public static class EventExtensions
{
    public static INotification ToNotification(this IDomainEvent domainEvent)
    {
        INotification? notification = null;

#pragma warning disable CA1031 // Do not catch general exception types
        try
        {
            var notificationGenericType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            notification = (INotification)Activator.CreateInstance(notificationGenericType, domainEvent)!;
        }
        catch (Exception exception)
        {
            ThrowCouldNotInstantiateNotification(domainEvent, exception);
        }
#pragma warning restore CA1031 // Do not catch general exception types

        if (notification is null)
        {
            ThrowCouldNotInstantiateNotification(domainEvent);
        }

        return notification!;
    }

    private static void ThrowCouldNotInstantiateNotification(object? @event, Exception? exception = null)
    {
        var receivedEventTypeName = @event?.GetType().FullName ?? "null";

        throw new InvalidOperationException(
            $"could not instantiate notification for event: {receivedEventTypeName}",
            exception);
    }
}
