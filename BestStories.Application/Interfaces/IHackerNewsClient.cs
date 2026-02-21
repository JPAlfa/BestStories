using BestStories.Application.DTOs;

namespace BestStories.Application.Interfaces;

public interface IHackerNewsClient
{
    Task<IEnumerable<HackerNewsItemResponse>> GetBestStoryItemsAsync(CancellationToken cancellationToken = default);
}
