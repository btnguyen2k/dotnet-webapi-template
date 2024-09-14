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
	/// <returns>null is returned if the role couldnot be created.</returns>
	Task<DwtUser?> CreateAsync(DwtUser user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new user if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>null is returned if the role couldnot be created (e.g. already existed).</returns>
	Task<DwtUser?> CreateIfNotExistsAsync(DwtUser user, CancellationToken cancellationToken = default);

	Task<DwtUser?> GetUserByIDAsync(string userId, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtUser?> GetUserByEmailAsync(string email, UserFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtUser?> GetUserByUserNameAsync(string userName, UserFetchOptions? options = default, CancellationToken cancellationToken = default);

	Task<DwtUser?> UpdateAsync(DwtUser user, CancellationToken cancellationToken = default);

	Task<DwtUser?> UpdateSecurityStampAsync(DwtUser user, CancellationToken cancellationToken = default);

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

	Task<DwtRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);
	Task<DwtRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = default, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>null is returned if the role couldnot be created.</returns>
	Task<DwtRole?> CreateAsync(DwtRole role, CancellationToken cancellationToken = default);

	/// <summary>
	/// Creates a new role if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>null is returned if the role couldnot be created (e.g. already existed).</returns>
	Task<DwtRole?> CreateIfNotExistsAsync(DwtRole role, CancellationToken cancellationToken = default);

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
	/// Adds a claim to the user.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>null is returned if the claim couldnot be added.</remarks>
	Task<IdentityUserClaim<string>?> AddClaimAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the user if it does not exist.
	/// </summary>
	/// <param name="user"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>null is returned if the claim couldnot be added (e.g. already existed).</remarks>
	Task<IdentityUserClaim<string>?> AddClaimIfNotExistsAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the role.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>null is returned if the claim couldnot be added.</remarks>
	Task<IdentityRoleClaim<string>?> AddClaimAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default);

	/// <summary>
	/// Adds a claim to the role if it does not exist.
	/// </summary>
	/// <param name="role"></param>
	/// <param name="claim"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	/// <remarks>null is returned if the claim couldnot be added (e.g. already existed).</remarks>
	Task<IdentityRoleClaim<string>?> AddClaimIfNotExistsAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default);
}
