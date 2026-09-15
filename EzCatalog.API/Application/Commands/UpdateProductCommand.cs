using System;
using EzCatalog.Application.RequestPipeline;
using MediatR;

namespace EzCatalog.Application.Commands;

public record UpdateProductCommand(
    Guid Id,
    string? Name,
    decimal? PriceAmount,
    string? PriceCurrency)
: IRequest<Unit>, ICommand
{
}
