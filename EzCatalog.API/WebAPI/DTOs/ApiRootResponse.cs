using System.Text.Json.Serialization;

namespace EzCatalog.WebAPI.DTOs;

public record ApiRootResponse(
    [property: JsonPropertyName("_links")] ApiRootLinks Links)
{
}
