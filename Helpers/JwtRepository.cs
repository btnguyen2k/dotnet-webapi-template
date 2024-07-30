using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace dwt.Helpers;

/// <summary>
/// Repository to hold JWT data to be shared application-wide.
/// </summary>
public static class JwtRepository
{
    /// <summary>
    /// The security key to sign and verify JWT tokens. This should be set by the application during startup.
    /// </summary>
    public static SecurityKey? Key;

    /// <summary>
    /// The algorithm to use for signing and verifying JWT tokens. This should be set by the application during startup.
    /// </summary>
    public static string? Algorithm;

    /// <summary>
    /// The issuer and audience to use for JWT tokens.
    /// </summary>
    public static string? Issuer, Audience;

    /// <summary>
    /// The default expiration time for JWT tokens in seconds.
    /// </summary>
    public static int DefaultExpirationSeconds = 3600;

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static string GenerateToken(Claim[] claims)
    {
        return GenerateToken(new ClaimsIdentity(claims));
    }

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="expiry"></param>
    /// <param name="claims"></param>
    /// <returns></returns>
    public static string GenerateToken(DateTime? expiry, Claim[] claims)
    {
        return GenerateToken(expiry, new ClaimsIdentity(claims));
    }

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string GenerateToken(ClaimsIdentity subject)
    {
        return GenerateToken(DateTime.Now.AddSeconds(DefaultExpirationSeconds), subject);
    }

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="expiry"></param>
    /// <param name="subject"></param>
    /// <returns></returns>
    public static string GenerateToken(DateTime? expiry, ClaimsIdentity subject)
    {
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: subject.Claims,
            expires: expiry?.ToUniversalTime() ?? DateTime.UtcNow.AddSeconds(DefaultExpirationSeconds),
            signingCredentials: new SigningCredentials(Key, Algorithm)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static ClaimsPrincipal ValidateToken(string token)
    {
        return ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = Key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
            //ValidateIssuer = true,
            //ValidIssuer = Issuer,
            //ValidateAudience = true,
            //ValidAudience = Audience,
        });
    }

    public static ClaimsPrincipal ValidateToken(string token, TokenValidationParameters validationParameters)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ValidateToken(token, validationParameters, out _);
    }
}
