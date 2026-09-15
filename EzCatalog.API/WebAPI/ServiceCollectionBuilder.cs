using Microsoft.Extensions.DependencyInjection;

namespace EzCatalog.WebAPI;

public static class ServiceCollectionBuilder
{
    public static IServiceCollection AddWebAPI(this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }
}
