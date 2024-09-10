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
	public struct CreateUserReq
	{
		[JsonPropertyName("username")]
		public string Username { get; set; }

		[JsonPropertyName("password")]
		public string Password { get; set; }

		[JsonPropertyName("email")]
		public string Email { get; set; }

		[JsonPropertyName("roles")]
		public IList<string>? Roles { get; set; }
	}

	/// <summary>
	/// Creates a new user account.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="userManager"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("/api/users")]
	[Authorize(Policy = DwtIdentity.POLICY_NAME_ADMIN_OR_CREATE_ACCOUNT_PERM)]
	public async Task<ActionResult<ApiResp<UserResponse>>> CreateUser(
		[FromBody] CreateUserReq req,
		UserManager<DwtUser> userManager,
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

		var iresult = await passwordValidator.ValidateAsync(userManager, null!, req.Password);
		if (iresult != IdentityResult.Success)
		{
			return ResponseNoData(400, iresult.ToString());
		}

		if (await userManager.FindByNameAsync(req.Username) != null)
		{
			return ResponseNoData(400, $"User with username '{req.Username}' already exists.");
		}
		if (await userManager.FindByEmailAsync(req.Email) != null)
		{
			return ResponseNoData(400, $"User with email '{req.Email}' already exists.");
		}

		var currentUser = await GetUserAsync(identityOptions, userManager);
		if (currentUser == null)
		{
			return _respAuthenticationRequired;
		}

		if (req.Roles != null)
		{
			var vresult = await ValidateRoles(currentUser, req.Roles);
			if (vresult != null)
			{
				return vresult;
			}
		}

		var newUser = new DwtUser { UserName = req.Username.ToLower(), Email = req.Email.ToLower() };
		iresult = await userManager.CreateAsync(newUser, req.Password);
		if (iresult != IdentityResult.Success)
		{
			return ResponseNoData(500, iresult.ToString());
		}
		if (req.Roles != null)
		{
			iresult = await userManager.AddToRolesAsync(newUser, req.Roles);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}
		}

		return ResponseOk(new UserResponse
		{
			Id = newUser.Id,
			Username = newUser.UserName!,
			Email = newUser.Email!,
			Roles = userManager.GetRolesAsync(newUser).Result
		});
	}
}
