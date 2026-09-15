namespace EzCatalog.WebAPI.DTOs;

public record AddProductRequest(
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency)
{
}
