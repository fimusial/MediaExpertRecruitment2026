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

public class AddProductCommandHandler : IRequestHandler<AddProductCommand, Unit>
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

    public async Task<Unit> Handle(AddProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogHandlerRunning(nameof(AddProductCommandHandler));

        var productId = ProductId.Create(guidProvider.GetNewGuid());
        var sku = Sku.Create(command.Sku);
        var name = ProductName.Create(command.Name);
        var price = new Money(command.PriceAmount, Enum.Parse<Currency>(command.PriceCurrency));

        var product = new Product(productId, sku, name, price);
        await repository.AddAsync(product, cancellationToken);

        await mediator.DispatchDomainEventsAsync(product, cancellationToken);
        return Unit.Value;
    }
}
