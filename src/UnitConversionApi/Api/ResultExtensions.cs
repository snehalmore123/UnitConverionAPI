using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Common;

namespace UnitConversionApi.Api;

public static class ResultExtensions
{
    public const string ErrorCodeExtension = "errorCode";

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static ProblemDetails ToProblemDetails(string code, string detail, int status, string title)
    {
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail };
        problem.Extensions[ErrorCodeExtension] = code;
        return problem;
    }

    /// <summary>Maps a failed result to a ProblemDetails response; a successful result to 200 OK with the value.</summary>
    public static IActionResult ToActionResult<T>(this Result<T> result) =>
        result.IsSuccess ? new OkObjectResult(result.Value) : result.Error.ToActionResult();

    public static IActionResult ToActionResult(this Error error)
    {
        var status = error.Type.ToStatusCode();
        return new ObjectResult(ToProblemDetails(error.Code, error.Message, status, ReasonTitle(error.Type)))
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static string ReasonTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "Bad Request",
        ErrorType.NotFound => "Not Found",
        ErrorType.Conflict => "Conflict",
        _ => "Internal Server Error"
    };
}
