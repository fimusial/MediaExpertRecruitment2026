using System;
using System.Text.Json.Serialization;

namespace EzCatalog.WebAPI.DTOs;

public record ProductResponse(
    Guid Id,
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency,
    [property: JsonPropertyName("_links")] ProductLinks Links)
{
}
