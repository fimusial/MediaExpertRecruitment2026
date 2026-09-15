namespace EzCatalog.WebAPI.DTOs;

public record UpdateProductCommand(
    string? Name = null,
    decimal? PriceAmount = null,
    string? PriceCurrency = null)
{
}
