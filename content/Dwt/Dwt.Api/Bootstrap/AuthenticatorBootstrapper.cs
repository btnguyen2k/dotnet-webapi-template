using Dwt.Api.Services;
using Dwt.Shared.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;

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
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<AuthenticatorBootstrapper>();

		logger.LogInformation("Initializing Sample Authenticator...");
		appBuilder.Services.AddSingleton<IAuthenticator, SampleJwtAuthenticator>();
		logger.LogInformation("Sample Authenticator initialized.");
	}
}

/// <summary>
/// JWT implementation of IAuthenticator.
/// </summary>
public sealed class SampleJwtAuthenticator(IOptions<JwtOptions> jwtOptions, IJwtService jwtService, IUserRepository userRepo) : IAuthenticator
{
	private readonly int expirationSeconds = jwtOptions.Value.DefaultExpirationSeconds;

	private string GenerateJwtToken(User user, DateTime expiry)
	{
		var claims = new ClaimsIdentity([
			new Claim(ClaimTypes.Upn, user.Id),
			new Claim(ClaimTypes.Role, user.Role),
			// Add more claims as needed
		]);
		return jwtService.GenerateToken(claims, expiry);
	}

	/// <inheritdoc />
	public AuthResp Authenticate(AuthReq req)
	{
		var user = userRepo.GetByID(req.Id ?? "");
		if (user == null || !user.Authenticate(req.Secret))
		{
			return AuthResp.AuthFailed;
		}
		var expiry = DateTime.Now.AddSeconds(expirationSeconds);
		return AuthResp.New(200, GenerateJwtToken(user, expiry), expiry);
	}

	/// <inheritdoc />
	public AuthResp Refresh(string jwtToken)
	{
		try
		{
			var principal = jwtService.ValidateToken(jwtToken);
			var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
			var user = userRepo.GetByID(claimUserId ?? "");
			if (user == null)
			{
				return AuthResp.AuthFailed;
			}
			var expiry = DateTime.Now.AddSeconds(expirationSeconds);
			return AuthResp.New(200, GenerateJwtToken(user, expiry), expiry);
		}
		catch (Exception e)
		{
			return AuthResp.New(403, e.Message);
		}
	}
}
