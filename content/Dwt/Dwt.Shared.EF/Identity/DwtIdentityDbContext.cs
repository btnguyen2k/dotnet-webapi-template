using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dwt.Shared.EF.Identity;
public class DwtIdentityDbContext : IdentityDbContext<DwtUser, DwtRole, string>
{
	//private readonly ILogger<DwtIdentityDbContext> logger;

	public DwtIdentityDbContext(DbContextOptions<DwtIdentityDbContext> options/*, ILogger<DwtIdentityDbContext> logger*/) : base(options)
	{
		//this.logger = logger;
		//logger.LogCritical("DwtIdentityDbContext instances created.");
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
}

/* Demostration of how to use the EntityTypeConfiguration classes to customize table and column names */

sealed class Commons
{
	public const string TABLE_PREFIX = "dwt_";
}

sealed class RoleEntityTypeConfiguration : IEntityTypeConfiguration<DwtRole>
{
	public void Configure(EntityTypeBuilder<DwtRole> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}roles");
		builder.Property(t => t.Id).HasColumnName("role_id");
		builder.Property(t => t.Name).HasColumnName("role_name");
		builder.Property(t => t.NormalizedName).HasColumnName("role_normalized_name");
		builder.Property(t => t.ConcurrencyStamp).HasColumnName("role_concurrency_stamp");
	}
}

sealed class IdentityRoleClaimEntityTypeConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
{
	public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}role_claims");
		builder.Property(t => t.Id).HasColumnName("rc_id");
		builder.Property(t => t.RoleId).HasColumnName("role_id");
		builder.Property(t => t.ClaimType).HasColumnName("claim_type");
		builder.Property(t => t.ClaimValue).HasColumnName("claim_value");

		builder.HasIndex(t => new { t.RoleId, t.ClaimType, t.ClaimValue }).IsUnique();
	}
}

sealed class IdentityUserEntityTypeConfiguration : IEntityTypeConfiguration<DwtUser>
{
	public void Configure(EntityTypeBuilder<DwtUser> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}users");
		builder.Property(t => t.Id).HasColumnName("uid");
		builder.Property(t => t.UserName).HasColumnName("uname");
		builder.Property(t => t.NormalizedUserName).HasColumnName("normalized_name");
		builder.Property(t => t.Email).HasColumnName("uemail");
		builder.Property(t => t.NormalizedEmail).HasColumnName("normalized_email");
		//builder.Property(t => t.EmailConfirmed).HasColumnName("email_confirmed");
		builder.Property(t => t.PasswordHash).HasColumnName("password_hash");
		builder.Property(t => t.SecurityStamp).HasColumnName("security_stamp");
		builder.Property(t => t.ConcurrencyStamp).HasColumnName("concurrency_stamp");
		//builder.Property(t => t.PhoneNumber).HasColumnName("phone_number");
		//builder.Property(t => t.PhoneNumberConfirmed).HasColumnName("phone_number_confirmed");
		//builder.Property(t => t.TwoFactorEnabled).HasColumnName("two_factor_enabled");
		//builder.Property(t => t.LockoutEnd).HasColumnName("lockout_end");
		//builder.Property(t => t.LockoutEnabled).HasColumnName("lockout_enabled");
		//builder.Property(t => t.AccessFailedCount).HasColumnName("access_failed_count");

		// username and email should be unique
		builder.HasIndex(t => t.NormalizedUserName).IsUnique();
		builder.HasIndex(t => t.NormalizedEmail).IsUnique();

		builder.Ignore(t => t.EmailConfirmed);
		builder.Ignore(t => t.PhoneNumber);
		builder.Ignore(t => t.PhoneNumberConfirmed);
		builder.Ignore(t => t.TwoFactorEnabled);
		builder.Ignore(t => t.LockoutEnd);
		builder.Ignore(t => t.LockoutEnabled);
		builder.Ignore(t => t.AccessFailedCount);
	}
}

sealed class IdentityUserClaimEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserClaim<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserClaim<string>> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}user_claims");
		builder.Property(t => t.Id).HasColumnName("uc_id");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.ClaimType).HasColumnName("claim_type");
		builder.Property(t => t.ClaimValue).HasColumnName("claim_value");

		builder.HasIndex(t => new { t.UserId, t.ClaimType, t.ClaimValue }).IsUnique();
	}
}

sealed class IdentityUserLoginEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserLogin<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserLogin<string>> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}user_logins");
		builder.Property(t => t.LoginProvider).HasColumnName("login_provider");
		builder.Property(t => t.ProviderKey).HasColumnName("provider_key");
		builder.Property(t => t.ProviderDisplayName).HasColumnName("provider_display_name");
		builder.Property(t => t.UserId).HasColumnName("user_id");
	}
}

sealed class IdentityUserRoleEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}user_roles");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.RoleId).HasColumnName("role_id");
	}
}

sealed class IdentityUserTokenEntityTypeConfiguration : IEntityTypeConfiguration<IdentityUserToken<string>>
{
	public void Configure(EntityTypeBuilder<IdentityUserToken<string>> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}user_tokens");
		builder.Property(t => t.UserId).HasColumnName("user_id");
		builder.Property(t => t.LoginProvider).HasColumnName("login_provider");
		builder.Property(t => t.Name).HasColumnName("token_name");
		builder.Property(t => t.Value).HasColumnName("token_value");
	}
}
