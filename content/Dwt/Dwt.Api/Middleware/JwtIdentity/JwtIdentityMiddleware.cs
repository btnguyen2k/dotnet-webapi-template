using Dwt.Api.Helpers;
using Dwt.Api.Services;
using Dwt.Shared.Models;
using System.Security.Claims;

namespace Dwt.Api.Middleware.JwtIdentity;

/// <summary>
/// Middleware that decodes the JWT token in the request Authorization header (if any) and attaches
/// the user-id/user to the HttpContext.Items collection to make it accessible within the scope of the current request.
/// </summary>
public class JwtIdentityMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IJwtService _jwtService;
	private readonly string _userIdKey;
	private readonly string _userKey;
	private readonly IUserRepository _userRepo;


	public JwtIdentityMiddleware(RequestDelegate next, IJwtService jwtService, IUserRepository userRepo, IConfiguration config)
	{
		ArgumentNullException.ThrowIfNull(nameof(next));
		ArgumentNullException.ThrowIfNull(nameof(config));
		ArgumentNullException.ThrowIfNull(nameof(userRepo));
		ArgumentNullException.ThrowIfNull(nameof(jwtService));

		_next = next;
		_jwtService = jwtService;
		_userIdKey = config["Jwt:HTTP_CTX_ITEM_USERID"] ?? GlobalVars.HTTP_CTX_ITEM_USERID_DEFAULT;
		_userKey = config["Jwt:HTTP_CTX_ITEM_USER"] ?? "";
		_userRepo = userRepo;
	}

	public async Task Invoke(HttpContext context)
	{
		var token = context.Request.Headers.Authorization.FirstOrDefault(hv => hv != null && hv.StartsWith("Bearer "))?.Substring(7);
		if (token != null)
		{
			await AttachUserIdToContext(context, token);
		}

		await _next(context);
	}

	private async Task AttachUserIdToContext(HttpContext context, string jwtToken)
	{
		try
		{
			var principal = _jwtService.ValidateToken(jwtToken);
			var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
			context.Items[_userIdKey] = claimUserId;

			if (!string.IsNullOrEmpty(_userKey) && !string.IsNullOrEmpty(claimUserId))
			{
				context.Items[_userKey] = await _userRepo.GetByIDAsync(claimUserId);
			}
		}
		catch
		{
			// do nothing if jwt validation fails
			// user-id is not attached to context
		}
	}
}
