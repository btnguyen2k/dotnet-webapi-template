using Dwt.Api.Bootstrap;
using Dwt.Api.Helpers;

var builder = WebApplication.CreateBuilder(args);
var logger = builder.Services.FirstOrDefault(
    s => s.ServiceType == typeof(ILogger<Program>))?.ImplementationInstance as ILogger<Program>;
if (logger == null)
{
    using ILoggerFactory factory = LoggerFactory.Create(b => b.AddConsole());
    //using ILoggerFactory factory = LoggerFactory.Create(b => b.AddConfiguration(builder.Configuration.GetSection("Logging")));
    logger = factory.CreateLogger<Program>();
}
logger.LogInformation("Starting application...");

var appBootstrapperName = builder.Configuration.GetSection("Bootstrap")["Application"];
var appBootstrapperType = appBootstrapperName != null ? Type.GetType(appBootstrapperName) : null;
if (appBootstrapperType == null)
{
    logger.LogError("Application bootstrapper not found.");
    return;
}
var appBootstrapper = ReflectionHelper.CreateInstance<IApplicationBootstrapper>(builder.Services, appBootstrapperType);
if (appBootstrapper == null)
{
    logger.LogError("Application bootstrapper not found.");
    return;
}

var app = appBootstrapper.CreateApplication(builder);
GlobalVars.App = app;

var backgroundBootstrappingTasks = appBootstrapper.InitializeApplication(app);
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
