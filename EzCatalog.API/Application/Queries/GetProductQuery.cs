using System;
using MediatR;

namespace EzCatalog.Application.Queries;

public record GetProductQuery(Guid Id)
    : IRequest<ProductResult>
{
}
