using System;
using EzCatalog.Application.Ports;

namespace EzCatalog.Infrastructure.Adapters;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetUtcNow() => DateTime.UtcNow;
}
