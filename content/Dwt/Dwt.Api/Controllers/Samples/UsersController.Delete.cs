using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	/// <summary>
	/// Deletes an existing user account.
	/// </summary>
	/// <param name="uid"></param>
	/// <param name="identityRepository"></param>
	/// <param name="identityOptions"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpDelete("/api/users/{uid}")]
	[Authorize(Roles = "Admin, Account Admin")]
	public async Task<ActionResult<ApiResp<UserResponse>>> DeleteUser(string uid,
		IIdentityRepository identityRepository,
		IOptions<IdentityOptions> identityOptions,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		var (vAuthTokenResult, currentUser) = await VerifyAuthTokenAndCurrentUser(
			identityRepository,
			identityOptions.Value,
			authenticator, authenticatorAsync);
		if (vAuthTokenResult != null)
		{
			// current auth token and signed-in user should all be valid
			return vAuthTokenResult;
		}

		var reqUser = await identityRepository.GetUserByIDAsync(uid);
		if (reqUser == null)
		{
			return _respNotFound;
		}

		if (currentUser.Id == reqUser.Id)
		{
			return ResponseNoData(400, "Cannot delete yourself.");
		}

		if (await identityRepository.HasRoleAsync(reqUser, DwtRole.ADMIN))
		{
			return ResponseNoData(403, $"Cannot delete user with role '{DwtRole.ROLE_NAME_ADMIN}'.");
		}

		var iresult = await identityRepository.DeleteAsync(reqUser);
		if (iresult != IdentityResult.Success)
		{
			//return ResponseNoData(500, $"{string.Join(", ", iresult.Errors.Select(r => r.Description))}");
			return ResponseNoData(500, iresult.ToString());
		}

		return ResponseOk(new UserResponse
		{
			Id = reqUser.Id,
			Username = reqUser.UserName!,
			Email = reqUser.Email!,
		});
	}
}
