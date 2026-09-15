using EzCatalog.WebAPI.Hypermedia;

namespace EzCatalog.WebAPI.DTOs;

public record ProductLinks(
    Link Self,
    Link Update)
{
}
