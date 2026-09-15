using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Logging;
using EzCatalog.Application.Ports;
using EzCatalog.Application.RequestPipeline;
using EzCatalog.Domain.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EzCatalog.Application.Commands;

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Guid>
{
    private readonly ILogger<AddProductCommandHandler> logger;
    private readonly IMediator mediator;
    private readonly IProductRepository repository;
    private readonly IGuidProvider guidProvider;

    public AddProductCommandHandler(
        ILogger<AddProductCommandHandler> logger,
        IMediator mediator,
        IProductRepository repository,
        IGuidProvider guidProvider)
    {
        this.logger = logger;
        this.mediator = mediator;
        this.repository = repository;
        this.guidProvider = guidProvider;
    }

    public async Task<Guid> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogHandlerRunning(nameof(AddProductCommandHandler));

        var product = Product.Create(
            guidProvider.GetNewGuid(),
            command.Sku,
            command.Name,
            command.PriceAmount,
            command.PriceCurrency);

        var id = await repository.AddAsync(product, cancellationToken);
        await mediator.DispatchDomainEventsAsync(product, cancellationToken);
        return id;
    }
}
