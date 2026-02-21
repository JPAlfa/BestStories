using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BestStories.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled by the client.");
            httpContext.Response.StatusCode = 499;
            return true;
        }

        _logger.LogError(exception, "An unhandled exception occurred.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred. Please try again later.",
            Status = StatusCodes.Status500InternalServerError
        }, cancellationToken);

        return true;
    }
}
