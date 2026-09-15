using System.ComponentModel;
using EzCatalog.WebAPI.Hypermedia;

namespace EzCatalog.WebAPI.DTOs;

public record ProductsPageLinks(
    Link Self,
    Link First,
    [property: Description("Null when there is no next page.")] Link? Next,
    Link Create)
{
}
