using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	public struct UserResponse
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public IEnumerable<string>? Roles { get; set; }
	}

	private static async Task<ObjectResult?> ValidateRolesBeforeCreateOrUpdate(
		IIdentityRepository identityRepository,
		DwtUser currentUser,
		IEnumerable<string> requestRoles)
	{
		var roles = await identityRepository.GetRolesAsync(currentUser);
		foreach (var role in requestRoles)
		{
			if (!DwtRole.ALL_ROLE_NAMES_NORMALIZED.Contains(role.ToUpper()))
			{
				return ResponseNoData(400, $"Invalid role '{role}'.");
			}
			if (role.Equals(DwtRole.ROLE_NAME_ADMIN, StringComparison.InvariantCultureIgnoreCase))
			{
				return ResponseNoData(400, $"Cannot create/add another user with/to role '{DwtRole.ROLE_NAME_ADMIN}'.");
			}
			if (role.Equals(DwtRole.ROLE_NAME_ACCOUNT_ADMIN, StringComparison.InvariantCultureIgnoreCase) &&
				!await identityRepository.HasRoleAsync(currentUser, DwtRole.ADMIN))
			{
				// only ADMIN can create/add users with role ACCOUNT_ADMIN
				return ResponseNoData(403, $"Donot have permission to create/add users with/to role '{DwtRole.ROLE_NAME_ACCOUNT_ADMIN}'.");
			}
		}

		return null;
	}

	private async Task<(ActionResult?, DwtUser)> VerifyAuthTokenAndCurrentUser(
	   IIdentityRepository identityRepository,
	   IdentityOptions identityOptions,
	   IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.");
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			// the auth token should still be valid
			return (ResponseNoData(403, tokenValidationResult.Error), null!);
		}

		var currentUser = await GetCurrentUserAsync(identityOptions, identityRepository);
		if (currentUser == null)
		{
			// should not happen  
			return (_respAuthenticationRequired, null!);
		}

		return (null, currentUser);
	}
}
