using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using MediatR;

namespace EzCatalog.Application.Queries;

public class GetProductsPageQueryHandler : IRequestHandler<GetProductsPageQuery, ProductsPageResult>
{
    private readonly IProductQueryService queryService;

    public GetProductsPageQueryHandler(IProductQueryService queryService)
    {
        this.queryService = queryService;
    }

    public Task<ProductsPageResult> Handle(GetProductsPageQuery query, CancellationToken cancellationToken)
    {
        return queryService.GetProductsPageAsync(query.Cursor, query.Limit, cancellationToken);
    }
}
