using BestStories.Application.Interfaces;
using BestStories.Domain;

namespace BestStories.Application.UseCases;

public class GetBestStoriesUseCase : IGetBestStoriesUseCase
{
    private readonly IHackerNewsClient _client;

    public GetBestStoriesUseCase(IHackerNewsClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<Story>> ExecuteAsync(int count, CancellationToken cancellationToken = default)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero.");

        var items = await _client.GetBestStoryItemsAsync(cancellationToken);

        var stories = items
            .Select(item => Story.Create(
                item.Title,
                item.Url,
                item.By,
                item.Time,
                item.Score,
                item.Descendants))
            .OrderByDescending(s => s.Score)
            .Take(count);

        return stories;
    }
}
