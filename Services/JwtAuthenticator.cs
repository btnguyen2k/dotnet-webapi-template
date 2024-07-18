using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace dwt.Services;

public abstract class JwtAuthenticator : IAuthenticator
{
    protected readonly IConfiguration config;
    protected readonly string issuer, audience;
    protected readonly int expirationSeconds;
    //protected readonly string key;
    //private readonly SecurityKey? asyncSecurityKey, syncSecurityKey;
    private readonly SigningCredentials signingCredentials;
    public JwtAuthenticator(IConfiguration config)
    {
        this.config = config;
        issuer = config["Jwt:Issuer"] ?? this.GetType().FullName ?? typeof(JwtAuthenticator).Name;
        audience = config["Jwt:Audience"] ?? "http://localhost:8080";
        expirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");
        var key = config["Jwt:Key"]?.Trim() ?? "";

        // TODO is SigningCredentials sharable and resuable?
        if (key == "")
        {
            if (Global.RSAPrivKey == null)
            {
                throw new NullReferenceException("RSA private key is null.");
            }
            signingCredentials = new SigningCredentials(new RsaSecurityKey(Global.RSAPrivKey), SecurityAlgorithms.RsaSha256Signature);
        }
        else
        {
            signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
        }
    }

    public abstract AuthResp Authenticate(AuthReq req);
    public abstract AuthResp Refresh(string token);

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    protected string GenerateToken(string data)
    {
        return GenerateToken(data, null);
    }

    /// <summary>
    /// Convenience method to generate a JWT token.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    protected string GenerateToken(string data, DateTime? expiry)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                //new Claim(ClaimTypes.NameIdentifier, user.UserID),
                //new Claim(ClaimTypes.Role, user.Role.ToString()),
                // Add more claims as needed
            }),
            Expires = expiry?.ToUniversalTime() ?? DateTime.UtcNow.AddSeconds(expirationSeconds),
            IssuedAt = DateTime.UtcNow,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = signingCredentials,
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
