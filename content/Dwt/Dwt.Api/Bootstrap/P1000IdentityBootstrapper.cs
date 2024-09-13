using Dwt.Api.Helpers;
using Dwt.Shared.EF.Identity;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Dwt.Api.Bootstrap;

sealed class Commons
{
	public static PasswordOptions passwordOptions = new()
	{
		RequiredLength = 8,
		RequiredUniqueChars = 4,
		RequireDigit = true,
		RequireLowercase = true,
		RequireUppercase = true,
		RequireNonAlphanumeric = false,
	};

	public static ClaimsIdentityOptions claimsIdentityOptions = new()
	{
		EmailClaimType = "email",
		RoleClaimType = "role",
		UserIdClaimType = "uid",
		UserNameClaimType = "uname",
		SecurityStampClaimType = "sec",
	};
}

/// <summary>
/// Built-in bootstrapper that initializes Asp.Net Core Identity services.
/// </summary>
[Bootstrapper]
public class IdentityBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<IdentityBootstrapper>();
		logger.LogInformation("Configuring Identity services...");

		const string confKeyBase = "Databases:Identity";
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
			appBuilder.Services.AddDbContext<IIdentityRepository, IdentityDbContextRepository>(optionsAction);
		else
			appBuilder.Services.AddDbContextPool<IIdentityRepository, IdentityDbContextRepository>(
				optionsAction, dbConf.PoolSize > 0 ? dbConf.PoolSize : DbConf.DEFAULT_POOL_SIZE);

		// https://github.com/dotnet/aspnetcore/issues/26119
		// Use .AddIdentityCore<DwtUser> then add necessary services manually (e.g. AddRoles, AddSignInManager, etc.)
		// instead of using .AddIdentity<DwtUser, DwtRole>
		appBuilder.Services
			//AddIdentity<DwtUser, DwtRole>(opts =>
			//{
			//	opts.Password = Commons.passwordOptions;
			//})
			.AddIdentityCore<DwtUser>(opts =>
			{
				opts.Password = Commons.passwordOptions;
				opts.ClaimsIdentity = Commons.claimsIdentityOptions;
			})
			.AddRoles<DwtRole>()
			.AddSignInManager<SignInManager<DwtUser>>()
			.AddEntityFrameworkStores<IdentityDbContextRepository>()
			;

		// perform initialization tasks in background
		appBuilder.Services.AddHostedService<IdentityInitializer>();
	}
}

