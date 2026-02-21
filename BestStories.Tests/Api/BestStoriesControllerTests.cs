using BestStories.Api.Controllers;
using BestStories.Application.Interfaces;
using BestStories.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace BestStories.Tests.Api;

public class BestStoriesControllerTests
{
    private readonly Mock<IGetBestStoriesUseCase> _useCaseMock;
    private readonly BestStoriesController _controller;

    public BestStoriesControllerTests()
    {
        _useCaseMock = new Mock<IGetBestStoriesUseCase>();
        _controller = new BestStoriesController(_useCaseMock.Object);
    }

    [Fact]
    public async Task Get_WithValidN_ShouldReturnOkWithStories()
    {
        var stories = new[]
        {
            Story.Create("Story 1", "https://a.com", "user1", 1571922181, 200, 10),
            Story.Create("Story 2", "https://b.com", "user2", 1571922181, 100, 5),
        };

        _useCaseMock.Setup(u => u.ExecuteAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stories);

        var result = await _controller.Get(2, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStories = Assert.IsAssignableFrom<IEnumerable<Story>>(okResult.Value);
        Assert.Equal(2, returnedStories.Count());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Get_WithInvalidN_ShouldReturnBadRequestWithProblemDetails(int n)
    {
        var result = await _controller.Get(n, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
        Assert.IsType<ProblemDetails>(objectResult.Value);
    }

    [Fact]
    public async Task Get_ShouldCallUseCaseWithCorrectCount()
    {
        _useCaseMock.Setup(u => u.ExecuteAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _controller.Get(5, CancellationToken.None);

        _useCaseMock.Verify(u => u.ExecuteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
