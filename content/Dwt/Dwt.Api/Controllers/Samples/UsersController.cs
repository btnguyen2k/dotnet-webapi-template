using Dwt.Api.Helpers;
using Dwt.Shared.Identity;
using Dwt.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dwt.Api.Controllers.Samples;

public class UsersController(
	IUserRepository userRepo,
	IOptions<IdentityOptions> identityOptions,
	UserManager<DwtUser> userManager) : ApiBaseController
{
	/// <summary>
	/// Fetches all users.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Only ADMIN role can see all users.</remarks>
	[HttpGet("/api/users")]
	[JwtAuthorize(Roles = "ADMIN")]
	public ActionResult<ApiResp<User>> GetAll()
	{
		return ResponseOk(userRepo.GetAll());
	}

	/// <summary>
	/// Fetches a user by ID.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	/// <remarks>
	/// - Users can only see their own information.
	/// - ADMIN role can see all users.
	/// </remarks>
	[HttpGet("/api/users/{id}")]
	[JwtAuthorize]
	public ActionResult<ApiResp<User>> GetByID(string id)
	{
		var user = userRepo.GetByID(id);
		if (user == null)
		{
			return _respNotFound;
		}

		var requestUser = userRepo.GetByID(GetRequestUserId() ?? "");
		if (requestUser == null)
		{
			//should not happen
			return _respAuthenticationRequired;
		}

		if (!requestUser.HasRole("ADMIN") && requestUser.Id != user.Id)
		{
			// return "Not Found" in this case to avoid leaking the fact that the user exists
			// return _respAccessDenied;
			return _respNotFound;
		}

		return ResponseOk(user);
	}

	/// <summary>
	/// Returns current user's information.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/users/-me")]
	[Authorize]
	public async Task<ActionResult<ApiResp<User>>> GetMyInfo()
	{
		var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == identityOptions.Value.ClaimsIdentity.UserIdClaimType)?.Value;
		var user = userManager.Users.FirstOrDefault(u => u.Id == userId);
		if (user == null)
		{
			return _respAuthenticationRequired;
		}
		var userResponse = new Dictionary<string, object?>()
		{
			{"id", user.Id},
			{"username", user.UserName},
			{"email", user.Email},
			{"roles", await userManager.GetRolesAsync(user)},
		};
		return ResponseOk(userResponse);
	}

	/* User list is immutable, can not add/remove/update users for now */
}
