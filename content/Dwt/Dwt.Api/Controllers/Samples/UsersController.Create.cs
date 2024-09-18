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
	/// <param name="identityRepository"></param>
	/// <param name="identityOptions"></param>
	/// <param name="passwordValidator"></param>
	/// <param name="lookupNormalizer"></param>
	/// <param name="passwordHasher"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <param name="userManager"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentNullException"></exception>
	[HttpPost("/api/users")]
	[Authorize(Policy = DwtIdentity.POLICY_NAME_ADMIN_OR_CREATE_ACCOUNT_PERM)]
	public async Task<ActionResult<ApiResp<UserResponse>>> CreateUser(
		[FromBody] CreateUserReq req,
		IIdentityRepository identityRepository,
		IOptions<IdentityOptions> identityOptions,
		IPasswordValidator<DwtUser> passwordValidator,
		ILookupNormalizer lookupNormalizer,
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

		var vPwdResult = await passwordValidator.ValidateAsync(userManager, null!, req.Password);
		if (vPwdResult != IdentityResult.Success)
		{
			// supplied password should pass complexity validation
			return ResponseNoData(400, vPwdResult.ToString());
		}

		if (await identityRepository.GetUserByUserNameAsync(req.Username) != null)
		{
			return ResponseNoData(400, $"User with username '{req.Username}' already exists.");
		}
		if (await identityRepository.GetUserByEmailAsync(req.Email) != null)
		{
			return ResponseNoData(400, $"User with email '{req.Email}' already exists.");
		}

		if (req.Roles != null)
		{
			var vRolesResult = await ValidateRolesBeforeCreateOrUpdate(identityRepository, currentUser, req.Roles);
			if (vRolesResult != null)
			{
				// supplied roles should be valid
				return vRolesResult;
			}
		}

		var newUser = new DwtUser
		{
			UserName = req.Username.ToLower(),
			PasswordHash = passwordHasher.HashPassword(null!, req.Password),
			NormalizedUserName = lookupNormalizer.NormalizeName(req.Username),
			Email = req.Email.ToLower(),
			NormalizedEmail = lookupNormalizer.NormalizeEmail(req.Email)
		};
		var iresult = await identityRepository.CreateAsync(newUser);
		if (iresult != IdentityResult.Success)
		{
			//return ResponseNoData(500, $"{string.Join(", ", iresult.Errors.Select(r => r.Description))}");
			return ResponseNoData(500, iresult.ToString());
		}
		if (req.Roles != null)
		{
			iresult = await identityRepository.AddToRolesAsync(newUser, req.Roles);
			if (iresult != IdentityResult.Success)
			{
				//return ResponseNoData(500, $"{string.Join(", ", iresult.Errors.Select(r => r.Description))}");
				return ResponseNoData(500, iresult.ToString());
			}
		}

		return ResponseOk(new UserResponse
		{
			Id = newUser.Id,
			Username = newUser.UserName!,
			Email = newUser.Email!,
			Roles = (await identityRepository.GetRolesAsync(newUser)).Select(r => r.Name!)
		});
	}
}
