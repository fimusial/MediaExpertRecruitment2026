using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using MediatR;

namespace EzCatalog.Application.RequestPipeline;

public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICommand
{
    private readonly IUnitOfWork unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        this.unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (unitOfWork.HasOngoingTransaction)
        {
            return await next(cancellationToken);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        var response = await next(cancellationToken);
        await unitOfWork.CommitTransactionAsync(cancellationToken);

        return response;
    }
}
