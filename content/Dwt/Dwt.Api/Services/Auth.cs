using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Dwt.Api.Services;

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
	/// <param name="ignoreTokenSecurityCheck">If true, donot check if token's security tag is still valid.</param>
	/// <returns></returns>
	public AuthResp Refresh(string token, bool ignoreTokenSecurityCheck = false);

	/// <summary>
	/// Validate an issued authentication token.
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public TokenValidationResp Validate(string token);
}

/// <summary>
/// Async version of IAuthenticator.
/// </summary>
public interface IAuthenticatorAsync
{
	/// <summary>
	/// Perform an authentication action.
	/// </summary>
	/// <param name="req">The authentication request.</param>
	/// <returns></returns>
	public Task<AuthResp> AuthenticateAsync(AuthReq req);

	/// <summary>
	/// Refresh an issued authentication token.
	/// </summary>
	/// <param name="token">The issued authentication token.</param>
	/// <param name="ignoreTokenSecurityCheck">If true, donot check if token's security tag is still valid.</param>
	/// <returns></returns>
	public Task<AuthResp> RefreshAsync(string token, bool ignoreTokenSecurityCheck = false);

	/// <summary>
	/// Validate an issued authentication token.
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public Task<TokenValidationResp> ValidateAsync(string token);
}

/// <summary>
/// Resposne to a token validation request.
/// </summary>
public class TokenValidationResp
{
	/// <summary>
	/// Validation status.
	/// </summary>
	/// <value>200: success</value>
	[JsonIgnore]
	public int Status { get; set; }

	/// <summary>
	/// Additional error information, if any.
	/// </summary>
	[JsonPropertyName("error")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Error { get; set; }

	[JsonIgnore]
	public ClaimsPrincipal? Principal { get; set; }
}

/// <summary>
/// Authentication request.
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
	public static readonly AuthResp AuthFailed = new() { _status = 403, _error = "Authentication failed." };
	public static readonly AuthResp TokenExpired = new() { _status = 401, _error = "Token expired." };

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
	private string? _error;
	private string? _token;
	private DateTime? _expiry;

	/// <summary>
	/// Authentication status.
	/// </summary>
	/// <value>200: success</value>
	[JsonIgnore]
	public int Status { get { return _status; } }

	/// <summary>
	/// Additional error information, if any.
	/// </summary>
	[JsonPropertyName("error")]
	[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
	public string? Error { get { return _error; } }

	/// <summary>
	/// Authentication token, if successful.
	/// </summary>
	[JsonPropertyName("token")]
	public string? Token { get { return _token; } }

	/// <summary>
	/// When the token expires.
	/// </summary>
	[JsonPropertyName("expiry")]
	public DateTime? Expiry { get { return _expiry; } }
}
