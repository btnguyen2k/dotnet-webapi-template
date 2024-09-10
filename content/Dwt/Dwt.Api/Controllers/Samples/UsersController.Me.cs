using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	/// <summary>
	/// Returns current user's information.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/users/-me")]
	[Authorize]
	public async Task<ActionResult<ApiResp<UserResponse>>> GetMyInfo()
	{
		var user = await GetUserAsync(identityOptions, userManager);
		if (user == null)
		{
			return _respAuthenticationRequired;
		}
		var userResponse = new UserResponse
		{
			Id = user.Id,
			Username = user.UserName!,
			Email = user.Email!,
			Roles = await userManager.GetRolesAsync(user)
		};
		return ResponseOk(userResponse);
	}

	public struct ChangePwdReq
	{
		[JsonPropertyName("old_password")]
		public string OldPassword { get; set; }

		[JsonPropertyName("new_password")]
		public string NewPassword { get; set; }
	}

	public struct ChangePwdResp
	{
		public string Message { get; set; }

		public string Token { get; set; }
	}

	/// <summary>
	/// Changes the currently signed-in user's password.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("/api/users/-me/password")]
	[Authorize]
	public async Task<ActionResult<ApiResp<ChangePwdResp>>> ChangeMyPassword(
		[FromBody] ChangePwdReq req,
		IPasswordValidator<DwtUser> passwordValidator,
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

		var userId = GetUserID(identityOptions);
		var user = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
		if (user == null || !await userManager.CheckPasswordAsync(user, req.OldPassword))
		{
			return ResponseNoData(403, "Invalid user/password combination.");
		}

		var iresult = await passwordValidator.ValidateAsync(userManager, user, req.NewPassword);
		if (iresult != IdentityResult.Success)
		{
			return ResponseNoData(400, iresult.ToString());
		}

		iresult = await userManager.ChangePasswordAsync(user, req.OldPassword, req.NewPassword);
		if (iresult != IdentityResult.Success)
		{
			return ResponseNoData(500, iresult.ToString());
		}

		var refreshResult = authenticatorAsync != null
			? await authenticatorAsync.RefreshAsync(jwtToken!, true)
			: authenticator?.Refresh(jwtToken!, true);

		return ResponseOk(new ChangePwdResp
		{
			Message = "Password changed successfully.",
			Token = refreshResult!.Token!, // changing password should invalidate all previous auth tokens
		});
	}
}
