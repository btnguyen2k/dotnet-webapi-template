using Dwt.Api.Helpers;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Dwt.Api.Middleware.JwtIdentity;

/// <summary>
/// Middleware that decodes the JWT token in the request Authorization header (if any) and attaches
/// the user-id to the HttpContext.Items collection to make it accessible within the scope of the current request.
/// </summary>
/// <remarks>
///     Sample usage:
///         app.UseMiddleware&lt;JwtMiddleware&gt;();
/// </remarks>
public class JwtIdentityMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IOptions<JwtIdentityOptions> _options;

	public JwtIdentityMiddleware(RequestDelegate next,
		IOptions<JwtIdentityOptions> options)
	{
		ArgumentNullException.ThrowIfNull(nameof(next));
		ArgumentNullException.ThrowIfNull(nameof(options));

		_next = next;
		_options = options;
	}

	public async Task Invoke(HttpContext context)
	{
		var token = context.Request.Headers.Authorization.FirstOrDefault(hv => hv != null && hv.StartsWith("Bearer "))?.Substring(7);
		if (token != null)
		{
			AttachUserIdToContext(context, token);
		}

		await _next(context);
	}

	private void AttachUserIdToContext(HttpContext context, string jwtToken)
	{
		if (!string.IsNullOrEmpty(jwtToken) && !string.IsNullOrEmpty(_options.Value.UserIdKey))
		{
			try
			{
				var principal = JwtRepository.ValidateToken(jwtToken);
				var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
				context.Items[_options.Value.UserIdKey] = claimUserId;
			}
			catch
			{
				// do nothing if jwt validation fails
				// user-id is not attached to context
			}
		}
	}
}
