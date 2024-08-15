using System.Security.Cryptography;

namespace Dwt.Api.Services;

public class CryptoOptions
{
	/// <summary>
	/// RSA public key for verifying API calls.
	/// </summary>
	public RSA RSAPrivKey { get; set; } = default!;

	/// <summary>
	/// RSA public key, derived from the private key.
	/// </summary>
	public RSA RSAPubKey { get; set; } = default!;
}
