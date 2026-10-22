using Microsoft.AspNetCore.Diagnostics;

namespace UnitConversionApi.Api;

/// <summary>Converts unexpected exceptions into a sanitized 500 ProblemDetails response.</summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public const string ErrorCode = "Server.UnexpectedError";

    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        var problem = ResultExtensions.ToProblemDetails(
            ErrorCode, "An unexpected error occurred.", StatusCodes.Status500InternalServerError, "Internal Server Error");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
