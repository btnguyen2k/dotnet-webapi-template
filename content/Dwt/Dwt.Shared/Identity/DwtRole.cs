using Microsoft.AspNetCore.Identity;

namespace Dwt.Shared.Identity;
public class DwtRole : IdentityRole
{
	public const string ROLE_NAME_ADMIN = "Admin";
	public const string ROLE_NAME_MANAGER = "Manager";
	public const string ROLE_NAME_STAFF = "Staff";
	public const string ROLE_NAME_ACCOUNT_ADMIN = "Account Admin";
	public const string ROLE_NAME_APPLICATION_ADMIN = "Application Admin";
	public static readonly IEnumerable<string> ALL_ROLE_NAMES_NORMALIZED = [
		ROLE_NAME_ADMIN.ToUpper(),
		ROLE_NAME_MANAGER.ToUpper(),
		ROLE_NAME_STAFF.ToUpper(),
		ROLE_NAME_ACCOUNT_ADMIN.ToUpper(),
		ROLE_NAME_APPLICATION_ADMIN.ToUpper()];

	public const string ROLE_ID_ADMIN = "admin";
	public const string ROLE_ID_MANAGER = "manager";
	public const string ROLE_ID_STAFF = "staff";
	public const string ROLE_ID_ACCOUNT_ADMIN = "account-admin";
	public const string ROLE_ID_APPLICATION_ADMIN = "app-admin";
	public static readonly IEnumerable<string> ALL_ROLE_IDS = [
		ROLE_ID_ADMIN,
		ROLE_ID_MANAGER,
		ROLE_ID_STAFF,
		ROLE_ID_ACCOUNT_ADMIN,
		ROLE_ID_APPLICATION_ADMIN];

	public static readonly DwtRole ADMIN = new() { Id = ROLE_ID_ADMIN, Name = ROLE_NAME_ADMIN, ConcurrencyStamp = new Guid().ToString() };
	public static readonly DwtRole MANAGER = new() { Id = ROLE_ID_MANAGER, Name = ROLE_NAME_MANAGER, ConcurrencyStamp = new Guid().ToString() };
	public static readonly DwtRole STAFF = new() { Id = ROLE_ID_STAFF, Name = ROLE_NAME_STAFF, ConcurrencyStamp = new Guid().ToString() };
	public static readonly DwtRole ACCOUNT_ADMIN = new() { Id = ROLE_ID_ACCOUNT_ADMIN, Name = ROLE_NAME_ACCOUNT_ADMIN, ConcurrencyStamp = new Guid().ToString() };
	public static readonly DwtRole APP_ADMIN = new() { Id = ROLE_ID_APPLICATION_ADMIN, Name = ROLE_NAME_APPLICATION_ADMIN, ConcurrencyStamp = new Guid().ToString() };
	public static readonly IEnumerable<DwtRole> ALL_ROLES = [
		ADMIN,
		MANAGER,
		STAFF,
		ACCOUNT_ADMIN,
		APP_ADMIN];

	public override bool Equals(object? obj)
	{
		if (obj == null || obj as DwtRole == null) return false;
		if (obj == this) return true;
		return (obj as DwtRole)!.Id == Id;
	}

	public override int GetHashCode() => Id.GetHashCode(StringComparison.InvariantCultureIgnoreCase);
}
