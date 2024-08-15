using Dwt.Api.Helpers;
using Dwt.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Dwt.Api.Middleware.JwtIdentity;

/// <summary>
/// Action filter that checks if the executing action is marked with JwtAuthorizeAttribute.
/// If yes, it will authorize the request based on the rules from the attached JwtAuthorizeAttribute.
/// </summary>
public class JwtAuthGlobalFilter : IAsyncActionFilter
{
	private readonly IUserRepository _userRepo;
	private readonly string _userIdKey;
	private readonly string _userKey;

	public JwtAuthGlobalFilter(IUserRepository userRepo, IConfiguration config)
	{
		ArgumentNullException.ThrowIfNull(userRepo, nameof(userRepo));
		ArgumentNullException.ThrowIfNull(config, nameof(config));

		_userRepo = userRepo;
		_userIdKey = config["Jwt:HTTP_CTX_ITEM_USERID"] ?? GlobalVars.HTTP_CTX_ITEM_USERID_DEFAULT;
		_userKey = config["Jwt:HTTP_CTX_ITEM_USER"] ?? "";
	}

	///// <inheritdoc/>
	//public void OnActionExecuted(ActionExecutedContext context)
	//{
	//	//noop
	//}

	private static readonly IActionResult _unauthorizedResult = new JsonResult(new
	{
		status = StatusCodes.Status401Unauthorized,
		message = "Unauthorized"
	})
	{ StatusCode = StatusCodes.Status401Unauthorized };

	private static readonly IActionResult _deniedResult = new JsonResult(new
	{
		status = StatusCodes.Status403Forbidden,
		message = "Access denied"
	})
	{ StatusCode = StatusCodes.Status403Forbidden };


	/// <inheritdoc/>
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		var jwtAuthorizeAttribute = context.ActionDescriptor.FilterDescriptors
			.Select(x => x.Filter).OfType<JwtAuthorizeAttribute>().FirstOrDefault();
		if (jwtAuthorizeAttribute == null)
		{
			//action is NOT marked with JwtAuthorizeAttribute
			await next();
			return;
		}


		var requestUser = await FetchUser(context);
		if (requestUser == null)
		{
			context.Result = _unauthorizedResult;
			return;
		}

		var acceptedRoles = User.RoleStrToArr(jwtAuthorizeAttribute.Roles ?? "");
		if (!acceptedRoles.Any(r => requestUser.HasRole(r)))
		{
			context.Result = _deniedResult;
			return;
		}
		await next();
	}

	private async ValueTask<User?> FetchUser(ActionExecutingContext context)
	{
		if (context.HttpContext.Items.TryGetValue(_userKey, out var user) && user != null)
		{
			return (User)user;
		}
		context.HttpContext.Items.TryGetValue(_userIdKey, out var userId);
		return await _userRepo.GetByIDAsync(userId?.ToString() ?? "");
	}
}
