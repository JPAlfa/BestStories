using BestStories.Domain;

namespace BestStories.Application.Interfaces;

public interface IGetBestStoriesUseCase
{
    Task<IEnumerable<Story>> ExecuteAsync(int count, CancellationToken cancellationToken = default);
}
