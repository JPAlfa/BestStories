using BestStories.Application.Interfaces;
using BestStories.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace BestStories.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetBestStoriesUseCase, GetBestStoriesUseCase>();

        return services;
    }
}
