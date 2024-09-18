using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
	public async Task<ActionResult<ApiResp<UserResponse>>> GetAll(IIdentityRepository identityRepository)
	{
		var users = await identityRepository.AllUsersAsync();
		var userResponses = users.Select(u => new UserResponse
		{
			Id = u.Id,
			Username = u.UserName!,
			Email = u.Email!,
			Roles = identityRepository.GetRolesAsync(u).Result.Select(r => r.Name!)
		});
		return ResponseOk(userResponses);
	}

	/// <summary>
	/// Fetches a user by ID.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <returns></returns>
	/// <remarks>
	/// - Users can only see their own information.
	/// - ADMIN role can see all users.
	/// </remarks>
	[HttpGet("/api/users/{id}")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResponse>>> GetByID(string id,
		IIdentityRepository identityRepository,
		IOptions<IdentityOptions> identityOptions)
	{
		var user = await identityRepository.GetUserByIDAsync(id);
		if (user == null)
		{
			return _respNotFound;
		}

		var currentUser = await GetCurrentUserAsync(identityOptions.Value, identityRepository);
		if (currentUser == null)
		{
			//should not happen
			return _respAuthenticationRequired;
		}
		var currentUserRoles = await identityRepository.GetRolesAsync(currentUser);
		if (!currentUserRoles.Contains(DwtRole.ADMIN) && currentUser.Id != user.Id)
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
			Roles = (await identityRepository.GetRolesAsync(user)).Select(r => r.Name!)
		});
	}
}
