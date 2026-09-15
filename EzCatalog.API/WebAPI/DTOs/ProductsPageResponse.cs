using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EzCatalog.WebAPI.DTOs;

public record ProductsPageResponse(
    int TotalCount,
    Guid? NextCursor,
    IEnumerable<ProductResponse> Products,
    [property: JsonPropertyName("_links")] ProductsPageLinks Links)
{
}
