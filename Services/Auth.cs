namespace dwt.Services;

/// <summary>
/// Abstract authenticator.
/// </summary>
public interface IAuthenticator
{
    /// <summary>
    /// Perform an authentication action.
    /// </summary>
    /// <param name="req">The authentication request.</param>
    /// <returns></returns>
    public AuthResp Authenticate(AuthReq req);

    /// <summary>
    /// Refresh an issued authentication token.
    /// </summary>
    /// <param name="token">The issued authentication token.</param>
    /// <returns></returns>
    public AuthResp Refresh(string token);
}
