using Dwt.Api.Helpers;
using Dwt.Api.Services;
using Dwt.Shared.Models;
using System.Security.Claims;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Demo async-bootstrapper that initializes a sample authenticator.
/// </summary>
public class SampleAuthenticatorAsyncBootstrapper(IServiceProvider serviceProvider, ILogger<SampleAuthenticatorAsyncBootstrapper> logger) : IAsyncBootstrapper
{
    public async Task BootstrapAsync(WebApplication app)
    {
        logger.LogInformation($"Initializing Sample Authenticator...");
        await Task.Delay(1024); // (sample) to simulate async
        GlobalVars.Authenticator = ReflectionHelper.CreateInstance<IAuthenticator>(serviceProvider, typeof(SampleJwtAuthenticator));
        logger.LogInformation($"Sample Authenticator initialized.");
    }

    /// <summary>
    /// JWT implementation of IAuthenticator.
    /// </summary>
    class SampleJwtAuthenticator(IConfiguration config, IUserRepository userService) : IAuthenticator
    {
        private readonly int expirationSeconds = int.Parse(config["Jwt:Expiration"] ?? "3600");
        private readonly IUserRepository userService = userService;

        private static string GenerateToken(User user, DateTime expiry)
        {
            return JwtRepository.GenerateToken(expiry, new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Upn, user.Id),
                new Claim(ClaimTypes.Role, user.Role),
                // Add more claims as needed
            ]));
        }

        /// <inheritdoc />
        public AuthResp Authenticate(AuthReq req)
        {
            User? user = userService.GetByID(req.Id ?? "");
            if (user == null || !user.Authenticate(req.Secret))
            {
                return AuthResp.AuthFailed;
            }
            var expiry = DateTime.Now.AddSeconds(expirationSeconds);
            return AuthResp.New(200, GenerateToken(user, expiry), expiry);
        }

        /// <inheritdoc />
        public AuthResp Refresh(string jwtToken)
        {
            try
            {
                var principal = JwtRepository.ValidateToken(jwtToken);
                var claimUserId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Upn)?.Value;
                User? user = userService.GetByID(claimUserId ?? "");
                if (user == null)
                {
                    return AuthResp.AuthFailed;
                }
                var expiry = DateTime.Now.AddSeconds(expirationSeconds);
                return AuthResp.New(200, GenerateToken(user, expiry), expiry);
            }
            catch (Exception e)
            {
                return AuthResp.New(403, e.Message);
            }
        }
    }
}
