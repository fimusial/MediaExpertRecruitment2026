using System;
using MediatR;

namespace EzCatalog.Application.Queries;

public record GetProductsPageQuery(Guid? Cursor, int Limit)
    : IRequest<ProductsPageResult>
{
    public const int DefaultLimit = 20;
}
