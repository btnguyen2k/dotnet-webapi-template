using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dwt.Shared.EF;
public sealed class ApplicationDbContextRepository(DbContextOptions<ApplicationDbContextRepository> options)
	: GenericDbContextRepository<ApplicationDbContextRepository, Application, string>(options), IApplicationRepository
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		new ApplicationEntityTypeConfiguration().Configure(modelBuilder.Entity<Application>());
	}
}

sealed class ApplicationEntityTypeConfiguration : IEntityTypeConfiguration<Application>
{
	public void Configure(EntityTypeBuilder<Application> builder)
	{
		builder.ToTable($"{Commons.TABLE_PREFIX}apps");
		builder.Property(t => t.Id).HasColumnName("app_id").HasMaxLength(64);
		builder.Property(t => t.DisplayName).HasColumnName("display_name").HasMaxLength(128);
		builder.Property(t => t.PublicKeyPEM).HasColumnName("public_key_pem");
		builder.Property(t => t.CreationTime).HasColumnName("creation_time");
		builder.Property(t => t.ConcurrencyStamp).HasColumnName("concurrency_stamp").IsConcurrencyToken().HasMaxLength(64);

		builder.HasKey(t => t.Id);
	}
}
