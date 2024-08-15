using Dwt.Api.Helpers;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that configures exception handlers.
/// </summary>
public class ExceptionHandlersBootstrapper : IBootstrapper
{
	public void DecorateApp(WebApplication app)
	{
		app.UseExceptionHandler(o => { }); //workaround for https://github.com/dotnet/aspnetcore/issues/51888
	}

	public void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddExceptionHandler<GlobalExceptionHandler>();
	}
}
