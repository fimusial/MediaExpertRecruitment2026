using EzCatalog.WebAPI.Hypermedia;

namespace EzCatalog.WebAPI.DTOs;

public record ApiRootLinks(
    Link Self,
    Link Products)
{
}
