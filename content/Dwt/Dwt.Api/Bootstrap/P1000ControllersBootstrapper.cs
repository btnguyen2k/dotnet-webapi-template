using Dwt.Api.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Bootstrap;

[Bootstrapper]
public class ControllersBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		appBuilder.Services.AddControllers()
			.ConfigureApiBehaviorOptions(options =>
			{
				// configure custom response for invalid model state (usually input validation failed)
				options.InvalidModelStateResponseFactory = context =>
				{
					var errors = context.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
					return new BadRequestObjectResult(new ApiResp<object>
					{
						Status = 400,
						Message = "Invalid model state.",
						Data = errors
					});
				};
			});
	}

	public static void DecorateApp(WebApplication app)
	{
		app.MapControllers();
	}
}
