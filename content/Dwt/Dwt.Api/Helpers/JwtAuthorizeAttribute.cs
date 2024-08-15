namespace Dwt.Api.Helpers;

/// <summary>
/// Custom attribute to authorize a request using JWT.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtAuthorizeAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
    /// </summary>
    public string? Roles { get; set; }
}
