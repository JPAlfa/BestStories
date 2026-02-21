using BestStories.Application.DTOs;
using BestStories.Application.Interfaces;
using BestStories.Application.UseCases;
using Moq;

namespace BestStories.Tests.Application;

public class GetBestStoriesUseCaseTests
{
    private readonly Mock<IHackerNewsClient> _clientMock;
    private readonly GetBestStoriesUseCase _useCase;

    public GetBestStoriesUseCaseTests()
    {
        _clientMock = new Mock<IHackerNewsClient>();
        _useCase = new GetBestStoriesUseCase(_clientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStoriesSortedByScoreDescending()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Low Score", "https://a.com", "user1", 1571922181, 50, 10),
            new HackerNewsItemResponse("High Score", "https://b.com", "user2", 1571922181, 200, 20),
            new HackerNewsItemResponse("Mid Score", "https://c.com", "user3", 1571922181, 100, 15),
        };

        _clientMock.Setup(c => c.GetBestStoryItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = (await _useCase.ExecuteAsync(3)).ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal("High Score", result[0].Title);
        Assert.Equal("Mid Score", result[1].Title);
        Assert.Equal("Low Score", result[2].Title);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLimitResultsToN()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Story 1", "https://a.com", "user1", 1571922181, 300, 10),
            new HackerNewsItemResponse("Story 2", "https://b.com", "user2", 1571922181, 200, 20),
            new HackerNewsItemResponse("Story 3", "https://c.com", "user3", 1571922181, 100, 15),
        };

        _clientMock.Setup(c => c.GetBestStoryItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = (await _useCase.ExecuteAsync(2)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldMapFieldsCorrectly()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Test Title", "https://example.com", "testuser", 1571922181, 150, 42),
        };

        _clientMock.Setup(c => c.GetBestStoryItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        var result = (await _useCase.ExecuteAsync(1)).First();

        Assert.Equal("Test Title", result.Title);
        Assert.Equal("https://example.com", result.Uri);
        Assert.Equal("testuser", result.PostedBy);
        Assert.Equal(150, result.Score);
        Assert.Equal(42, result.CommentCount);
        Assert.Contains("2019-10-24", result.Time);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ExecuteAsync_WithInvalidCount_ShouldThrow(int count)
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _useCase.ExecuteAsync(count));
    }
}
