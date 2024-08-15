using Dwt.Api.Helpers;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in ready-to-use IApplicationBootstrapper.
/// </summary>
public class ApplicationBootstrapper(ILogger<ApplicationBootstrapper> logger) : IApplicationBootstrapper
{
	/// <inheritdoc/>
	public List<Task> Boostrap(WebApplicationBuilder appBuilder, out WebApplication app)
	{
		logger.LogInformation("Boostrapping application...");

		// load all bootstrappers defined in configuration
		var bootstrapperNames = appBuilder.Configuration.GetSection("Bootstrap:Components").Get<List<string>>() ?? [];
		var bootstrapperList = new List<Type>();
		foreach (var bootstrapperName in bootstrapperNames)
		{
			logger.LogInformation("Loading bootstrapper '{bootstrapperName}'...", bootstrapperName);

			var bootstrapperType = Type.GetType(bootstrapperName);
			if (bootstrapperType == null)
			{
				logger.LogWarning("Cannot find bootstrapper type '{bootstrapperName}'", bootstrapperName);
				continue;
			}
			if (!bootstrapperType.IsAssignableTo(typeof(IBootstrapper)) && !bootstrapperType.IsAssignableTo(typeof(IAsyncBootstrapper)))
			{
				logger.LogError("Bootstrapper '{bootstrapperName}' does not implement IBootstrapper or IAsyncBootstrapper", bootstrapperName);
				continue;
			}
			bootstrapperList.Add(bootstrapperType);
		}

		// start bootstrapping
		var asyncBootstrapTasks = new List<Task>();
		asyncBootstrapTasks.AddRange(ConfigureBuilder(appBuilder, bootstrapperList));
		app = appBuilder.Build();
		asyncBootstrapTasks.AddRange(DecorateApp(app, bootstrapperList));
		return asyncBootstrapTasks;
	}

	/// <summary>
	/// Configurate the WebApplicationBuilder with the bootstrappers.
	/// </summary>
	/// <param name="builder"></param>
	/// <param name="bootstrappers">Each bootstrapper must be IBootstrapper/IAsyncBootstrapper.</param>
	/// <returns>List of background bootstrapping tasks, if any.</returns>
	protected virtual List<Task> ConfigureBuilder(WebApplicationBuilder builder, List<Type> bootstrappers)
	{
		var asyncBootstrapTasks = new List<Task>();

		foreach (var bootstrapperType in bootstrappers)
		{
			if (bootstrapperType.IsAssignableTo(typeof(IBootstrapper)))
			{
				var bootstrapper = ReflectionHelper.CreateInstance<IBootstrapper>(builder.Services, bootstrapperType)
					?? throw new InvalidCastException($"Cannot create instance of bootstrapper '{bootstrapperType.FullName}' casted to IBootstrapper");
				logger.LogInformation("Invoking ConfigureBuilder() method on bootstrapper '{bootstrapper}'...", bootstrapperType.FullName);
				bootstrapper.ConfigureBuilder(builder);
			}

			if (bootstrapperType.IsAssignableTo(typeof(IAsyncBootstrapper)))
			{
				var bootstrapper = ReflectionHelper.CreateInstance<IAsyncBootstrapper>(builder.Services, bootstrapperType)
					?? throw new InvalidCastException($"Cannot create instance of bootstrapper '{bootstrapperType.FullName}' casted to IAsyncBootstrapper");
				logger.LogInformation("Invoking ConfigureBuilderAsync() method on bootstrapper '{bootstrapper}'...", bootstrapperType.FullName);
				asyncBootstrapTasks.Add(bootstrapper.ConfigureBuilderAsync(builder));
			}
		}

		return asyncBootstrapTasks;
	}

	/// <summary>
	/// Decorate the WebApplication with the bootstrappers.
	/// </summary>
	/// <param name="app"></param>
	/// <param name="bootstrappers"></param>
	/// <returns></returns>
	/// <exception cref="InvalidCastException"></exception>
	protected virtual List<Task> DecorateApp(WebApplication app, List<Type> bootstrappers)
	{
		var asyncBootstrapTasks = new List<Task>();

		foreach (var bootstrapperType in bootstrappers)
		{
			if (bootstrapperType.IsAssignableTo(typeof(IBootstrapper)))
			{
				var bootstrapper = ReflectionHelper.CreateInstance<IBootstrapper>(app.Services, bootstrapperType)
					?? throw new InvalidCastException($"Cannot create instance of bootstrapper '{bootstrapperType.FullName}' casted to IBootstrapper");
				logger.LogInformation("Invoking DecorateApp() method on bootstrapper '{bootstrapper}'...", bootstrapperType.FullName);
				bootstrapper.DecorateApp(app);
			}

			if (bootstrapperType.IsAssignableTo(typeof(IAsyncBootstrapper)))
			{
				var bootstrapper = ReflectionHelper.CreateInstance<IAsyncBootstrapper>(app.Services, bootstrapperType)
					?? throw new InvalidCastException($"Cannot create instance of bootstrapper '{bootstrapperType.FullName}' casted to IAsyncBootstrapper");
				logger.LogInformation("Invoking DecorateAppAsync() method on bootstrapper '{bootstrapper}'...", bootstrapperType.FullName);
				asyncBootstrapTasks.Add(bootstrapper.DecorateAppAsync(app));
			}
		}

		return asyncBootstrapTasks;
	}
}
