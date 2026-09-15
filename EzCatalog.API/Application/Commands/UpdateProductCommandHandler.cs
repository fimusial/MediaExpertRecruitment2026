using System;
using System.Threading;
using System.Threading.Tasks;
using EzCatalog.Application.Exceptions;
using EzCatalog.Application.Logging;
using EzCatalog.Application.Ports;
using EzCatalog.Application.RequestPipeline;
using EzCatalog.Domain.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EzCatalog.Application.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Unit>
{
    private readonly ILogger<UpdateProductCommandHandler> logger;
    private readonly IMediator mediator;
    private readonly IProductRepository repository;

    public UpdateProductCommandHandler(
        ILogger<UpdateProductCommandHandler> logger,
        IMediator mediator,
        IProductRepository repository)
    {
        this.logger = logger;
        this.mediator = mediator;
        this.repository = repository;
    }

    public async Task<Unit> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        logger.LogHandlerRunning(nameof(AddProductCommandHandler));

        var product = await repository.GetAsync(ProductId.Create(command.Id), cancellationToken);
        if (product == null)
        {
            throw new NotFoundException($"{nameof(Product)} with Id {command.Id} was not found");
        }

        if (command.Name != null)
        {
            var newName = ProductName.Create(command.Name);
            product.UpdateName(newName);
        }

        if (command.PriceAmount != null && command.PriceCurrency != null)
        {
            var newPrice = new Money(command.PriceAmount.Value, Enum.Parse<Currency>(command.PriceCurrency));
            product.UpdatePrice(newPrice);
        }

        await repository.UpdateAsync(product, cancellationToken);

        await mediator.DispatchDomainEventsAsync(product, cancellationToken);
        return Unit.Value;
    }
}
