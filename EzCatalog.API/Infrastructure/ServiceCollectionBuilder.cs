using EzCatalog.Application.Ports;
using EzCatalog.Infrastructure.Adapters;
using Microsoft.Extensions.DependencyInjection;

namespace EzCatalog.Infrastructure;

public static class ServiceCollectionBuilder
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IGuidProvider, GuidProvider>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProductQueryService, ProductQueryService>();

        return serviceCollection;
    }
}
