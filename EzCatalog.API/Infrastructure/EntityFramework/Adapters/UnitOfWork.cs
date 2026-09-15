using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Ports;
using EzCatalog.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace EzCatalog.Infrastructure.EntityFramework.Adapters;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly ILogger<UnitOfWork> logger;
    private readonly CatalogDbContext catalogDbContext;
    private IDbContextTransaction? currentTransaction;

    public UnitOfWork(
        ILogger<UnitOfWork> logger,
        CatalogDbContext catalogDbContext)
    {
        this.logger = logger;
        this.catalogDbContext = catalogDbContext;
    }

    public bool HasOngoingTransaction => currentTransaction is not null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (currentTransaction is not null)
        {
            throw new InvalidOperationException("This unit of work has already initiated a transaction.");
        }

        currentTransaction = await catalogDbContext.Database.BeginTransactionAsync(cancellationToken);

        if (currentTransaction is null)
        {
            throw new InvalidOperationException("Could not begin transaction.");
        }

        var logTransactionId = currentTransaction.TransactionId;
        logger.LogUnitOfWorkStep(nameof(BeginTransactionAsync), logTransactionId);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (currentTransaction is null)
        {
            throw new InvalidOperationException("Cannot commit; a transaction has not been initiated.");
        }

        await catalogDbContext.SaveChangesAsync(cancellationToken);
        await currentTransaction.CommitAsync(cancellationToken);

        var logTransactionId = currentTransaction.TransactionId;
        logger.LogUnitOfWorkStep(nameof(CommitTransactionAsync), logTransactionId);

        currentTransaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (currentTransaction is not null)
        {
            await currentTransaction.DisposeAsync();
        }

        var logTransactionId = currentTransaction?.TransactionId;
        logger.LogUnitOfWorkStep(nameof(DisposeAsync), logTransactionId);
    }
}
