using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Sample bootstrapper that initializes an IAuthenticator service.
/// </summary>
/// <remarks>
///		This bootstrapper initializes and registers an IAuthenticator service with the DI container.
/// </remarks>
[Bootstrapper]
public class AuthenticatorBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder, IOptions<JwtOptions> jwtOptions)
	{
		//var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AuthenticatorBootstrapper>();

		// use JwtBearer authentication scheme
		appBuilder.Services.AddAuthentication()
		.AddJwtBearer(options =>
		{
			//logger.LogCritical("AddJwtBearer: {options}", JsonSerializer.Serialize(options));

			options.SaveToken = true;
			options.RequireHttpsMetadata = false;
			options.TokenValidationParameters = jwtOptions.Value.TokenValidationParameters;
		});

		// Customize the behavior of the authorization middleware.
		appBuilder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SampleAuthorizationMiddlewareResultHandler>();

		// setup IAuthenticator/IAuthenticatorAsync services
		appBuilder.Services.AddSingleton<SampleJwtAuthenticator>()
			.AddSingleton<IAuthenticator>(x => x.GetRequiredService<SampleJwtAuthenticator>())
			.AddSingleton<IAuthenticatorAsync>(x => x.GetRequiredService<SampleJwtAuthenticator>());
	}

	public static void DecorateApp(WebApplication app)
	{
		// activate authentication/authorization middleware
		app.UseAuthentication();
		app.UseAuthorization();
	}
}

public class SampleAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
	private readonly byte[] unauthorizedResult = JsonSerializer.SerializeToUtf8Bytes(new
	{
		status = StatusCodes.Status401Unauthorized,
		message = "Unauthorized"
	});

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		if (!authorizeResult.Succeeded)
		{
			if (authorizeResult.Challenged) await context.ChallengeAsync();
			else if (authorizeResult.Forbidden) await context.ForbidAsync();

			//var logger = context.RequestServices.GetRequiredService<ILogger<SampleAuthorizationMiddlewareResultHandler>>();
			//logger.LogCritical("AuthResult: {result}", JsonSerializer.Serialize(authorizeResult));
			//logger.LogCritical("Policy: {policy}", JsonSerializer.Serialize(policy));
			//logger.LogCritical("Items: {items}", JsonSerializer.Serialize(context.Items));

			context.Response.ContentType = "application/json";
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.BodyWriter.WriteAsync(unauthorizedResult);
			return;
		}

		// Fall back to the default implementation.
		await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}

/// <summary>
/// JWT implementation of IAuthenticator.
/// </summary>
public sealed class SampleJwtAuthenticator(
	IServiceProvider serviceProvider,
	IOptions<JwtOptions> jwtOptions,
	IJwtService jwtService,
	IOptions<IdentityOptions> identityOptions,
	ILogger<SampleJwtAuthenticator> logger) : IAuthenticator, IAuthenticatorAsync
{
	private readonly int expirationSeconds = jwtOptions.Value.DefaultExpirationSeconds;
	private readonly string claimTypeUserId = identityOptions.Value.ClaimsIdentity.UserIdClaimType;
	private readonly string claimTypeUsername = identityOptions.Value.ClaimsIdentity.UserNameClaimType;
	private readonly string claimTypeRole = identityOptions.Value.ClaimsIdentity.RoleClaimType;

	private async Task<string> GenerateJwtToken(UserManager<DwtUser> userManager, DwtUser user, DateTime expiry)
	{
		var authClaims = new List<Claim>
			{
				new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
				new(claimTypeUsername, user.UserName ?? ""),
new(claimTypeUserId, user.Id),
			};

		var userRoles = await userManager.GetRolesAsync(user);
		authClaims.AddRange(userRoles.Select(role => new Claim(claimTypeRole, role)));

		// Add more claims as needed

		return jwtService.GenerateToken([.. authClaims], expiry);
	}

	/// <inheritdoc />
	public async Task<AuthResp> AuthenticateAsync(AuthReq req)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DwtUser>>();
			var user = await userManager.FindByNameAsync(req?.Id ?? "");
			if (user == null)
			{
				logger.LogError("Authentication failed: user '{username}' not found.", req?.Id);
				return AuthResp.AuthFailed;
			}
			var signinManager = scope.ServiceProvider.GetRequiredService<SignInManager<DwtUser>>();
			var result = await signinManager.CheckPasswordSignInAsync(user, req?.Secret ?? "", false);
			//var result = await signinManager.PasswordSignInAsync(user, req?.Secret ?? "", false, false);
			if (!result.Succeeded)
			{
				logger.LogError("Authentication failed: {error}", result.ToString());
				return AuthResp.AuthFailed;
			}
			var expiry = DateTime.Now.AddSeconds(expirationSeconds);
			return AuthResp.New(200, await GenerateJwtToken(userManager, user, expiry), expiry);
		}
	}


	/// <inheritdoc />
	public AuthResp Authenticate(AuthReq req)
	{
		return AuthenticateAsync(req).Result;
	}

	/// <inheritdoc />
	public async Task<AuthResp> RefreshAsync(string jwtToken)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken);
			var claimUsername = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUsername)?.Value;
			using (var scope = serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DwtUser>>();
				var user = await userManager.FindByNameAsync(claimUsername ?? "");
				if (user == null)
				{
					logger.LogError("AuthToken refreshing failed: user '{user}' not found.", claimUsername);
					return AuthResp.AuthFailed;
				}
				var expiry = DateTime.Now.AddSeconds(expirationSeconds);
				return AuthResp.New(200, await GenerateJwtToken(userManager, user, expiry), expiry);
			}
		}
		catch (Exception e) when (e is ArgumentException || e is SecurityTokenException)
		{
			return AuthResp.New(403, e.Message);
		}
	}

	/// <inheritdoc />
	public AuthResp Refresh(string jwtToken)
	{
		return RefreshAsync(jwtToken).Result;
	}
}
