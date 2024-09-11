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
		const string CONF_DB_TYPE = "DatabaseTypes:Identity";
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<IdentityBootstrapper>();
		logger.LogInformation("Configuring Identity services...");

		Enum.TryParse<DbType>(appBuilder.Configuration[CONF_DB_TYPE], true, out var dbType);
		if (dbType == DbType.NULL)
		{
			logger.LogWarning("No value found at key {conf} in the configurations. Defaulting to INMEMORY.", CONF_DB_TYPE);
			dbType = DbType.INMEMORY;
		}

		appBuilder.Services.AddDbContext<DwtIdentityDbContext>(options =>
		{
			if (appBuilder.Environment.IsDevelopment())
			{
				options.EnableDetailedErrors().EnableSensitiveDataLogging();
			}
			var connStr = appBuilder.Configuration.GetConnectionString("IdentityDbContext") ?? "";
			switch (dbType)
			{
				case DbType.INMEMORY or DbType.MEMORY:
					options.UseInMemoryDatabase("DwtIdentity");
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
			.AddEntityFrameworkStores<DwtIdentityDbContext>()
			;

		appBuilder.Services.AddHostedService<IdentityInitializer>();
	}
}

sealed class IdentityInitializer(
	IServiceProvider serviceProvider,
	ILogger<IdentityInitializer> logger,
	IWebHostEnvironment environment) : IHostedService
{
	static void ThrowsIfNotSucceeded(IdentityResult result, ILogger logger)
	{
		if (!result.Succeeded)
		{
			foreach (var error in result.Errors)
			{
				logger.LogError("Failed to execute DB operation: {code} - {description}", error.Code, error.Description);
				throw new InvalidOperationException($"{error.Code} - {error.Description}");
			}
		}
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Initializing identity data...");

		using (var scope = serviceProvider.CreateScope())
		{
			var dbContext = scope.ServiceProvider.GetRequiredService<DwtIdentityDbContext>();
			var tryParseInitDb = bool.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_INIT_DB), out var initDb);
			if (environment.IsDevelopment() || (tryParseInitDb && initDb))
			{
				logger.LogInformation("Ensuring database schema exist...");
				dbContext.Database.EnsureCreated();
			}

			logger.LogInformation("Ensuring roles exist...");
			var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<DwtRole>>();
			foreach (var r in DwtRole.ALL_ROLES)
			{
				if (await roleManager.FindByIdAsync(r.Id) == null)
				{
					ThrowsIfNotSucceeded(await roleManager.CreateAsync(r), logger);
				}
			}

			logger.LogInformation("Ensuring permissions setup...");
			var role = await roleManager.FindByIdAsync(DwtRole.ACCOUNT_ADMIN.Id);
			if (role != null)
			{
				// permissions setup for role ACCOUNT_ADMIN
				var claims = await roleManager.GetClaimsAsync(role);
				var expectedClaims = new Claim[] { DwtIdentity.CLAIM_PERM_CREATE_USER };
				foreach (var expectedClaim in expectedClaims)
				{
					if (!claims.Contains(expectedClaim, ClaimEqualityComparer.Instance))
					{
						ThrowsIfNotSucceeded(await roleManager.AddClaimAsync(role, expectedClaim), logger);
					}
				}
			}
			role = await roleManager.FindByIdAsync(DwtRole.APP_ADMIN.Id);
			if (role != null)
			{
				// permissions setup for role APP_ADMIN
				var claims = await roleManager.GetClaimsAsync(role);
				var expectedClaims = new Claim[] { DwtIdentity.CLAIM_PERM_CREATE_APP, DwtIdentity.CLAIM_PERM_DELETE_APP, DwtIdentity.CLAIM_PERM_MODIFY_APP };
				foreach (var expectedClaim in expectedClaims)
				{
					if (!claims.Contains(expectedClaim, ClaimEqualityComparer.Instance))
					{
						ThrowsIfNotSucceeded(await roleManager.AddClaimAsync(role, expectedClaim), logger);
					}
				}
			}

			logger.LogInformation("Ensuring admin user exist...");
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DwtUser>>();
			var adminUser = await userManager.FindByIdAsync("admin");
			if (adminUser == null)
			{
				var identityOptions = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>()?.Value;
				var generatedPassword = GenerateRandomPassword(identityOptions?.Password);
				logger.LogWarning("Admin user does not exist. Creating one with a random password: {password}", generatedPassword);
				logger.LogWarning("PLEASE REMEMBER THIS PASSWORD AS IT WILL NOT BE DISPLAYED AGAIN!");

				adminUser = new DwtUser { Id = "admin", UserName = "admin@local", Email = "admin@local" };
				ThrowsIfNotSucceeded(await userManager.CreateAsync(adminUser, generatedPassword), logger);
				ThrowsIfNotSucceeded(await userManager.AddToRoleAsync(adminUser, DwtRole.ADMIN.Name!), logger);
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
