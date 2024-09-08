using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Dwt.Shared.Identity;
public class DwtIdentity
{
	/// <summary>
	/// Permission to create a new user account.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_USER = new("perm", "create-user");

	/// <summary>
	/// Permission to create a new application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_CREATE_APP = new("perm", "create-app");

	/// <summary>
	/// Permission to delete an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_DELETE_APP = new("perm", "delete-app");

	/// <summary>
	/// Permission to modify an application.
	/// </summary>
	public static readonly Claim CLAIM_PERM_MODIFY_APP = new("perm", "modify-app");

	public const string POLICY_NAME_ADMIN_OR_CREATE_ACCOUNT_PERM = "AdminOrCreateUserPermission";
	public static readonly AuthorizationPolicy POLICY_ADMIN_OR_CREATE_ACCOUNT_PERM = new AuthorizationPolicyBuilder()
		.RequireAuthenticatedUser()
		.RequireAssertion(context =>
		{
			var hasAdminRole = context.User.IsInRole(DwtRole.ROLE_NAME_ADMIN);
			var hasCreateUserPerm = context.User.HasClaim(CLAIM_PERM_CREATE_USER.Type, CLAIM_PERM_CREATE_USER.Value);
			return hasAdminRole || hasCreateUserPerm;
		})
		.Build();
}

public class ClaimEqualityComparer : IEqualityComparer<Claim>
{
	public static readonly ClaimEqualityComparer Instance = new();

	public bool Equals(Claim? x, Claim? y)
	{
		if (x == null && y == null) return true;
		if (x == null || y == null) return false;
		return x.Type == y.Type && x.Value == y.Value;
	}

	public int GetHashCode(Claim obj)
	{
		return HashCode.Combine(obj.Type, obj.Value);
	}
}
