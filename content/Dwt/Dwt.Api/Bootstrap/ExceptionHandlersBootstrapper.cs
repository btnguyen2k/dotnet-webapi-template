using Dwt.Api.Helpers;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that configures exception handlers.
/// </summary>
[Bootstrapper(Priority = 100)]
public class ExceptionHandlersBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddExceptionHandler<GlobalExceptionHandler>();
	}

	public static void DecorateApp(WebApplication app)
	{
		app.UseExceptionHandler(o => { }); //workaround for https://github.com/dotnet/aspnetcore/issues/51888
	}
}
