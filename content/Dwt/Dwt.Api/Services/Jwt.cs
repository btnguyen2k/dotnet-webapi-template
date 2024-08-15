using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Dwt.Api.Services;

public class JwtOptions
{
	/// <summary>
	/// The security key to sign and verify JWT tokens.
	/// </summary>
	public SecurityKey Key { get; set; } = default!;

	/// <summary>
	/// The algorithm to use for signing and verifying JWT tokens.
	/// </summary>
	public string Algorithm { get; set; } = default!;

	/// <summary>
	/// The issuer to use for JWT tokens.
	/// </summary>
	public string Issuer { get; set; } = default!;

	/// <summary>
	/// The audience to use for JWT tokens.
	/// </summary>
	public string Audience { get; set; } = default!;

	/// <summary>
	/// The default expiration time for JWT tokens in seconds.
	/// </summary>
	public int DefaultExpirationSeconds { get; set; } = 3600;
}

public interface IJwtService
{
	/// <summary>
	/// Generates a JWT token with default expiration.
	/// </summary>
	/// <param name="claims"></param>
	/// <returns></returns>
	public string GenerateToken(Claim[] claims);

	/// <summary>
	/// Generates a JWT token with specified expiration.
	/// </summary>
	/// <param name="claims"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public string GenerateToken(Claim[] claims, DateTime? expiry);

	/// <summary>
	/// Generate a JWT token with default expiration.
	/// </summary>
	/// <param name="subject"></param>
	/// <returns></returns>
	public string GenerateToken(ClaimsIdentity subject);

	/// <summary>
	/// Generates a JWT token with specified expiration.
	/// </summary>
	/// <param name="subject"></param>
	/// <param name="expiry"></param>
	/// <returns></returns>
	public string GenerateToken(ClaimsIdentity subject, DateTime? expiry);

	/// <summary>
	/// Validates a JWT token using the default validation parameters.
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public ClaimsPrincipal ValidateToken(string token);

	/// <summary>
	/// Validates a JWT token using the specified validation parameters.
	/// </summary>
	/// <param name="token"></param>
	/// <param name="validationParameters"></param>
	/// <returns></returns>
	public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters);
}

public class JwtService : IJwtService
{
	private readonly JwtOptions _options;

	public JwtService(IOptions<JwtOptions> jwtOptions)
	{
		ArgumentNullException.ThrowIfNull(jwtOptions, nameof(jwtOptions));

		_options = jwtOptions.Value;
	}

	/// <inheritdoc />
	public string GenerateToken(Claim[] claims)
	{
		return GenerateToken(new ClaimsIdentity(claims));
	}

	/// <inheritdoc />
	public string GenerateToken(Claim[] claims, DateTime? expiry)
	{
		return GenerateToken(new ClaimsIdentity(claims), expiry);
	}

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity subject)
	{
		return GenerateToken(subject, DateTime.Now.AddSeconds(_options.DefaultExpirationSeconds));
	}

	/// <inheritdoc />
	public string GenerateToken(ClaimsIdentity subject, DateTime? expiry)
	{
		var token = new JwtSecurityToken(
			issuer: _options.Issuer,
			audience: _options.Audience,
			claims: subject.Claims,
			expires: expiry?.ToUniversalTime() ?? DateTime.UtcNow.AddSeconds(_options.DefaultExpirationSeconds),
			signingCredentials: new SigningCredentials(_options.Key, _options.Algorithm)
		);
		return new JwtSecurityTokenHandler().WriteToken(token);
	}

	/// <inheritdoc />
	public ClaimsPrincipal ValidateToken(string token)
	{
		return ValidateToken(token, new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = _options.Key,
			ValidateIssuer = false,
			ValidateAudience = false,
			ClockSkew = TimeSpan.Zero,
			//ValidateIssuer = true,
			//ValidIssuer = Issuer,
			//ValidateAudience = true,
			//ValidAudience = Audience,
		});
	}

	/// <inheritdoc />
	public ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		return tokenHandler.ValidateToken(token, validationParameters, out _);
	}
}
