using System;

namespace EzCatalog.Application.Ports;

public interface IGuidProvider
{
    Guid GetNewGuid();
}
