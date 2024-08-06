using Dwt.Api.Helpers;
using System.Security.Claims;

namespace Dwt.Api.Middleware;

/// <summary>
/// Middleware that decodes the JWT token in the request Authorization header (if any) and attaches
/// the user-id to the HttpContext.Items collection to make it accessible within the scope of the current request.
/// </summary>
/// <remarks>
///     Sample usage:
///         app.UseMiddleware&lt;JwtMiddleware&gt;();
/// </remarks>
public class JwtMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        if (token != null)
        {
            AttachUserIdToContext(context, token);
        }
        await next(context);
    }

    private static void AttachUserIdToContext(HttpContext context, string jwtToken)
    {
        try
        {
            var principal = JwtRepository.ValidateToken(jwtToken);
            var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
            context.Items[GlobalVars.HTTP_CTX_ITEM_USERID] = claimUserId;
        }
        catch
        {
            // do nothing if jwt validation fails
            // user-id is not attached to context
        }
    }
}
