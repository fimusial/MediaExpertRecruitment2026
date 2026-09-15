using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using MediatR;

namespace EzCatalog.Application.Queries;

public class GetProductsPageQueryHandler : IRequestHandler<GetProductsPageQuery, IEnumerable<ProductResult>>
{
    private readonly IProductQueryService queryService;

    public GetProductsPageQueryHandler(IProductQueryService queryService)
    {
        this.queryService = queryService;
    }

    public async Task<IEnumerable<ProductResult>> Handle(GetProductsPageQuery query, CancellationToken cancellationToken)
    {
        throw new NotImplementedException(nameof(GetProductsPageQueryHandler));
    }
}
