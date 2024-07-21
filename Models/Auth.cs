using Microsoft.AspNetCore.Mvc;

namespace dwt.Services;

/// <summary>
/// An authentication request.
/// </summary>
public class AuthReq
{
    /// <summary>
    /// Credentials: client/user id.
    /// </summary>
    [BindProperty(Name = "id")]
    public string? Id { get; set; }

    /// <summary>
    /// Credentials: client/user secret.
    /// </summary>
    [BindProperty(Name = "secret")]
    public string? Secret { get; set; }

    /// <summary>
    /// (Optional) Encryption settings.
    /// </summary>
    [BindProperty(Name = "encryption")]
    public string? Encryption { get; set; }
}

/// <summary>
/// Response to an authentication request.
/// </summary>
public class AuthResp
{
    public static readonly AuthResp AuthFailed = new AuthResp { _status = 403, _error = "Authentication failed." };
    public static readonly AuthResp TokenExpired = new AuthResp { _status = 401, _error = "Token expired." };

    /// <summary>
    /// Convenience method to create a new AuthResp instance.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="error"></param>
    /// <returns></returns>
    public static AuthResp New(int status, string error)
    {
        return new AuthResp { _status = status, _error = error ?? "" };
    }

    /// <summary>
    /// Convenience method to create a new AuthResp instance.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="token"></param>
    /// <param name="expiry"></param>
    /// <returns></returns>
    public static AuthResp New(int status, string token, DateTime? expiry)
    {
        return new AuthResp { _status = status, _token = token ?? "", _expiry = expiry };
    }

    private int _status;
    private string _error = "";
    private string _token = "";
    private DateTime? _expiry;

    /// <summary>
    /// Authentication status.
    /// </summary>
    /// <value>200: success</value>
    [BindProperty(Name = "status")]
    public int Status { get { return _status; } }

    /// <summary>
    /// Additional error information, if any.
    /// </summary>
    [BindProperty(Name = "error")]
    public string? Error { get { return _error; } }

    /// <summary>
    /// Authentication token, if successful.
    /// </summary>
    [BindProperty(Name = "token")]
    public string? Token { get { return _token; } }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime? Expiry { get { return _expiry; } }
}
