using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EzCatalog.WebAPI;

public static class ServiceCollectionBuilder
{
    public static IServiceCollection AddWebAPI(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddProblemDetails()
            .Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

        return serviceCollection;
    }
}
