using Microsoft.AspNetCore.Identity;

namespace Dwt.Shared.Identity;
public class DwtRole : IdentityRole
{
	public static readonly DwtRole ADMIN = new() { Id = "admin", Name = "Admin" };

	public static readonly DwtRole MANAGER = new() { Id = "manager", Name = "Manager" };

	public static readonly DwtRole STAFF = new() { Id = "staff", Name = "Staff" };

	public static readonly DwtRole ACCOUNT_ADMIN = new() { Id = "account-admin", Name = "Account Admin" };

	public static readonly DwtRole APP_ADMIN = new() { Id = "app-admin", Name = "Application Admin" };

	public static readonly IEnumerable<DwtRole> ALL_ROLES = [ADMIN, MANAGER, STAFF, ACCOUNT_ADMIN, APP_ADMIN];
}
