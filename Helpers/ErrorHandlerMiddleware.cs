using System.Net;
using System.Text.Json;

namespace dwt.Helpers;

/// <summary>
/// Middleware that handles exceptions and returns a JSON response.
/// </summary>
public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger, RequestDelegate next)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    protected async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, ex.Message);

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
