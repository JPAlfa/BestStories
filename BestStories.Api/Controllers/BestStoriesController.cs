using BestStories.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BestStoriesController : ControllerBase
{
    private readonly IGetBestStoriesUseCase _useCase;

    public BestStoriesController(IGetBestStoriesUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int n, CancellationToken cancellationToken)
    {
        if (n <= 0)
            return BadRequest(new { error = "Parameter 'n' must be greater than zero." });

        var stories = await _useCase.ExecuteAsync(n, cancellationToken);

        return Ok(stories);
    }
}
