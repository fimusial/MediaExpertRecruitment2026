using EzCatalog.Application.RequestPipeline;
using MediatR;

namespace EzCatalog.Application.Commands;

public record AddProductCommand(
    string Sku,
    string Name,
    decimal PriceAmount,
    string PriceCurrency)
    : IRequest<Unit>, ICommand
{
}
