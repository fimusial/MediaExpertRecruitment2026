using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;

namespace EzCatalog.Infrastructure.Adapters;

public class UnitOfWork : IUnitOfWork
{
    public bool HasOngoingTransaction { get; private set; }

    public Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        // TODO: Entity Framework Transaction
        HasOngoingTransaction = true;
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        // TODO: Entity Framework Transaction
        HasOngoingTransaction = false;
        return Task.CompletedTask;
    }
}
