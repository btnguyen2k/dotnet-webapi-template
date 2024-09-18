using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Dwt.Shared.EF.Identity;

public class IdentityDbContextRepository : IdentityDbContext<DwtUser, DwtRole, string>, IIdentityRepository
{
	private readonly ILogger<IdentityDbContextRepository> logger;

	public IdentityDbContextRepository(
		DbContextOptions<IdentityDbContextRepository> options,
		ILogger<IdentityDbContextRepository> logger) : base(options)
	{
		this.logger = logger;
		//logger.LogCritical("IdentityDbContextRepository instances created.");
		//ChangeTracker.DetectingEntityChanges += (sender, e) =>
		//{
		//	logger.LogError("Detected changes: {e}", e.Entry.ToString());
		//};
	}

	//protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	//{
	//	base.OnConfiguring(optionsBuilder);
	//}

	private void ChangeTracker_DetectedAllChanges(object? sender, Microsoft.EntityFrameworkCore.ChangeTracking.DetectedChangesEventArgs e) => throw new NotImplementedException();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		new RoleEntityTypeConfiguration().Configure(modelBuilder.Entity<DwtRole>());
		new IdentityRoleClaimEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityRoleClaim<string>>());
		new IdentityUserEntityTypeConfiguration().Configure(modelBuilder.Entity<DwtUser>());
		new IdentityUserClaimEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserClaim<string>>());
		//new IdentityUserLoginEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserLogin<string>>());
		new IdentityUserRoleEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserRole<string>>());
		//new IdentityUserTokenEntityTypeConfiguration().Configure(modelBuilder.Entity<IdentityUserToken<string>>());

		modelBuilder.Ignore<IdentityUserLogin<string>>();
		modelBuilder.Ignore<IdentityUserToken<string>>();
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<IdentityResult> CreateAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		var entry = Users.Add(user);
		entry.Entity.SecurityStamp = Guid.NewGuid().ToString("N");
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError() { Code = "500", Description = $"User '{user.UserName}' couldnot be created." });
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> CreateIfNotExistsAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		if (await GetUserByIDAsync(user.Id, cancellationToken: cancellationToken) is not null) return IdentityResult.Success;
		return await CreateAsync(user, cancellationToken);
	}

	private async Task<DwtUser?> PostFetchUser(DwtUser? user, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		if (user is null || options is null || cancellationToken.IsCancellationRequested) return user;
		user.Roles = options.IncludeRoles ? await GetRolesAsync(user, cancellationToken) : null;
		user.Claims = options.IncludeClaims ? await GetClaimsAsync(user, cancellationToken) : null;
		return user;
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> GetUserByIDAsync(string userId, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> GetUserByEmailAsync(string email, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> GetUserByUserNameAsync(string userName, UserFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		var user = await Users.FirstOrDefaultAsync(u => u.UserName == userName, cancellationToken);
		return await PostFetchUser(user, options, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<DwtUser>> AllUsersAsync(CancellationToken cancellationToken = default)
	{
		return await Users.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> UpdateAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		var entry = Users.Update(user);
		entry.Entity.Touch();
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entry.Entity : null;
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> UpdateSecurityStampAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		user.SecurityStamp = Guid.NewGuid().ToString("N");
		return await UpdateAsync(user, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> DeleteAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		Users.Remove(user);
		await SaveChangesAsync(cancellationToken);
		return IdentityResult.Success;
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<DwtRole>> GetRolesAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		return await UserRoles
			.Where(ur => ur.UserId == user.Id)
			.Join(Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<IdentityUserClaim<string>>> GetClaimsAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		return await UserClaims
			.Where(uc => uc.UserId == user.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async Task<bool> HasRoleAsync(DwtUser user, DwtRole role, CancellationToken cancellationToken = default)
	{
		var userRoles = await GetRolesAsync(user, cancellationToken: cancellationToken);
		return userRoles.Contains(role);
	}

	/// <inheritdoc/>
	public async Task<bool> HasRoleAsync(DwtUser user, string roleName, CancellationToken cancellationToken = default)
	{
		var userRoles = await GetRolesAsync(user, cancellationToken: cancellationToken);
		return userRoles.Any(r => roleName.Equals(r.Name, StringComparison.InvariantCultureIgnoreCase));
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddToRolesAsync(DwtUser user, IEnumerable<DwtRole> roles, CancellationToken cancellationToken = default)
	{
		var rolesList = roles is not null ? roles.ToList() : []; // Convert to list to avoid multiple enumerations
		if (rolesList.Count == 0) return IdentityResult.Success;
		foreach (var role in rolesList)
		{
			UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
		}
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "500",
				Description = $"Failed to add user '{user.UserName}' to roles [{string.Join(", ", rolesList.Select(r => r.Name!))}]"
			});
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddToRolesAsync(DwtUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
	{
		var roles = new List<DwtRole>();
		foreach (var roleName in roleNames)
		{
			var role = await GetRoleByNameAsync(roleName, cancellationToken: cancellationToken);
			if (role is null)
				return IdentityResult.Failed(new IdentityError()
				{
					Code = "404",
					Description = $"Role '{roleName}' does not exist."
				});
			roles.Add(role);
		}
		return await AddToRolesAsync(user, roles, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> RemoveFromRolesAsync(DwtUser user, IEnumerable<DwtRole> roles, CancellationToken cancellationToken = default)
	{
		var rolesList = roles is not null ? roles.ToList() : []; // Convert to list to avoid multiple enumerations
		if (rolesList.Count == 0) return IdentityResult.Success;
		UserRoles.RemoveRange(rolesList.Select(r => new IdentityUserRole<string>() { RoleId = r.Id, UserId = user.Id }));
		await SaveChangesAsync(cancellationToken);
		return IdentityResult.Success;
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> RemoveFromRolesAsync(DwtUser user, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
	{
		var roles = new List<DwtRole>();
		foreach (var roleName in roleNames)
		{
			var role = await GetRoleByNameAsync(roleName, cancellationToken: cancellationToken);
			if (role is null)
				return IdentityResult.Failed(new IdentityError()
				{
					Code = "404",
					Description = $"Role '{roleName}' does not exist."
				});
			roles.Add(role);
		}
		return await RemoveFromRolesAsync(user, roles, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default)
	{
		UserClaims.Add(new IdentityUserClaim<string>
		{
			UserId = user.Id,
			ClaimType = claim.Type,
			ClaimValue = claim.Value
		});
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "500",
				Description = $"Claim '{claim.Type}:{claim.Value}' couldnot be added to user '{user.UserName}'."
			});
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimIfNotExistsAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await UserClaims.FirstOrDefaultAsync(
			uc => uc.UserId == user.Id && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value,
			cancellationToken);
		return existing is not null ? IdentityResult.Success : await AddClaimAsync(user, claim, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimsAsync(DwtUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		var claimsList = claims is not null ? claims.ToList() : []; // Convert to list to avoid multiple enumerations
		if (claimsList.Count == 0) return IdentityResult.Success;
		foreach (var claim in claimsList)
		{
			UserClaims.Add(new IdentityUserClaim<string>
			{
				UserId = user.Id,
				ClaimType = claim.Type,
				ClaimValue = claim.Value
			});
		}
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "500",
				Description = $"Failed to add claims [{string.Join(", ", claimsList.Select(c => $"{c.Type}:{c.Value}"))}] to user '{user.UserName}'."
			});
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<DwtRole?> GetRoleByIDAsync(string roleId, RoleFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		return await Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<DwtRole?> GetRoleByNameAsync(string roleName, RoleFetchOptions? options = default, CancellationToken cancellationToken = default)
	{
		return await Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> CreateAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		var entry = Roles.Add(role);
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError() { Code = "500", Description = $"Role '{role.Name}' couldnot be created." });
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> CreateIfNotExistsAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		if (await GetRoleByIDAsync(role.Id, cancellationToken: cancellationToken) is not null) return IdentityResult.Success;
		return await CreateAsync(role, cancellationToken: cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		return await RoleClaims
			.Where(rc => rc.RoleId == role.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		RoleClaims.Add(new IdentityRoleClaim<string>
		{
			RoleId = role.Id,
			ClaimType = claim.Type,
			ClaimValue = claim.Value
		});
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "500",
				Description = $"Claim '{claim.Type}:{claim.Value}' couldnot be added to role '{role.Name}'."
			});
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimIfNotExistsAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await RoleClaims.FirstOrDefaultAsync(
			rc => rc.RoleId == role.Id && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value,
			cancellationToken);
		return existing is not null ? IdentityResult.Success : await AddClaimAsync(role, claim, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityResult> AddClaimsAsync(DwtRole role, IEnumerable<Claim> claims, CancellationToken cancellationToken = default)
	{
		var claimsList = claims is not null ? claims.ToList() : []; // Convert to list to avoid multiple enumerations
		if (claimsList.Count == 0) return IdentityResult.Success;
		foreach (var claim in claimsList)
		{
			RoleClaims.Add(new IdentityRoleClaim<string>
			{
				RoleId = role.Id,
				ClaimType = claim.Type,
				ClaimValue = claim.Value
			});
		}
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0
			? IdentityResult.Success
			: IdentityResult.Failed(new IdentityError()
			{
				Code = "500",
				Description = $"Failed to add claims [{string.Join(", ", claimsList.Select(c => $"{c.Type}:{c.Value}"))}] to role '{role.Name}'."
			});
	}
}
