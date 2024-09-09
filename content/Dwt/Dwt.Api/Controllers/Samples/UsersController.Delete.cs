using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	/// <summary>
	/// Delete an existing user account.
	/// </summary>
	/// <param name="uid"></param>
	/// <param name="userManager"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpDelete("/api/users/{uid}")]
	[Authorize(Roles = "Admin, Account Admin")]
	public async Task<ActionResult<ApiResp<UserResponse>>> DeleteUser(string uid,
		UserManager<DwtUser> userManager,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var user = await userManager.FindByIdAsync(uid);
		if (user == null)
		{
			return _respNotFound;
		}

		var currentUser = await GetUserAsync(identityOptions, userManager);
		if (currentUser == null)
		{
			// should not happen
			return _respAuthenticationRequired;
		}

		if (currentUser.Id == user.Id)
		{
			return ResponseNoData(400, "Cannot delete yourself.");
		}

		if (await userManager.IsInRoleAsync(user, DwtRole.ROLE_NAME_ADMIN))
		{
			return ResponseNoData(400, $"Cannot delete user with role '{DwtRole.ROLE_NAME_ADMIN}'.");
		}

		var iresult = await userManager.DeleteAsync(user);
		if (iresult != IdentityResult.Success)
		{
			return ResponseNoData(500, iresult.ToString());
		}

		return ResponseOk(new UserResponse
		{
			Id = user.Id,
			Username = user.UserName!,
			Email = user.Email!,
		});
	}
}
