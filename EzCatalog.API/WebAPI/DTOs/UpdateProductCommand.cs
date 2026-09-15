namespace EzCatalog.WebAPI.DTOs;

public record UpdateProductCommand(
    string? Name,
    decimal? PriceAmount,
    string? PriceCurrency)
{
}
