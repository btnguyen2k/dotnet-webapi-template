using Dwt.Api.Middleware.JwtIdentity;

namespace Dwt.Api.Bootstrap;

public class ControllersBootstrapper : IBootstrapper
{
	public void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddControllers(options =>
		{
			// all requests go through this global filter
			// Controller/Action marked with [JwtAuthorizeAttribute] will be examined by the filter. 
			options.Filters.Add<JwtAuthGlobalFilter>();
		});
	}

	public void DecorateApp(WebApplication app)
	{
		app.MapControllers();
	}
}
