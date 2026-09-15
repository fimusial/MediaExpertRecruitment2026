using System.Collections.Generic;
using MediatR;

namespace EzCatalog.Application.Queries;

public record GetProductsPageQuery(string Cursor, int Limit)
    : IRequest<IEnumerable<ProductResult>>
{
}
