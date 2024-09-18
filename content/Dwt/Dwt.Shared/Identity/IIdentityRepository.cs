using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Dwt.Shared.Identity;

public class UserFetchOptions
{
	public static readonly UserFetchOptions DEFAULT = new();

	public bool IncludeRoles { get; set; } = false;
	public bool IncludeClaims { get; set; } = false;
}

public class RoleFetchOptions
{
	public static readonly RoleFetchOptions DEFAULT = new();
}

public interface IIdentityRepository
{
	/// <summary>
	/// Creates a new user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> CreateAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new user if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> CreateIfNotExistsAsync(DwtUser user, CancellationToken cancellationToken = default);

	Task<DwtUser?> GetUserByIDAsync(string userId, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtUser?> GetUserByEmailAsync(string email, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtUser?> GetUserByUserNameAsync(string userName, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<IEnumerable<DwtUser>> AllUsersAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The user with updated data.</returns>
	/// <remarks>
	///		null is returned if the update operated didnot succeed.
	///		user's concurrency stamp is automatically updated and reflected in the returned instance.
	///	</remarks>
	Task<DwtUser?> UpdateAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates the security stamp of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The user with new security stamp</returns>
	/// <remarks>
	///		null is returned if the update operated didnot succeed.
	///		user's concurrency stamp is automatically updated and reflected in the returned instance.
	///	</remarks>
	Task<DwtUser?> UpdateSecurityStampAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Deletes an existing user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user does not exist, the operation is considered successful.
	/// </remarks>
	Task<IdentityResult> DeleteAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the roles of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user has no roles, an empty collection is returned.
	///		If the user does not exist, null is returned.
	/// </remarks>
	Task<IEnumerable<DwtRole>> GetRolesAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the claims of the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user has no claims, an empty collection is returned.
	///		If the user does not exist, null is returned.
	/// </remarks>
	Task<IEnumerable<IdentityUserClaim<string>>> GetClaimsAsync(DwtUser user, CancellationToken cancellationToken = default);

	Task<bool> HasRoleAsync(DwtUser user, DwtRole role, CancellationToken cancellationToken = default);

	Task<bool> HasRoleAsync(DwtUser user, string roleName, CancellationToken cancellationToken = default);

	Task<IdentityResult> AddToRolesAsync(DwtUser user, IEnumerable<DwtRole> roles, CancellationToken cancellationToken = default);
	Task<IdentityResult> AddToRolesAsync(DwtUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the user from the specified roles.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="roles"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user is not in the specified roles, the operation is considered successful.
	/// </remarks>
	Task<IdentityResult> RemoveFromRolesAsync(DwtUser user, IEnumerable<DwtRole> roles, CancellationToken cancellationToken = default);

	/// <summary>
	/// Removes the user from the specified roles.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="roleNames"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the user is not in the specified roles, the operation is considered successful.
	/// </remarks>
	Task<IdentityResult> RemoveFromRolesAsync(DwtUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the user if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimIfNotExistsAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds claims to the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimsAsync(DwtUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);

	Task<DwtRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> CreateAsync(DwtRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new role if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> CreateIfNotExistsAsync(DwtRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Retrieves the claims of the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>
	///		If the role has no claims, an empty collection is returned.
	///		If the role does not exist, null is returned.
	/// </remarks>
	Task<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(DwtRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the role if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimIfNotExistsAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds claims to the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claims"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	Task<IdentityResult> AddClaimsAsync(DwtRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default);
}
