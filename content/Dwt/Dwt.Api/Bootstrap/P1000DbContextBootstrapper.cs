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

		appBuilder.Services.AddDbContext<IApplicationRepository, ApplicationDbContextRepository>(options =>
		{
			if (appBuilder.Environment.IsDevelopment())
			{
				options.EnableDetailedErrors().EnableSensitiveDataLogging();
			}

			const string CONF_DB_TYPE = "DatabaseTypes:Application";
			Enum.TryParse<DbType>(appBuilder.Configuration[CONF_DB_TYPE], true, out var dbType);
			if (dbType == DbType.NULL)
			{
				logger.LogWarning("No value found at key {conf} in the configurations. Defaulting to INMEMORY.", CONF_DB_TYPE);
				dbType = DbType.INMEMORY;
			}

			var connStr = appBuilder.Configuration.GetConnectionString("ApplicationDbContext") ?? "";
			switch (dbType)
			{
				case DbType.INMEMORY or DbType.MEMORY:
					options.UseInMemoryDatabase("DwtApplication");
					break;
				case DbType.SQLITE:
					options.UseSqlite(connStr);
					break;
				case DbType.SQLSERVER:
					options.UseSqlServer(connStr);
					break;
				default:
					throw new InvalidDataException($"Invalid value at key {CONF_DB_TYPE} in the configurations: '{dbType}'.");
			}
		});

		appBuilder.Services.AddHostedService<ApplicationInitializer>();
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
