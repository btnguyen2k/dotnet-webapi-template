using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Dwt.Api.Helpers;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, exception.Message);

        var code = HttpStatusCode.InternalServerError;
        var result = new Dictionary<string, object>
        {
            { "status", code },
            { "message", exception.Message }
        };

        if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
        }
        else if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
        }
        else if (exception is NotImplementedException)
        {
            code = HttpStatusCode.NotImplemented;
        }

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)code;
        result["status"] = (int)code;
        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(result), cancellationToken: cancellationToken);
        return true;
    }
}

