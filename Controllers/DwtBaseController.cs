using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;

namespace dwt.Controllers;

/// <summary>
/// Base controller for all controllers in the application.
/// </summary>
/// <remarks>
/// - application/json as the default content type.
/// - Utility methods to conveniently return common responses.
/// </remarks>
[ApiController]
[Consumes("application/json")]
public abstract class DwtBaseController : ControllerBase
{
    /// <summary>
    /// Generic response for "401 Unauthorized" errors.
    /// </summary>
    protected static readonly ObjectResult _respAuthenticationRequired = new NotFoundObjectResult(new
    {
        status = 401,
        message = "Authentication required."
    });

    /// <summary>
    /// Generic response for "403 Forbidden" errors.
    /// </summary>
    protected static readonly ObjectResult _respAccessDenied = new NotFoundObjectResult(new
    {
        status = 403,
        message = "Access denied."
    });

    /// <summary>
    /// Generic response for "404 Not Found" errors.
    /// </summary>
    protected static readonly ObjectResult _respNotFound = new NotFoundObjectResult(new
    {
        status = 404,
        message = "Not found."
    });

    /// <summary>
    /// Generic response for "200 OK".
    /// </summary>
    protected static readonly ObjectResult _respOk = new OkObjectResult(new
    {
        status = 200,
        message = "Ok."
    });

    /// <summary>
    /// Convenience method to return a 200 OK response.
    /// </summary>
    /// <returns></returns>
    protected static ObjectResult ResponseOk() => ResponseOk(null);

    /// <summary>
    /// Convenience method to return a 200 OK response.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected static ObjectResult ResponseOk(object? data) => data == null ? _respOk : new OkObjectResult(new
    {
        status = 200,
        message = "Ok.",
        data
    });

    /// <summary>
    /// Convenience method to return a non-200 response.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <returns></returns>
    protected static ObjectResult ResponseNotOk(int statusCode) => ResponseNotOk(statusCode, null);

    /// <summary>
    /// Convenience method to return a non-200 response.
    /// </summary>
    /// <param name="statusCode"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    protected static ObjectResult ResponseNotOk(int statusCode, string? message) => new ObjectResult(String.IsNullOrWhiteSpace(message) ? new
    {
        status = statusCode
    }
    : new
    {
        status = statusCode,
        message
    })
    {
        StatusCode = statusCode
    };

    /// <summary>
    /// Generic response for "503 Service Unavailable" errors.
    /// </summary>
    protected static readonly ObjectResult _respServiceUnavailable = new ObjectResult(new
    {
        status = 503,
        message = "Server is unavailable to handle the request."
    })
    {
        StatusCode = 503
    };

    /// <summary>
    /// Convenience method to return a 503 Service Unavailable response.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    protected static ObjectResult Response503(string? message) => String.IsNullOrWhiteSpace(message) ? _respServiceUnavailable : new(new
    {
        status = 503,
        message
    })
    {
        StatusCode = 503
    };

    /// <summary>
    /// Get the attached user-id from the http-context.
    /// </summary>
    /// <returns></returns>
    protected string? GetRequestUserId()
    {
        HttpContext.Items.TryGetValue(Global.HTTP_CTX_ITEM_USERID, out var userId);
        return userId?.ToString();
    }
}
