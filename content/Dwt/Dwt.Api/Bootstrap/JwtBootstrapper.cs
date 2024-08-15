using Dwt.Api.Middleware.JwtIdentity;
using Dwt.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that performs JWT-related intializing tasks.
/// </summary>
/// <remarks>
///		- This bootstrapper requires access to the RSA private key. Hence, it should be initialized after the CryptoKeysBootstrapper.<br/>
///		- This bootstrapper stores the JWT configurations in the service container as IOption&lt;JwtOptions&gt; for later use via dependency injection.<br/>
///		- Also, a IJwtService is registered in the service container.
/// </remarks>
public class JwtBootstrapper(ILogger<JwtBootstrapper> logger, IConfiguration config, IOptions<CryptoOptions> cryptoOptions) : IBootstrapper
{
	public void DecorateApp(WebApplication app)
	{
		app.UseMiddleware<JwtIdentityMiddleware>();
	}

	public void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		logger.LogInformation("Initializing JWT...");

		var jwtIssuer = config["Jwt:Issuer"] ?? "<not defined>";
		var jetAudience = config["Jwt:Audience"] ?? "http://localhost:8080";
		var jwtDefaultExpirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");
		SecurityKey jwtKey;
		string jwtAlgorithm;

		var key = config["Jwt:Key"]?.Trim() ?? "";
		if (string.IsNullOrEmpty(key))
		{
			// JWT signing key is not defined, use RSA private key to sign JWT
			ArgumentNullException.ThrowIfNull(cryptoOptions.Value, nameof(cryptoOptions.Value));
			ArgumentNullException.ThrowIfNull(cryptoOptions.Value.RSAPrivKey, "RSA Private key");

			jwtKey = new RsaSecurityKey(cryptoOptions.Value.RSAPrivKey);
			jwtAlgorithm = SecurityAlgorithms.RsaSha256Signature;
		}
		else
		{
			// JWT signing key is defined, use it to sign JWT
			jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
			jwtAlgorithm = SecurityAlgorithms.HmacSha256;
		}

		// store JWT configurations in the service container
		appBuilder.Services.Configure<JwtOptions>(options =>
		{
			options.Key = jwtKey;
			options.Algorithm = jwtAlgorithm;
			options.Issuer = jwtIssuer;
			options.Audience = jetAudience;
			options.DefaultExpirationSeconds = jwtDefaultExpirationSeconds;
		});

		// register JwtService in the service container
		appBuilder.Services.AddSingleton<IJwtService, JwtService>();

		logger.LogInformation("JWT initialized.");
	}
}
