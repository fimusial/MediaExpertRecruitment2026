using System.Threading;
using System.Threading.Tasks;

namespace EzCatalog.Application.Ports;

public interface IUnitOfWork
{
    bool HasOngoingTransaction { get; }

    Task BeginTransactionAsync(CancellationToken cancellationToken);

    Task CommitTransactionAsync(CancellationToken cancellationToken);
}
