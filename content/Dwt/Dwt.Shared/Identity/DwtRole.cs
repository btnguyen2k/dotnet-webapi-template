using Microsoft.AspNetCore.Identity;

namespace Dwt.Shared.Identity;
public class DwtRole : IdentityRole
{
	public const string ROLE_NAME_ADMIN = "Admin";
	public const string ROLE_NAME_MANAGER = "Manager";
	public const string ROLE_NAME_STAFF = "Staff";
	public const string ROLE_NAME_ACCOUNT_ADMIN = "Account Admin";
	public const string ROLE_NAME_APPLICATION_ADMIN = "Application Admin";

	public static readonly DwtRole ADMIN = new() { Id = "admin", Name = ROLE_NAME_ADMIN };

	public static readonly DwtRole MANAGER = new() { Id = "manager", Name = ROLE_NAME_MANAGER };

	public static readonly DwtRole STAFF = new() { Id = "staff", Name = ROLE_NAME_STAFF };

	public static readonly DwtRole ACCOUNT_ADMIN = new() { Id = "account-admin", Name = ROLE_NAME_ACCOUNT_ADMIN };

	public static readonly DwtRole APP_ADMIN = new() { Id = "app-admin", Name = ROLE_NAME_APPLICATION_ADMIN };

	public static readonly IEnumerable<DwtRole> ALL_ROLES = [ADMIN, MANAGER, STAFF, ACCOUNT_ADMIN, APP_ADMIN];
}
