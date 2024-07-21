using dwt.Entities;
using dwt.Helpers;
using dwt.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace dwt;

/// <summary>
/// Services implement this interface to perform bootstrapping tasks.
/// </summary>
public interface IBootstrapper
{
    /// <summary>
    /// Perform bootstrapping tasks.
    /// </summary>
    /// <param name="app"></param>
    public void Bootstrap(WebApplication app);
}

/// <summary>
/// Async version of IBootstrapper.
/// </summary>
public interface IAsyncBootstrapper
{
    /// <summary>
    /// Perform bootstrapping tasks asynchronously.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public Task BootstrapAsync(WebApplication app);
}

/// <summary>
/// Built-in bootstrapper that initializes Cryptography keys.
/// </summary>
public class CryptoKeysBootstrapper(ILogger<CryptoKeysBootstrapper> logger, IConfiguration config) : IBootstrapper
{
    public void Bootstrap(WebApplication app)
    {
        logger.LogInformation("Initializing Cryptography keys...");

        RSA? privKey = null;
        var rsaPfxFile = config["Keys:RSAPFXFile"];
        var rsaPrivKeyFile = config["Keys:RSAPrivKeyFile"];

        if (!string.IsNullOrWhiteSpace(rsaPfxFile))
        {
            // load RSA private key from PFX file if available
            logger.LogInformation($"Loading RSA private key from PFX file: {rsaPfxFile}...");
            var rsaPfxPassword = config["Keys:RSAPFXPassword"] ?? "";
            using var cert = new X509Certificate2(rsaPfxFile, rsaPfxPassword);
            privKey = cert.GetRSAPrivateKey() ?? throw new InvalidDataException($"Failed to load RSA private key from PFX file: {rsaPfxFile}");
        }
        else if (!string.IsNullOrWhiteSpace(rsaPrivKeyFile))
        {
            // load RSA private key from PEM file if available
            logger.LogInformation($"Loading RSA private key from file: {rsaPrivKeyFile}...");
            var rsaPrivKey = File.ReadAllText(rsaPrivKeyFile);
            privKey = RSA.Create();
            privKey.ImportFromPem(rsaPrivKey);
        }
        else
        {
            // generate new RSA private key
            logger.LogInformation("Generating new RSA key...");
            privKey = RSA.Create(3072);
        }

        Global.RSAPrivKey = privKey;
        Global.RSAPubKey = RSA.Create(privKey.ExportParameters(false));

        logger.LogInformation("Cryptography keys initialized.");
    }
}

/// <summary>
/// Built-in bootstrapper that performs JWT-related intializing tasks.
/// Note: this bootstrapper requires access to the RSA private key. Hence, it should be initialized after the CryptoKeysBootstrapper.
/// </summary>
public class JwtBootstrapper(ILogger<JwtBootstrapper> logger, IConfiguration config) : IBootstrapper
{
    public void Bootstrap(WebApplication app)
    {
        logger.LogInformation("Initializing JWT...");

        JwtRepository.Issuer = config["Jwt:Issuer"] ?? "<not defined>";
        JwtRepository.Audience = config["Jwt:Audience"] ?? "http://localhost:8080";
        JwtRepository.DefaultExpirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");

        var key = config["Jwt:Key"]?.Trim() ?? "";
        if (key == "")
        {
            if (Global.RSAPrivKey == null)
            {
                throw new NullReferenceException("RSA private key is null.");
            }
            JwtRepository.Key = new RsaSecurityKey(Global.RSAPrivKey);
            JwtRepository.Algorithm = SecurityAlgorithms.RsaSha256Signature;
        }
        else
        {
            JwtRepository.Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            JwtRepository.Algorithm = SecurityAlgorithms.HmacSha256;
        }

        logger.LogInformation("JWT initialized.");
    }
}

/*----------------------------------------------------------------------*/

/// <summary>
/// Demo async-bootstrapper that initializes a sample authenticator.
/// </summary>
public class SampleAuthenticatorAsyncBootstrapper(IServiceProvider serviceProvider, ILogger<SampleAuthenticatorAsyncBootstrapper> logger) : IAsyncBootstrapper
{
    public async Task BootstrapAsync(WebApplication app)
    {
        logger.LogInformation($"Initializing Sample Authenticator...");
        Global.Authenticator = ReflectionHelper.CreateInstance<IAuthenticator>(serviceProvider, typeof(SampleJwtAuthenticator));
        await Task.CompletedTask;
        logger.LogInformation($"Sample Authenticator initialized.");
    }

    class SampleJwtAuthenticator : IAuthenticator
    {
        private readonly int expirationSeconds;
        private readonly IUserService userService;
        public SampleJwtAuthenticator(IConfiguration config, IUserService userService)
        {
            expirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");
            this.userService = userService;
        }

        public AuthResp Authenticate(AuthReq req)
        {
            User? user = userService.GetUser(req.Id ?? "");
            if (user == null || !user.Authenticate(req.Secret))
            {
                return AuthResp.AuthFailed;
            }

            var expiry = DateTime.Now.AddSeconds(expirationSeconds);
            return AuthResp.New(200, JwtRepository.GenerateToken(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, JsonSerializer.Serialize(user.Roles)),
                // Add more claims as needed
            })), expiry);
        }

        public AuthResp Refresh(string token)
        {
            return new AuthResp { };
        }
    }
}
