using System.Net;
using System.Net.Http.Json;
using BestStories.Application.DTOs;
using BestStories.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BestStories.Tests.Integration;

public class BestStoriesIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BestStoriesIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClientWithMockedHackerNews(IEnumerable<HackerNewsItemResponse> items)
    {
        var mockClient = new Mock<IHackerNewsClient>();
        mockClient.Setup(c => c.GetBestStoryItemsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IHackerNewsClient));
                if (descriptor != null) services.Remove(descriptor);

                services.AddSingleton(mockClient.Object);
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetBestStories_ReturnsStoriesSortedByScoreDescending()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Low", "https://a.com", "user1", 1571922181, 50, 10),
            new HackerNewsItemResponse("High", "https://b.com", "user2", 1571922181, 200, 20),
            new HackerNewsItemResponse("Mid", "https://c.com", "user3", 1571922181, 100, 15),
        };

        var client = CreateClientWithMockedHackerNews(items);

        var response = await client.GetAsync("/api/beststories?n=3");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var stories = await response.Content.ReadFromJsonAsync<List<StoryResponse>>();
        Assert.NotNull(stories);
        Assert.Equal(3, stories.Count);
        Assert.Equal("High", stories[0].Title);
        Assert.Equal("Mid", stories[1].Title);
        Assert.Equal("Low", stories[2].Title);
    }

    [Fact]
    public async Task GetBestStories_ReturnsCorrectJsonFields()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Test Story", "https://example.com", "testuser", 1571922181, 150, 42),
        };

        var client = CreateClientWithMockedHackerNews(items);

        var response = await client.GetAsync("/api/beststories?n=1");
        var stories = await response.Content.ReadFromJsonAsync<List<StoryResponse>>();

        Assert.NotNull(stories);
        var story = stories.First();
        Assert.Equal("Test Story", story.Title);
        Assert.Equal("https://example.com", story.Uri);
        Assert.Equal("testuser", story.PostedBy);
        Assert.Equal(150, story.Score);
        Assert.Equal(42, story.CommentCount);
        Assert.Contains("2019-10-24", story.Time);
    }

    [Fact]
    public async Task GetBestStories_LimitsResultsToN()
    {
        var items = new[]
        {
            new HackerNewsItemResponse("Story 1", "https://a.com", "user1", 1571922181, 300, 10),
            new HackerNewsItemResponse("Story 2", "https://b.com", "user2", 1571922181, 200, 20),
            new HackerNewsItemResponse("Story 3", "https://c.com", "user3", 1571922181, 100, 15),
        };

        var client = CreateClientWithMockedHackerNews(items);

        var response = await client.GetAsync("/api/beststories?n=1");
        var stories = await response.Content.ReadFromJsonAsync<List<StoryResponse>>();

        Assert.NotNull(stories);
        Assert.Single(stories);
        Assert.Equal("Story 1", stories[0].Title);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetBestStories_WithInvalidN_ReturnsBadRequest(int n)
    {
        var client = CreateClientWithMockedHackerNews([]);

        var response = await client.GetAsync($"/api/beststories?n={n}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // DTO to deserialize the API JSON response
    private record StoryResponse(string Title, string Uri, string PostedBy, string Time, int Score, int CommentCount);
}
