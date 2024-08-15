using Dwt.Api.Bootstrap;
using Dwt.Api.Helpers;

var appBuilder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<Program>();
logger.LogInformation("Starting application...");

var appBootstrapperName = appBuilder.Configuration.GetSection("Bootstrap")["Application"];
var appBootstrapperType = appBootstrapperName != null ? Type.GetType(appBootstrapperName) : null;
if (appBootstrapperType == null)
{
	logger.LogError("Cannot find application bootstrapper: '{appBootstrapperName}'", appBootstrapperName);
	return;
}

var appBootstrapper = ReflectionHelper.CreateInstance<IApplicationBootstrapper>(appBuilder.Services, appBootstrapperType);
if (appBootstrapper == null)
{
	logger.LogError("Application bootstrapper not found.");
	return;
}

var backgroundBootstrappingTasks = appBootstrapper.Boostrap(appBuilder, out var app);
GlobalVars.App = app;
logger.LogInformation("WebApplication instance created and added to GlobalVars.");
WaitForBackgroundTasks(backgroundBootstrappingTasks);
app.Run();

async void WaitForBackgroundTasks(ICollection<Task> tasks)
{
	while (tasks.Count > 0)
	{
		var finishedTask = await Task.WhenAny(tasks);
		try { await finishedTask; }
		catch (Exception e)
		{
			logger.LogError(e, "Error executing bootstrapper task.");
		}
		tasks.Remove(finishedTask);
	}
	GlobalVars.Ready = true; // server is ready to handle requests
	logger.LogInformation("Background bootstrapping completed.");
}
