using Dwt.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Dwt.Api.Helpers;

/// <summary>
/// Custom attribute to authorize a request using JWT.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class JwtAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
    /// </summary>
    public string? Roles
    {
        get => _roles;
        set
        {
            _roles = value;
            _rolesList = User.RoleStrToArr(value ?? "");
        }
    }

    private string? _roles;
    private string[]? _rolesList;

    private static readonly IActionResult _unauthorizedResult = new JsonResult(new
    {
        status = StatusCodes.Status401Unauthorized,
        message = "Unauthorized"
    })
    { StatusCode = StatusCodes.Status401Unauthorized };

    private static readonly IActionResult _deniedResult = new JsonResult(new
    {
        status = StatusCodes.Status403Forbidden,
        message = "Access denied"
    })
    { StatusCode = StatusCodes.Status403Forbidden };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var token = context.HttpContext.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            VerifyToken(context, token);
        }
        else
        {
            context.Result = _unauthorizedResult;
        }
    }

    private void VerifyToken(AuthorizationFilterContext context, string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = JwtRepository.Key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var claimRoles = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (claimRoles == null || _rolesList is not { Length: > 0 }) return;
            
            var userRoles = User.RoleStrToArr(claimRoles);
            if (!_rolesList.Any(r => userRoles.Contains(r)))
            {
	            context.Result = _deniedResult;
            }
        }
        catch (Exception e)
        {
            context.Result = new JsonResult(new
            {
                status = StatusCodes.Status401Unauthorized,
                message = $"Unauthorized: {e.Message}"
            })
            { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}
