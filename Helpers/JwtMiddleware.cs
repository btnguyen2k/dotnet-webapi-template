using dwt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace dwt.Helpers;

/// <summary>
/// Middleware that decodes the JWT token in the request Authorization header (if any) and attaches
/// the user-id to the HttpContext.Items collection to make it accessible within the scope of the current request.
/// </summary>
/// <remarks>
///     Sample usage:
///         app.UseMiddleware&lt;JwtMiddleware&gt;();
/// </remarks>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public JwtMiddleware(ILogger<ErrorHandlerMiddleware> logger, RequestDelegate next)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            attachUserIdToContext(context, token);
        }
        await _next(context);
    }

    private void attachUserIdToContext(HttpContext context, string jwtToken)
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
            var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
            var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            context.Items[Global.HTTP_CTX_ITEM_USERID] = claimUserId;
        }
        catch
        {
            // do nothing if jwt validation fails
            // user-id is not attached to context
        }
    }
}
