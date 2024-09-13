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
	public async Task<DwtUser?> CreateAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		var entity = Users.Add(user);
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entity.Entity : null;
	}

	/// <inheritdoc/>
	public async Task<DwtUser?> CreateIfNotExistsAsync(DwtUser user, CancellationToken cancellationToken = default)
	{
		if (await GetUserByIDAsync(user.Id, cancellationToken: cancellationToken) is not null) return null;
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
	public async Task<DwtRole?> CreateAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		var entity = Roles.Add(role);
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entity.Entity : null;
	}

	/// <inheritdoc/>
	public async Task<DwtRole?> CreateIfNotExistsAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		if (await GetRoleByIDAsync(role.Id, cancellationToken: cancellationToken) is not null) return null;
		return await CreateAsync(role, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<IdentityRoleClaim<string>>> GetClaimsAsync(DwtRole role, CancellationToken cancellationToken = default)
	{
		return await RoleClaims
			.Where(rc => rc.RoleId == role.Id)
			.ToListAsync(cancellationToken) ?? [];
	}

	/*----------------------------------------------------------------------*/

	/// <inheritdoc/>
	public async Task<IdentityUserClaim<string>?> AddClaimAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default)
	{
		var entry = UserClaims.Add(new IdentityUserClaim<string>
		{
			UserId = user.Id,
			ClaimType = claim.Type,
			ClaimValue = claim.Value
		});
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entry.Entity : null;
	}

	/// <inheritdoc/>
	public async Task<IdentityUserClaim<string>?> AddClaimIfNotExistsAsync(DwtUser user, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await UserClaims.FirstOrDefaultAsync(uc => uc.UserId == user.Id && uc.ClaimType == claim.Type && uc.ClaimValue == claim.Value, cancellationToken);
		return existing is not null ? null : await AddClaimAsync(user, claim, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task<IdentityRoleClaim<string>?> AddClaimAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		var entry = RoleClaims.Add(new IdentityRoleClaim<string>
		{
			RoleId = role.Id,
			ClaimType = claim.Type,
			ClaimValue = claim.Value
		});
		var result = await SaveChangesAsync(cancellationToken);
		return result > 0 ? entry.Entity : null;
	}

	/// <inheritdoc/>
	public async Task<IdentityRoleClaim<string>?> AddClaimIfNotExistsAsync(DwtRole role, Claim claim, CancellationToken cancellationToken = default)
	{
		var existing = await RoleClaims.FirstOrDefaultAsync(rc => rc.RoleId == role.Id && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value, cancellationToken);
		return existing is not null ? null : await AddClaimAsync(role, claim, cancellationToken);
	}
}
