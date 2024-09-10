using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	/// <summary>
	/// Fetches all users.
	/// </summary>
	/// <returns></returns>
	/// <remarks>Only ADMIN role can see all users.</remarks>
	[HttpGet("/api/users")]
	[Authorize(Roles = DwtRole.ROLE_NAME_ADMIN)]
	public async Task<ActionResult<ApiResp<UserResponse>>> GetAll()
	{
		var users = await userManager.Users.ToListAsync();
		var userResponses = users.ConvertAll(u => new UserResponse
		{
			Id = u.Id,
			Username = u.UserName!,
			Email = u.Email!,
			Roles = userManager.GetRolesAsync(u).Result
		});
		return ResponseOk(userResponses);
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
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResponse>>> GetByID(string id)
	{
		var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
		if (user == null)
		{
			return _respNotFound;
		}

		var userId = GetUserID(identityOptions);
		var currentUser = userManager.Users.FirstOrDefault(u => u.Id == userId);
		if (currentUser == null)
		{
			//should not happen
			return _respAuthenticationRequired;
		}
		var currentUserRoles = await userManager.GetRolesAsync(currentUser);

		if (!currentUserRoles.Contains(DwtRole.ROLE_NAME_ADMIN) && currentUser.Id == user.Id)
		{
			// return "Not Found" in this case to avoid leaking the fact that the user exists
			// return _respAccessDenied;
			return _respNotFound;
		}

		return ResponseOk(new UserResponse
		{
			Id = user.Id,
			Username = user.UserName!,
			Email = user.Email!,
			Roles = userManager.GetRolesAsync(user).Result
		});
	}
}
