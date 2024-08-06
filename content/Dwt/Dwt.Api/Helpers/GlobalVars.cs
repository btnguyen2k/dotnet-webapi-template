using Dwt.Api.Services;
using System.Security.Cryptography;

namespace Dwt.Api.Helpers;

/// <summary>
/// An application-wide global variables and repository.
/// </summary>
public static class GlobalVars
{
    /// <summary>
    /// Reference to the WebApplication instance.
    /// </summary>
    public static WebApplication? App { get; set; }

    /// <summary>
    /// Set to true when the server is ready to handle requests.
    /// </summary>
    public static bool Ready { get; set; }

    /// <summary>
    /// RSA public key for verifying API calls.
    /// </summary>
    public static RSA? RSAPrivKey { get; set; }

    /// <summary>
    /// RSA public key, derived from the private key.
    /// </summary>
    public static RSA? RSAPubKey { get; set; }

    /// <summary>
    /// Reference to the authenticator instance to authenticate requests.
    /// </summary>
    public static IAuthenticator? Authenticator { get; set; }

    /// <summary>
    /// Name of the HttpContext item for storing the user ID.
    /// </summary>
    public const string HTTP_CTX_ITEM_USERID = "UserId";
}
