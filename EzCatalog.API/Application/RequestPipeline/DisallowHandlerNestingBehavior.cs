using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EzCatalog.Application.RequestPipeline;

public class DisallowHandlerNestingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private bool Nested { get; set; }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (Nested)
        {
            throw new InvalidOperationException("Nesting handlers is not allowed.");
        }

        Nested = true;

        try
        {
            return await next(cancellationToken);
        }
        finally
        {
            Nested = false;
        }
    }
}
