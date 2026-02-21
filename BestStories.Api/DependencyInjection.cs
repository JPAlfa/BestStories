using BestStories.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace BestStories.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}
