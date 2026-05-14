using Microsoft.AspNetCore.Diagnostics;
using MusicApp.Web.Models;

namespace MusicApp.Web.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext         httpContext,
        Exception           exception,
        CancellationToken   cancellationToken)
    {
        _logger.LogError(exception,
            "Unhandled exception on {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var statusCode = exception switch
        {
            ArgumentException            => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException  => StatusCodes.Status401Unauthorized,
            KeyNotFoundException         => StatusCodes.Status404NotFound,
            _                            => StatusCodes.Status500InternalServerError
        };

        httpContext.Response.StatusCode  = statusCode;
        httpContext.Response.ContentType = "application/json";

        var detail = statusCode == 500
            ? "An unexpected error occurred. Please try again later."
            : exception.Message;

        await httpContext.Response.WriteAsJsonAsync(
            new ApiError("INTERNAL_ERROR", detail),
            cancellationToken);

        return true;
    }
}
