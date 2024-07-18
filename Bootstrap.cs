using dwt.Services;
using Microsoft.AspNetCore.Components;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

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
/// Demo async-bootstrapper that initializes a sample authenticator.
/// </summary>
public class SampleAuthenticatorAsyncBootstrapper(ILogger<SampleAuthenticatorAsyncBootstrapper> logger, IConfiguration config) : IAsyncBootstrapper
{
    public async Task BootstrapAsync(WebApplication app)
    {
        logger.LogInformation($"Initializing Sample Authenticator...");
        Global.Authenticator = new SampleAuthenticator(config);
        await Task.CompletedTask;
        logger.LogInformation($"Sample Authenticator initialized.");
    }

    class SampleAuthenticator : JwtAuthenticator
    {
        public SampleAuthenticator(IConfiguration config) : base(config) { }

        public override AuthResp Authenticate(AuthReq req)
        {
            if (req.Id == "admin" && req.Secret == "@dminS3cret")
            {
                return AuthResp.New(200, GenerateToken("demo"), null);
            }
            return AuthResp.AuthFailed;
        }

        public override AuthResp Refresh(string token)
        {
            return new AuthResp { };
        }
    }
}
