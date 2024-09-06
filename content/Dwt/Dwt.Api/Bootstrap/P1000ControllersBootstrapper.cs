namespace Dwt.Api.Bootstrap;

[Bootstrapper]
public class ControllersBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddControllers();
	}

	public static void DecorateApp(WebApplication app)
	{
		app.MapControllers();
	}
}
