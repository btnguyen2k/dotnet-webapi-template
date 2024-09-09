using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dwt.Api.Controllers.Samples;

public partial class UsersController : ApiBaseController
{
	protected readonly IdentityOptions identityOptions;
	protected readonly UserManager<DwtUser> userManager;

	public UsersController(IOptions<IdentityOptions> identityOptions, UserManager<DwtUser> userManager)
	{
		ArgumentNullException.ThrowIfNull(identityOptions, nameof(identityOptions));
		ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

		this.identityOptions = identityOptions.Value;
		this.userManager = userManager;
	}

	public struct UserResponse
	{
		public string Id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public IList<string> Roles { get; set; }
	}

	private async Task<ObjectResult?> ValidateRoles(DwtUser currentUser, IEnumerable<string> reqRoles)
	{
		foreach (var role in reqRoles)
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
				!await userManager.IsInRoleAsync(currentUser, DwtRole.ROLE_NAME_ADMIN))
			{
				// only ADMIN can create/add users with role ACCOUNT_ADMIN
				return ResponseNoData(403, $"Donot have permission to create/add users with/to role '{DwtRole.ROLE_NAME_ACCOUNT_ADMIN}'.");
			}
		}

		return null;
	}
}
