namespace Dwt.Api.Bootstrap;

/// <summary>
/// A concrete implementation of IApplicationBootstrapper will be used to bootstrap the application.
/// </summary>
public interface IApplicationBootstrapper
{
	/// <summary>
	/// Creates and initalizes a new WebApplication instance.
	/// </summary>
	/// <param name="appBuilder"></param>
	/// <param name="app"></param>
	/// <returns></returns>
	/// <remarks>Since some bootstrapping tasks can be done in the background, check the returned Task list for the completion of background bootstrapping tasks.</remarks>
	List<Task> Boostrap(WebApplicationBuilder appBuilder, out WebApplication app);
}

/// <summary>
/// Implement this interface to perform custom initializing tasks during application bootstrapping.
/// </summary>
/// <remarks>
/// One bootstrapper may implement one of methods ConfigureBuilder(IApplicationBuilder) or DecorateApp(WebApplication) or both.
/// However, do _not_ assume the two methods are invoked on the same bootstrapper instance.
/// </remarks>
public interface IBootstrapper
{
	void ConfigureBuilder(WebApplicationBuilder appBuilder);

	void DecorateApp(WebApplication app);
}

/// <summary>
/// A no-op implementation of IBootstrapper, convenient class for boostrappers to extend from.
/// </summary>
public class NoopBootstrapper : IBootstrapper
{
	public virtual void ConfigureBuilder(WebApplicationBuilder appBuilder) { /* noop */ }

	public virtual void DecorateApp(WebApplication app) { /* noop */ }
}

/// <summary>
/// Async version of IBootstrapper.
/// </summary>
public interface IAsyncBootstrapper
{
	Task ConfigureBuilderAsync(WebApplicationBuilder appBuilder);

	Task DecorateAppAsync(WebApplication app);
}

/// <summary>
/// A no-op implementation of IAsyncBootstrapper, convenient class for boostrappers to extend from.
/// </summary>
public class NoopAsyncBootstrapper : IAsyncBootstrapper
{
	public virtual async Task ConfigureBuilderAsync(WebApplicationBuilder appBuilder) => await Task.CompletedTask;

	public virtual async Task DecorateAppAsync(WebApplication app) => await Task.CompletedTask;
}
