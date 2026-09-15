namespace EzCatalog.WebAPI.DTOs;

public record UpdateProductRequest(
    string? Name = null,
    decimal? PriceAmount = null,
    string? PriceCurrency = null)
{
}
