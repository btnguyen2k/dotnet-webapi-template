using dwt;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// builder.Services.AddDbContext<TodoContext>(opt => opt.UseInMemoryDatabase("TodoList"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer(); // required only for minimal APIs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "DWT Backend API",
        Description = "Dotnet WebAPI Template Backend.",
        // TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "GitHub Repo",
            Url = new Uri("https://github.com/btnguyen2k/dotnet-webapi-template")
        },
        License = new OpenApiLicense
        {
            Name = "MIT - License",
            Url = new Uri("https://github.com/btnguyen2k/dotnet-webapi-template/blob/main/LICENSE.md")
        }
    });

    // https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-8.0&tabs=visual-studio-code#xml-comments
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();
Global.App = app;

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

AppBootstrap(app);

app.Run();

object?[] BuildDIParams(IServiceProvider serviceProvider, Type objType)
{
    var constructor = objType.GetConstructors().First();
    var constructorParams = constructor.GetParameters();
    return constructorParams.Select(parameter => serviceProvider.GetService(parameter.ParameterType)).ToArray();
}

// Perform initialization tasks via bootstrappers.
async void AppBootstrap(WebApplication app)
{
    app.Logger.LogInformation("Start bootstrapping...");

    var bootstrapperNames = app.Configuration.GetSection("Bootstrappers").Get<List<String>>() ?? new List<String>();
    var asyncBootstrapTasks = new List<Task>();
    foreach (var bootstrapperName in bootstrapperNames)
    {
        app.Logger.LogInformation($"Loading bootstrapper: {bootstrapperName}...");

        var bootstrapperType = Type.GetType(bootstrapperName);
        if (bootstrapperType == null)
        {
            app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
            continue;
        }

        if (bootstrapperType.IsAssignableTo(typeof(IBootstrapper)))
        {
            var bootstrapper = Activator.CreateInstance(bootstrapperType, BuildDIParams(app.Services, bootstrapperType)) as IBootstrapper;
            if (bootstrapper == null)
            {
                app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
                continue;
            }
            bootstrapper.Bootstrap(app);
        }
        else if (bootstrapperType.IsAssignableTo(typeof(IAsyncBootstrapper)))
        {
            var bootstrapper = Activator.CreateInstance(bootstrapperType, BuildDIParams(app.Services, bootstrapperType)) as IAsyncBootstrapper;
            if (bootstrapper == null)
            {
                app.Logger.LogWarning($"Bootstrapper not found: {bootstrapperName}");
                continue;
            }
            asyncBootstrapTasks.Add(bootstrapper.BootstrapAsync(app));
        }
        else
        {
            app.Logger.LogError($"Bootstrapper {bootstrapperName} does not implement IBootstrapper or IAsyncBootstrapper");
        }
    }

    while (asyncBootstrapTasks.Count > 0)
    {
        var finishedTask = await Task.WhenAny(asyncBootstrapTasks);
        try { await finishedTask; }
        catch (Exception e)
        {
            // failure of an async bootstrapper task is logged, but does not stop the bootstrapping process
            app.Logger.LogError(e, "Error executing bootstrapper task.");
        }
        asyncBootstrapTasks.Remove(finishedTask);
    }

    Global.Ready = true; // server is ready to handle requests
    app.Logger.LogInformation("Bootstrapping completed.");
}
