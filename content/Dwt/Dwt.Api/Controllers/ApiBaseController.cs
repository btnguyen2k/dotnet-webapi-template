using Dwt.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Controllers;

/// <summary>
/// Base controller for other API controllers in the application.
/// </summary>
/// <remarks>
///		- Uses application/json as the default content type.<br/>
///		- Provides utility methods to conveniently return common responses.
/// </remarks>
[ApiController]
[Consumes("application/json")]
public abstract class ApiBaseController : ControllerBase
{
	/// <summary>
	/// Convenience method to return a 200 OK response.
	/// </summary>
	/// <returns></returns>
	protected static ObjectResult ResponseOk() => ResponseOk<object>(null);

	/// <summary>
	/// Generic response for "200 OK".
	/// </summary>
	protected static readonly ObjectResult _respOk = ResponseNoData(200, "Ok.");

	/// <summary>
	/// Convenience method to return a 200 OK response with data.
	/// </summary>
	/// <param name="data"></param>
	/// <returns></returns>
	protected static ObjectResult ResponseOk<T>(T? data) => data == null ? _respOk : new OkObjectResult(new ApiResp<T>
	{
		Status = 200,
		Message = "Ok.",
		Data = data
	});

	/// <summary>
	/// Convenience method to return a response without attached data.
	/// </summary>
	/// <param name="statusCode"></param>
	/// <returns></returns>
	protected static ObjectResult ResponseNoData(int statusCode) => ResponseNoData(statusCode, null);

	/// <summary>
	/// Convenience method to return a response with a message and without attached data.
	/// </summary>
	/// <param name="statusCode"></param>
	/// <param name="message"></param>
	/// <returns></returns>
	protected static ObjectResult ResponseNoData(int statusCode, string? message) => new(String.IsNullOrWhiteSpace(message) ? new ApiResp<object>
	{
		Status = statusCode
	}
	: new ApiResp<object>
	{
		Status = statusCode,
		Message = message
	})
	{
		StatusCode = statusCode
	};

	/// <summary>
	/// Generic response for "401 Unauthorized" errors.
	/// </summary>
	protected static readonly ObjectResult _respAuthenticationRequired = ResponseNoData(401, "Authentication required.");

	/// <summary>
	/// Generic response for "403 Forbidden" errors.
	/// </summary>
	protected static readonly ObjectResult _respAccessDenied = ResponseNoData(403, "Access denied.");

	/// <summary>
	/// Generic response for "404 Not Found" errors.
	/// </summary>
	protected static readonly ObjectResult _respNotFound = ResponseNoData(404, "Not found.");

	/// <summary>
	/// Generic response for "501 Not Implemented" errors.
	/// </summary>
	protected static readonly ObjectResult _respNotImplemented = ResponseNoData(501, "Not implemented.");

	/// <summary>
	/// Generic response for "503 Service Unavailable" errors.
	/// </summary>
	protected static readonly ObjectResult _respServiceUnavailable = ResponseNoData(503, "Server is unavailable to handle the request.");

	/// <summary>
	/// Get the attached user-id from the http-context.
	/// </summary>
	/// <returns></returns>
	protected string? GetRequestUserId()
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Retrieve the auth token from the request headers.
	/// </summary>
	/// <returns></returns>
	protected string? GetAuthToken()
	{
		return HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
	}

	/// <summary>
	/// Convenience method to validate an auth token.
	/// </summary>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	protected async Task<TokenValidationResp> ValidateAuthTokenAsync(IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync, string token)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}
		return authenticatorAsync != null
			? await authenticatorAsync.ValidateAsync(token)
			: authenticator!.Validate(token);
	}

	/// <summary>
	/// Get the attached user-id from the http-context.
	/// </summary>
	/// <param name="opts"></param>
	/// <returns></returns>
	protected string? GetUserID(IdentityOptions opts)
	{
		return HttpContext.User.Claims.FirstOrDefault(c => c.Type == opts.ClaimsIdentity.UserIdClaimType)?.Value;
	}

	/// <summary>
	/// Get the attached user-name from the http-context.
	/// </summary>
	/// <param name="opts"></param>
	/// <returns></returns>
	protected string? GetUserName(IdentityOptions opts)
	{
		return HttpContext.User.Claims.FirstOrDefault(c => c.Type == opts.ClaimsIdentity.UserNameClaimType)?.Value;
	}
}