sealed class IdentityInitializer(
	IServiceProvider serviceProvider,
	ILogger<IdentityInitializer> logger,
	IWebHostEnvironment environment) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Initializing identity data...");

		using (var scope = serviceProvider.CreateScope())
		{
			var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>() as IdentityDbContextRepository
				?? throw new InvalidOperationException("Identity repository is not an instance of IdentityDbContextRepository.");
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				identityRepo.Database.EnsureCreated();
			}

			logger.LogInformation("Ensuring roles exist...");
			foreach (var r in DwtRole.ALL_ROLES)
			{
				await identityRepo.CreateIfNotExistsAsync(r, cancellationToken: cancellationToken);
			}

			logger.LogInformation("Ensuring permissions setup...");

			// permissions setup for role ACCOUNT_ADMIN
			var roleAccountAdmin = await identityRepo.GetRoleByIDAsync(DwtRole.ACCOUNT_ADMIN.Id, cancellationToken: cancellationToken)
				?? throw new InvalidOperationException($"Role '{DwtRole.ACCOUNT_ADMIN.Id}' does not exist.");
			foreach (var claim in new Claim[] { DwtIdentity.CLAIM_PERM_CREATE_USER })
			{
				await identityRepo.AddClaimIfNotExistsAsync(roleAccountAdmin, claim, cancellationToken: cancellationToken);
			}

			// permissions setup for role APP_ADMIN
			var roleAppAdmin = await identityRepo.GetRoleByIDAsync(DwtRole.APP_ADMIN.Id, cancellationToken: cancellationToken)
				?? throw new InvalidOperationException($"Role '{DwtRole.APP_ADMIN.Id}' does not exist.");
			foreach (var claim in new Claim[] { DwtIdentity.CLAIM_PERM_CREATE_APP, DwtIdentity.CLAIM_PERM_DELETE_APP, DwtIdentity.CLAIM_PERM_MODIFY_APP })
			{
				await identityRepo.AddClaimIfNotExistsAsync(roleAppAdmin, claim, cancellationToken: cancellationToken);
			}

			logger.LogInformation("Ensuring admin user exist...");
			var userAdmin = await identityRepo.GetUserByIDAsync("admin", cancellationToken: cancellationToken);
			if (userAdmin == null)
			{
				var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>()?.Value;
				var generatedPassword = GenerateRandomPassword(identityOptions?.Password);
				logger.LogWarning("Admin user does not exist. Creating one with a random password: {password}", generatedPassword);
				logger.LogWarning("PLEASE REMEMBER THIS PASSWORD AS IT WILL NOT BE DISPLAYED AGAIN!");

				userAdmin = new DwtUser { Id = "admin", UserName = "admin@local", Email = "admin@local" };
				await identityRepo.CreateIfNotExistsAsync(userAdmin, cancellationToken: cancellationToken);
			}
		}
	}

	static class RandomGenerator
	{
		private static readonly RandomNumberGenerator _random = RandomNumberGenerator.Create();

		public static int Next(int minValue, int maxValue)
		{
			if (minValue > maxValue)
			{
				throw new ArgumentOutOfRangeException(nameof(minValue), $"{nameof(minValue)} must be less than or equal to {nameof(maxValue)}");
			}
			if (minValue == maxValue)
			{
				return minValue;
			}
			var data = new byte[sizeof(int)];
			_random.GetBytes(data);
			var value = BitConverter.ToInt32(data, 0);
			return Math.Abs(value % (maxValue - minValue)) + minValue;
		}
	}

	/// <summary>
	/// Generates a Random Password respecting the given strength requirements.
	/// </summary>
	/// <param name="opts">A valid PasswordOptions objectcontaining the password strength requirements.</param>
	/// <returns>A random password</returns>
	/// <remarks>https://www.ryadel.com/en/c-sharp-random-password-generator-asp-net-core-mvc/</remarks>
	static string GenerateRandomPassword(PasswordOptions? opts)
	{
		opts ??= Commons.passwordOptions;

		var randomChars = new[] {
			"ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase
			"abcdefghijkmnopqrstuvwxyz",    // lowercase
			"0123456789",                   // digits
			"!@$?_-"                        // non-alphanumeric
		};
		var chars = new List<char>();
		if (opts.RequireUppercase)
		{
			chars.Insert(RandomGenerator.Next(0, chars.Count), randomChars[0][RandomGenerator.Next(0, randomChars[0].Length)]);
		}

		if (opts.RequireLowercase)
		{
			chars.Insert(RandomGenerator.Next(0, chars.Count), randomChars[1][RandomGenerator.Next(0, randomChars[1].Length)]);
		}

		if (opts.RequireDigit)
		{
			chars.Insert(RandomGenerator.Next(0, chars.Count), randomChars[2][RandomGenerator.Next(0, randomChars[2].Length)]);
		}

		if (opts.RequireNonAlphanumeric)
		{
			chars.Insert(RandomGenerator.Next(0, chars.Count), randomChars[3][RandomGenerator.Next(0, randomChars[3].Length)]);
		}

		for (var i = chars.Count; i < opts.RequiredLength || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
		{
			var rcs = randomChars[RandomGenerator.Next(0, opts.RequireNonAlphanumeric ? randomChars.Length : randomChars.Length - 1)];
			chars.Insert(RandomGenerator.Next(0, chars.Count), rcs[RandomGenerator.Next(0, rcs.Length)]);
		}

		return new string(chars.ToArray());
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
