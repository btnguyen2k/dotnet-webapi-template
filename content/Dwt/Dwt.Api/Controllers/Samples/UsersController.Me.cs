using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
	public async Task<ActionResult<ApiResp<UserResponse>>> GetMyInfo(
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository)
	{
		var currentUser = await GetCurrentUserAsync(identityOptions.Value, identityRepository);
		if (currentUser == null)
		{
			return _respAuthenticationRequired;
		}
		var userResponse = new UserResponse
		{
			Id = currentUser.Id,
			Username = currentUser.UserName!,
			Email = currentUser.Email!,
			Roles = (await identityRepository.GetRolesAsync(currentUser)).Select(r => r.Name!)
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
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? Message { get; set; }

		public string Token { get; set; }
	}

	/// <summary>
	/// Changes the currently signed-in user's password.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="passwordHasher"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <param name="userManager"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("/api/users/-me/password")]
	[Authorize]
	public async Task<ActionResult<ApiResp<ChangePwdResp>>> ChangeMyPassword(
		[FromBody] ChangePwdReq req,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		IPasswordValidator<DwtUser> passwordValidator,
		IPasswordHasher<DwtUser> passwordHasher,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync,
		UserManager<DwtUser> userManager)
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
		if (passwordHasher.VerifyHashedPassword(currentUser, currentUser.PasswordHash!, req.OldPassword) == PasswordVerificationResult.Failed)
		{
			return ResponseNoData(403, "Invalid user/password combination.");
		}

		var vresult = await passwordValidator.ValidateAsync(userManager, null!, req.NewPassword);
		if (vresult != IdentityResult.Success)
		{
			return ResponseNoData(400, vresult.ToString());
		}

		currentUser.PasswordHash = passwordHasher.HashPassword(currentUser, req.NewPassword);
		currentUser = await identityRepository.UpdateAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}
		currentUser = await identityRepository.UpdateSecurityStampAsync(currentUser);
		if (currentUser == null)
		{
			return ResponseNoData(500, "Failed to update user.");
		}

		var jwtToken = GetAuthToken();
		var refreshResult = authenticatorAsync != null
			? await authenticatorAsync.RefreshAsync(jwtToken!, true)
			: authenticator?.Refresh(jwtToken!, true);

		return ResponseOk("Password changed successfully.", new ChangePwdResp
		{
			Token = refreshResult!.Token!, // changing password should invalidate all previous auth tokens
		});
	}
}
