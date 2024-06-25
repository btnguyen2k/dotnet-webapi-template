using System.Text.Json;

namespace dwt;

public static class Global {
    public static WebApplication? APP;

    public static bool READY = false;

    private static readonly Dictionary<string, object> _ok = new Dictionary<string, object> { { "status", 200 } }; 
    private static readonly Dictionary<string, object> _notReady = new Dictionary<string, object> { 
        { "status", 503 }, { "message", "Server is not ready to handle requests." }
    }; 

    /// <summary>
    /// Health check handler.
    /// </summary>
    public static readonly Delegate HANDLER_HEALTH = () => {
        return Results.Ok(_ok);
    };

    /// <summary>
    /// Readiness check handler.
    /// </summary>
    public static readonly Delegate HANDLER_READY = () => {
        return READY ? Results.Ok(_ok) : Results.Json(_notReady, JsonSerializerOptions.Default, null, 503);
    };

    public static readonly Delegate HANDLER_INFO = () => {
        return APP == null 
            ? Results.Ok(new Dictionary<string, object> { { "status", 500 }, { "message", "AppInfo is null." } })
            : Results.Ok(new Dictionary<string, object> { 
                { "status", 200 }, 
                { "data", new {
                    app = new {
                        name = APP.Configuration["App:Name"],
                        version = APP.Configuration["App:Version"],
                        description = APP.Configuration["App:Description"],
                    },
                    server = new {
                        env = APP.Environment.EnvironmentName,
                        time = DateTime.Now,
                    },
                }} 
            });
    };
}
