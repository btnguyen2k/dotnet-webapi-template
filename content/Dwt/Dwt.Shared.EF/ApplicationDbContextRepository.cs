using Dwt.Shared.Cache;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;

namespace Dwt.Shared.EF;
public sealed class ApplicationDbContextRepository
	: GenericDbContextRepository<ApplicationDbContextRepository, Application, string>, IApplicationRepository
{
	private readonly ICacheFacade<IApplicationRepository>? cache;
	private readonly ILogger<ApplicationDbContextRepository> logger;

	public ApplicationDbContextRepository(
		DbContextOptions<ApplicationDbContextRepository> options,
		ILogger<ApplicationDbContextRepository> logger,
		ICacheFacade<IApplicationRepository>? cache = null) : base(options)
	{
		this.cache = cache;
		this.logger = logger;

		ChangeTracker.StateChanged += async (sender, args) =>
		{
			logger.LogDebug("State changed - {entity}: {state}", args.Entry.Entity.GetType().FullName, args.Entry.State);
			if (args.Entry.Entity is Application app)
			{
				switch (args.Entry.State)
				{
					case EntityState.Added or EntityState.Modified or EntityState.Unchanged:
						if (args.Entry.State != EntityState.Unchanged)
						{
							app.Touch();
						}
						if (cache != null)
							await cache.SetAsync(app.Id, app, default!);
						break;
					default:
						if (cache != null)
							await cache.RemoveAsync(app.Id);
						break;
				}
			}
		};
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		new ApplicationEntityTypeConfiguration().Configure(modelBuilder.Entity<Application>());
	}

	private Application CacheHit(Application cached)
	{
		var app = Attach(cached).Entity;
		logger.LogDebug("Cache hit: {id} / {count}", cached.Id, ChangeTracker.Entries().Count());
		return app;
	}

	private async Task<Application?> CacheMissAsync(Application? item)
	{
		if (item == null) return null;
		if (cache != null)
		{
			logger.LogDebug("Cache miss: {id}", item.Id);
			await cache.SetAsync(item.Id, item, default!);
		}
		return item;
	}

	/// <inheritdoc/>
	public override Application? GetByID(string id)
	{
		var cached = cache?.Get<Application>(id);
		if (cached != null)
		{
			return CacheHit(cached);
		}

		var result = base.GetByID(id);
		return CacheMissAsync(result).Result;
	}

	/// <inheritdoc/>
	public override async ValueTask<Application?> GetByIDAsync(string id)
	{
		var cached = cache != null ? await cache.GetAsync<Application>(id) : null;
		if (cached != null)
		{
			return CacheHit(cached);
		}

		var result = await base.GetByIDAsync(id);
		return await CacheMissAsync(result);
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
