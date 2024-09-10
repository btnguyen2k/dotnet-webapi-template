using Dwt.Api.Services;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes Cryptography keys. This bootstrapper initializes the RSA keypair for JWT signing.
/// </summary>
/// <remarks>
///		The keypair is then stored in the service container as IOption&lt;CryptoOptions&gt; for later use via dependency injection.
/// </remarks>
[Bootstrapper(Priority = 100)]
public class CryptoKeysBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder, IConfiguration config)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<CryptoKeysBootstrapper>();

		logger.LogInformation("Initializing Cryptography keys...");

		RSA privKey;
		var rsaPfxFile = config["Keys:RSAPFXFile"];
		var rsaPrivKeyFile = config["Keys:RSAPrivKeyFile"];

		if (!string.IsNullOrWhiteSpace(rsaPfxFile))
		{
			// load RSA private key from PFX file if available
			logger.LogInformation("Loading RSA private key from PFX file '{rsaPfxFile}'...", rsaPfxFile);
			var rsaPfxPassword = config["Keys:RSAPFXPassword"] ?? "";
			using var cert = new X509Certificate2(rsaPfxFile, rsaPfxPassword);
			privKey = cert.GetRSAPrivateKey() ?? throw new InvalidDataException($"Failed to load RSA private key from PFX file '{rsaPfxFile}'");
		}
		else if (!string.IsNullOrWhiteSpace(rsaPrivKeyFile))
		{
			// load RSA private key from PEM file if available
			logger.LogInformation("Loading RSA private key from file '{PrivateKeyFile}'...", rsaPrivKeyFile);
			var rsaPrivKeyPem = File.ReadAllText(rsaPrivKeyFile);
			privKey = RSA.Create();
			privKey.ImportFromPem(rsaPrivKeyPem);
		}
		else
		{
			// generate new RSA private key
			logger.LogInformation("Generating new RSA key...");
			privKey = RSA.Create(3072);
		}

		// store RSA keypair in the service container
		appBuilder.Services.Configure<CryptoOptions>(options =>
		{
			options.RSAPrivKey = privKey;
			options.RSAPubKey = RSA.Create(privKey.ExportParameters(false));
		});

		logger.LogInformation("Cryptography keys initialized.");
	}
}
