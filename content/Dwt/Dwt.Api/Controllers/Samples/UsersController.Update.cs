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
	public struct UpdateUserReq
	{
		[JsonPropertyName("email")]
		public string? Email { get; set; }

		[JsonPropertyName("roles")]
		public IList<string>? Roles { get; set; }
	}

	/// <summary>
	/// Updates an existing user's profile (email, roles).
	/// </summary>
	/// <param name="req"></param>
	/// <param name="uid"></param>
	/// <param name="identityOptions"></param>
	/// <param name="identityRepository"></param>
	/// <param name="normalizer"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPut("/api/users/{uid}")]
	[Authorize(Roles = "Admin, Account Admin")]
	public async Task<ActionResult<ApiResp<UserResponse>>> UpdateUser([FromBody] UpdateUserReq req,
		string uid,
		IOptions<IdentityOptions> identityOptions,
		IIdentityRepository identityRepository,
		ILookupNormalizer normalizer,
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

		if (req.Roles != null)
		{
			var vresult = await ValidateRolesBeforeCreateOrUpdate(identityRepository, currentUser, req.Roles);
			if (vresult != null)
			{
				return vresult;
			}
		}

		if (req.Email != null)
		{
			var userByEmail = await identityRepository.GetUserByEmailAsync(req.Email);
			if (userByEmail != null && !userByEmail.Email!.Equals(reqUser.Email!, StringComparison.InvariantCultureIgnoreCase))
			{
				return ResponseNoData(400, $"Email '{req.Email}' is already in use.");
			}
		}

		if (req.Roles != null)
		{
			// first, clear all user's current roles
			var roles = await identityRepository.GetRolesAsync(reqUser);
			var iresult = await identityRepository.RemoveFromRolesAsync(reqUser, roles);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}

			// then, add new roles
			iresult = await identityRepository.AddToRolesAsync(reqUser, req.Roles);
			if (iresult != IdentityResult.Success)
			{
				return ResponseNoData(500, iresult.ToString());
			}

			// chaning roles should also change security stamp
			reqUser = await identityRepository.UpdateSecurityStampAsync(reqUser);
			if (reqUser == null)
			{
				return ResponseNoData(500, "Failed to update user's security stamp.");
			}
		}

		if (req.Email != null)
		{
			reqUser.Email = req.Email;
			reqUser.NormalizedEmail = normalizer.NormalizeEmail(req.Email);
			reqUser = await identityRepository.UpdateAsync(reqUser);
			if (reqUser == null)
			{
				return ResponseNoData(500, "Failed to update user's email.");
			}
		}

		return ResponseOk(new UserResponse
		{
			Id = reqUser.Id,
			Username = reqUser.UserName!,
			Email = reqUser.Email!,
			Roles = (await identityRepository.GetRolesAsync(reqUser)).Select(r => r.Name!)
		});
	}
}
