using Dwt.Api.Helpers;
using Dwt.Shared.EF;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes DbContext/DbContextPool services.
/// </summary>
[Bootstrapper]
public class DbContextBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DbContextBootstrapper>();
		logger.LogInformation("Configuring DbContext services...");

		ConfigureDbContext<IApplicationRepository, ApplicationDbContextRepository>(appBuilder, "Databases:Application", logger);
		appBuilder.Services.AddHostedService<ApplicationInitializer>();
	}

	private static void ConfigureDbContext<TContextService, TContextImplementation>(
		WebApplicationBuilder appBuilder,
		string confKeyBase,
		ILogger logger) where TContextService : class where TContextImplementation : DbContext, TContextService
	{
		var dbConf = appBuilder.Configuration.GetSection(confKeyBase).Get<DbConf>()
			?? throw new InvalidDataException($"No configuration found at key {confKeyBase} in the configurations.");
		void optionsAction(DbContextOptionsBuilder options)
		{
			if (appBuilder.Environment.IsDevelopment())
			{
				options.EnableDetailedErrors().EnableSensitiveDataLogging();
			}
			if (dbConf.Type == DbType.NULL)
			{
				logger.LogWarning("Unknown value at key {conf} in the configurations. Defaulting to INMEMORY.", $"{confKeyBase}:Type");
				dbConf.Type = DbType.INMEMORY;
			}

			var connStr = appBuilder.Configuration.GetConnectionString(dbConf.ConnectionString) ?? "";
			switch (dbConf.Type)
			{
				case DbType.INMEMORY or DbType.MEMORY:
					options.UseInMemoryDatabase(confKeyBase);
					break;
				case DbType.SQLITE or DbType.SQLSERVER:
					if (string.IsNullOrWhiteSpace(dbConf.ConnectionString))
					{
						throw new InvalidDataException($"No connection string name found at key {confKeyBase}:ConnectionString in the configurations.");
					}
					if (string.IsNullOrWhiteSpace(connStr))
					{
						throw new InvalidDataException($"No connection string {dbConf.ConnectionString} defined in the ConnectionStrings section in the configurations.");
					}

					if (appBuilder.Environment.IsDevelopment())
					{
						logger.LogError("Using connection string {connStr} for {dbType} database.", connStr, dbConf.Type);
					}

					if (dbConf.Type == DbType.SQLITE)
						options.UseSqlite(connStr);
					else if (dbConf.Type == DbType.SQLSERVER)
						options.UseSqlServer(connStr);
					break;
				default:
					throw new InvalidDataException($"Invalid value at key {confKeyBase}:Type in the configurations: '{dbConf.Type}'");
			}
		}
		if (dbConf.UseDbContextPool)
			appBuilder.Services.AddDbContext<TContextService, TContextImplementation>(optionsAction);
		else
			appBuilder.Services.AddDbContextPool<TContextService, TContextImplementation>(
				optionsAction, dbConf.PoolSize > 0 ? dbConf.PoolSize : DbConf.DEFAULT_POOL_SIZE);
	}
}

sealed class ApplicationInitializer(
	IServiceProvider serviceProvider,
	ILogger<ApplicationInitializer> logger,
	IWebHostEnvironment environment) : IHostedService
{
	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Initializing application data...");

		using (var scope = serviceProvider.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationRepository>() as ApplicationDbContextRepository;
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				dbContext!.Database.EnsureCreated();
			}
		}

		return Task.CompletedTask;
	}
}
