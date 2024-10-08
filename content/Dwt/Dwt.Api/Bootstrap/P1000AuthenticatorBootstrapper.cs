﻿using Dwt.Api.Services;
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
		// use JwtBearer authentication scheme
		appBuilder.Services.AddAuthentication()
		.AddJwtBearer(options =>
		{
			options.SaveToken = true;
			options.RequireHttpsMetadata = false;
			options.TokenValidationParameters = jwtOptions.Value.TokenValidationParameters;
		});

		// Customize the behavior of the authorization middleware.
		appBuilder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, SampleAuthorizationMiddlewareResultHandler>();

		// Configurate authorization policies
		appBuilder.Services.AddAuthorizationBuilder()
			.AddPolicy(DwtIdentity.POLICY_NAME_ADMIN_OR_CREATE_ACCOUNT_PERM, DwtIdentity.POLICY_ADMIN_OR_CREATE_ACCOUNT_PERM)
			.AddPolicy(DwtIdentity.POLICY_NAME_ADMIN_OR_CREATE_APP_PERM, DwtIdentity.POLICY_ADMIN_OR_CREATE_APP_PERM)
			.AddPolicy(DwtIdentity.POLICY_NAME_ADMIN_OR_DELETE_APP_PERM, DwtIdentity.POLICY_ADMIN_OR_DELETE_APP_PERM)
			.AddPolicy(DwtIdentity.POLICY_NAME_ADMIN_OR_MODIFY_APP_PERM, DwtIdentity.POLICY_ADMIN_OR_MODIFY_APP_PERM);

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
	private const string DefaultSecValue = "00000000";

	private async Task<string> GenerateJwtToken(IIdentityRepository identityRepo, DwtUser user, DateTime expiry)
	{
		var authClaims = new List<Claim>
		{
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(claimTypeUsername, user.UserName ?? ""),
			new(claimTypeUserId, user.Id),
			new("sec", user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8) ?? DefaultSecValue)
		};

		var userRoles = await identityRepo.GetRolesAsync(user);
		authClaims.AddRange(userRoles.Select(role => new Claim(claimTypeRole, role.Name!)));

		// Add more claims as needed

		return jwtService.GenerateToken([.. authClaims], expiry);
	}

	/// <inheritdoc />
	public async Task<AuthResp> AuthenticateAsync(AuthReq req)
	{
		using (var scope = serviceProvider.CreateScope())
		{
			var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
			var user = req.Id != null ? await identityRepo.GetUserByUserNameAsync(req.Id) : null;
			if (user == null)
			{
				logger.LogError("Authentication failed: user '{username}' not found.", req?.Id);
				return AuthResp.AuthFailed;
			}
			var pwdHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<DwtUser>>();
			var pwdHashResult = pwdHasher.VerifyHashedPassword(user, user.PasswordHash!, req?.Secret ?? "");
			if (pwdHashResult != PasswordVerificationResult.Success && pwdHashResult != PasswordVerificationResult.SuccessRehashNeeded)
			{
				logger.LogError("Authentication failed: password verification failed.");
				return AuthResp.AuthFailed;
			}
			var expiry = DateTime.Now.AddSeconds(expirationSeconds);
			return AuthResp.New(200, await GenerateJwtToken(identityRepo, user, expiry), expiry);
		}
	}

	/// <inheritdoc />
	public AuthResp Authenticate(AuthReq req)
	{
		return AuthenticateAsync(req).Result;
	}

	/// <inheritdoc />
	public async Task<AuthResp> RefreshAsync(string jwtToken, bool ignoreTokenSecurityCheck = false)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken);
			var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserId)?.Value;
			var claimUsername = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUsername)?.Value;
			using (var scope = serviceProvider.CreateScope())
			{
				var identityRepo = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
				var user = claimUserId != null
					? await identityRepo.GetUserByIDAsync(claimUserId)
					: claimUsername != null
						? await identityRepo.GetUserByUserNameAsync(claimUsername)
						: null;
				if (user == null)
				{
					logger.LogError("AuthToken refreshing failed: user '{user}' not found.",
						claimUserId != null ? $"id:{claimUserId}" : $"name:{claimUsername}");
					return AuthResp.New(403, "User not found.");
				}
				if (!ignoreTokenSecurityCheck)
				{
					var claimSec = principal.Claims.FirstOrDefault(c => c.Type == "sec")?.Value ?? DefaultSecValue;
					if (claimSec != user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8))
					{
						logger.LogError("AuthToken refreshing failed: invalid security stamp.");
						return AuthResp.New(403, "Invalid security stamp.");
					}
				}
				await identityRepo.UpdateSecurityStampAsync(user); // new token should invalidate all previous tokens
				var expiry = DateTime.Now.AddSeconds(expirationSeconds);
				return AuthResp.New(200, await GenerateJwtToken(identityRepo, user, expiry), expiry);
			}
		}
		catch (Exception e) when (e is ArgumentException || e is SecurityTokenException)
		{
			return AuthResp.New(403, e.Message);
		}
	}

	/// <inheritdoc />
	public AuthResp Refresh(string jwtToken, bool ignoreTokenSecurityCheck = false)
	{
		return RefreshAsync(jwtToken, ignoreTokenSecurityCheck).Result;
	}

	/// <inheritdoc />
	public async Task<TokenValidationResp> ValidateAsync(string jwtToken)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken);
			using (var scope = serviceProvider.CreateScope())
			{
				var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DwtUser>>();
				var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUserId)?.Value;
				var claimUsername = principal.Claims.FirstOrDefault(c => c.Type == claimTypeUsername)?.Value;
				var user = claimUserId != null
					? await userManager.FindByIdAsync(claimUserId)
					: claimUsername != null
						? await userManager.FindByNameAsync(claimUsername)
						: null;
				if (user == null)
				{
					return new TokenValidationResp { Status = 404, Error = "User not found." };
				}
				var claimSec = principal.Claims.FirstOrDefault(c => c.Type == "sec")?.Value ?? DefaultSecValue;
				if (claimSec != user.SecurityStamp?.Substring(user.SecurityStamp.Length - 8))
				{
					return new TokenValidationResp { Status = 403, Error = "Invalid security stamp." };
				}
				return new TokenValidationResp { Status = 200, Principal = principal };
			}
		}
		catch (Exception e) when (e is ArgumentException || e is SecurityTokenException)
		{
			return new TokenValidationResp { Status = 403, Error = e.Message };
		}
	}

	/// <inheritdoc />
	public TokenValidationResp Validate(string jwtToken)
	{
		return ValidateAsync(jwtToken).Result;
	}
}
