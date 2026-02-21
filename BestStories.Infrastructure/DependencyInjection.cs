using BestStories.Application.Interfaces;
using BestStories.Application.UseCases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BestStories.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddHttpClient<IHackerNewsClient, HackerNewsClient>(client =>
        {
            var baseUrl = configuration.GetValue("HackerNews:BaseUrl", "https://hacker-news.firebaseio.com/v0/");
            client.BaseAddress = new Uri(baseUrl!);
        });

        services.AddScoped<GetBestStoriesUseCase>();

        return services;
    }
}
