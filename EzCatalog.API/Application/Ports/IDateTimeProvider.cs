using System;

namespace EzCatalog.Application.Ports;

public interface IDateTimeProvider
{
    DateTime GetUtcNow();
}
