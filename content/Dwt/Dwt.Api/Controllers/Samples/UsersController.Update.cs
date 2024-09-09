using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	public struct UpdateUserReq
	{
		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("roles")]
		public IList<string>? Roles { get; set; }
	}

	/// <summary>
	/// Update an existing user's profile (email, roles).
	/// </summary>
	/// <param name="req"></param>
	/// <param name="uid"></param>
	/// <param name="userManager"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPut("/api/users/{uid}")]
	[Authorize(Roles = "Admin, Account Admin")]
	public async Task<ActionResult<ApiResp<UserResponse>>> UpdateUser([FromBody] UpdateUserReq req, string uid,
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

		if (req.Roles != null)
		{
			var vresult = await ValidateRoles(currentUser, req.Roles);
			if (vresult != null)
			{
				return vresult;
			}
		}

		if (req.Email != null)
		{
			var userByEmail = await userManager.FindByEmailAsync(req.Email);
			if (userByEmail != null && !userByEmail.Email!.Equals(user.Email!, StringComparison.InvariantCultureIgnoreCase))
			{
				return ResponseNoData(400, $"Email '{req.Email}' is already in use.");
			}
		}

		if (req.Roles != null)
		{
			// first, clear all user's current roles
			var roles = await userManager.GetRolesAsync(user);
			var iresult = await userManager.RemoveFromRolesAsync(user, roles);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}

			// then, add new roles
			iresult = await userManager.AddToRolesAsync(user, req.Roles);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}
		}

		if (req.Email != null)
		{
			var iresult = await userManager.SetEmailAsync(user, req.Email);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}
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
