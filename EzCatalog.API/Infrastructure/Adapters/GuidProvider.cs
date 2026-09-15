using System;
using EzCatalog.Application.Ports;
using SequentialGuid;

namespace EzCatalog.Infrastructure.Adapters;

public class GuidProvider : IGuidProvider
{
    public Guid GetNewGuid() => GuidV7.NewGuid();
}
