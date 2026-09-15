using EzCatalog.Application.Ports;
using EzCatalog.Infrastructure.Adapters;
using EzCatalog.Infrastructure.EntityFramework;
using EzCatalog.Infrastructure.EntityFramework.Adapters;
using EzCatalog.Infrastructure.EntityFramework.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace EzCatalog.Infrastructure;

public static class ServiceCollectionBuilder
{
    public const string InMemoryDatabaseName = "EzCatalog";
    public const int InMemoryDatabaseSeedCount = 1000;

    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddDbContext()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddSingleton<IGuidProvider, GuidProvider>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IProductRepository, ProductRepository>()
            .AddScoped<IProductQueryService, ProductQueryService>();

        return serviceCollection;
    }

    public static IServiceCollection AddDbContext(this IServiceCollection serviceCollection)
    {
        // in-memory configuration, definitely not for production use!
        serviceCollection.AddDbContext<CatalogDbContext>(
            options =>
            {
                options
                    .UseAsyncSeeding((catalogDbContext, _, cancellationToken) =>
                    {
                        return new ProductSeeder((CatalogDbContext)catalogDbContext, new GuidProvider())
                            .SeedAsync(InMemoryDatabaseSeedCount, cancellationToken);
                    })
                    .UseInMemoryDatabase(InMemoryDatabaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

        return serviceCollection;
    }
}
