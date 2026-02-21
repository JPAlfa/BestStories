using System.Text.Json;
using BestStories.Application.DTOs;
using BestStories.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace BestStories.Infrastructure;

public class HackerNewsClient : IHackerNewsClient
{
    private const string CacheKey = "BestStories";
    private const string BestStoriesUrl = "beststories.json";
    private const string ItemUrl = "item/{0}.json";

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly int _cacheTtlMinutes;
    private readonly int _maxConcurrency;

    public HackerNewsClient(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cache = cache;
        _cacheTtlMinutes = configuration.GetValue("HackerNews:CacheTtlMinutes", 5);
        _maxConcurrency = configuration.GetValue("HackerNews:MaxConcurrency", 20);
    }

    public async Task<IEnumerable<HackerNewsItemResponse>> GetBestStoryItemsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKey, out IEnumerable<HackerNewsItemResponse>? cached) && cached is not null)
            return cached;

        var storyIds = await FetchBestStoryIdsAsync(cancellationToken);
        var items = await FetchStoryDetailsAsync(storyIds, cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_cacheTtlMinutes)
        };

        _cache.Set(CacheKey, items, cacheOptions);

        return items;
    }

    private async Task<IEnumerable<int>> FetchBestStoryIdsAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetStringAsync(BestStoriesUrl, cancellationToken);
        return JsonSerializer.Deserialize<IEnumerable<int>>(response) ?? [];
    }

    private async Task<IEnumerable<HackerNewsItemResponse>> FetchStoryDetailsAsync(
        IEnumerable<int> storyIds,
        CancellationToken cancellationToken)
    {
        using var semaphore = new SemaphoreSlim(_maxConcurrency);
        var tasks = storyIds.Select(async id =>
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                return await FetchSingleStoryAsync(id, cancellationToken);
            }
            finally
            {
                semaphore.Release();
            }
        });

        var results = await Task.WhenAll(tasks);
        return results.Where(item => item is not null)!;
    }

    private async Task<HackerNewsItemResponse?> FetchSingleStoryAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var url = string.Format(ItemUrl, id);
            var response = await _httpClient.GetStringAsync(url, cancellationToken);
            var item = JsonSerializer.Deserialize<HackerNewsItem>(response);

            if (item is null) return null;
            if (string.IsNullOrWhiteSpace(item.Title) || string.IsNullOrWhiteSpace(item.By)) return null;

            return new HackerNewsItemResponse(
                item.Title,
                item.Url ?? string.Empty,
                item.By,
                item.Time,
                item.Score,
                item.Descendants
            );
        }
        catch
        {
            return null;
        }
    }
}
