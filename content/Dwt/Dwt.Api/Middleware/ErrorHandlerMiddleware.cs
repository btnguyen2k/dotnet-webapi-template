using System.Net;
using System.Text.Json;

namespace Dwt.Api.Middleware;

/// <summary>
/// Middleware that handles exceptions and returns corresponding JSON responses.
/// </summary>
/// <remarks>
///     Sample usage:
///         app.UseMiddleware&lt;ErrorHandlerMiddleware&gt;();
/// </remarks>
public class ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger, RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    protected async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        logger.LogError(ex, ex.Message);

        var code = HttpStatusCode.InternalServerError;
        var result = new Dictionary<string, object>
        {
            { "status", code },
            { "message", ex.Message }
        };

        if (ex is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
        }
        else if (ex is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
        }
        else if (ex is NotImplementedException)
        {
            code = HttpStatusCode.NotImplemented;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;
        result["status"] = (int)code;
        await context.Response.WriteAsync(JsonSerializer.Serialize(result));
    }
}
