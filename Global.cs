using dwt.Services;
using System.Security.Cryptography;

namespace dwt;

/// <summary>
/// An application-wide global variables and repository.
/// </summary>
public static class Global
{
    /// <summary>
    /// Reference to the WebApplication instance.
    /// </summary>
    public static WebApplication? App { get; set; } = null;

    /// <summary>
    /// Set to true when the server is ready to handle requests.
    /// </summary>
    public static bool Ready { get; set; } = false;

    /// <summary>
    /// RSA public key for verifying API calls.
    /// </summary>
    public static RSA? RSAPrivKey { get; set; } = null;

    /// <summary>
    /// RSA public key, derived from the private key.
    /// </summary>
    public static RSA? RSAPubKey { get; set; } = null;

    /// <summary>
    /// Reference to the authenticator instance to authenticate requests.
    /// </summary>
    public static IAuthenticator? Authenticator { get; set; } = null;

    public const string HTTP_CTX_ITEM_USERID = "UserId";
}
