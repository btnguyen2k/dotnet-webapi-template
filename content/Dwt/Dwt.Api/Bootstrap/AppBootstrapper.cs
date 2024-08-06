namespace Dwt.Api.Bootstrap;

/// <summary>
/// A concrete implementation of IApplicationBootstrapper will be used to bootstrap the application.
/// </summary>
public interface IApplicationBootstrapper
{
    /// <summary>
    /// Creates a new WebApplication instance.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    WebApplication CreateApplication(WebApplicationBuilder builder);

    /// <summary>
    /// Initializes the application created by CreateApplication.
    /// </summary>
    /// <param name="app"></param>
    /// <remarks>Since some bootstrapping tasks can be done in the background, check the returned Task list for the completion of background bootstrapping tasks.</remarks>
    List<Task> InitializeApplication(WebApplication app);
}

/// <summary>
/// Implement this interface to perform custom initializing tasks during application bootstrapping.
/// </summary>
public interface IBootstrapper
{
    void Bootstrap(WebApplication app);
}

/// <summary>
/// Async version of IBootstrapper.
/// </summary>
public interface IAsyncBootstrapper
{
    Task BootstrapAsync(WebApplication app);
}
