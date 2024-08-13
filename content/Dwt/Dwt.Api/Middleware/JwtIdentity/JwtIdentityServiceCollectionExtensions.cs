namespace Dwt.Api.Middleware.Jwt;

public static class JwtIdentityServiceCollectionExtensions
{
	public static IServiceCollection AddJwtIdentity(this IServiceCollection services)
	{
		ArgumentNullException.ThrowIfNull(services);

		return services;
	}

	public static IServiceCollection AddJwtIdentity(this IServiceCollection services, Action<JwtIdentityOptions> configureOptions)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configureOptions);

		services.Configure(configureOptions);
		services.AddJwtIdentity();

		return services;
	}
}
