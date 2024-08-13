using Microsoft.Extensions.Options;

namespace Dwt.Api.Middleware.Jwt;

public static class JwtIdentityApplicationBuilderExtensions
{
	public static IApplicationBuilder UseJwtIdentity(this IApplicationBuilder app)
	{
		ArgumentNullException.ThrowIfNull(app);

		return app.UseMiddleware<JwtIdentityMiddleware>();
	}

	public static IApplicationBuilder UseJwtIdentity(this IApplicationBuilder app, JwtIdentityOptions options)
	{
		ArgumentNullException.ThrowIfNull(app);
		ArgumentNullException.ThrowIfNull(options);

		return app.UseMiddleware<JwtIdentityMiddleware>(Options.Create(options));
	}

}
