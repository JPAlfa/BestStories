using BestStories.Api.Models;
using BestStories.Application.Interfaces;
using BestStories.Domain;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BestStoriesController : ControllerBase
{
    private readonly IGetBestStoriesUseCase _useCase;

    public BestStoriesController(IGetBestStoriesUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// Retrieves the top N best stories from Hacker News, sorted by score descending.
    /// </summary>
    /// <param name="n">Number of stories to return. Must be greater than zero.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Array of stories sorted by score descending.</returns>
    /// <response code="200">Returns the top N stories.</response>
    /// <response code="400">If n is less than or equal to zero.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Story>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get([FromQuery] int n, CancellationToken cancellationToken)
    {
        if (n <= 0)
            return BadRequest(new ErrorResponse("Parameter 'n' must be greater than zero."));

        var stories = await _useCase.ExecuteAsync(n, cancellationToken);

        return Ok(stories);
    }
}
