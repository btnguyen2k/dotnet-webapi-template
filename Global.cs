using System.Text.Json;

namespace dwt;

public static class Global {
    /// <summary>
    /// Reference to the WebApplication instance.
    /// </summary>
    public static WebApplication? App { get; set; } = null;

    /// <summary>
    /// Set to true when the server is ready to handle requests.
    /// </summary>
    public static bool Ready { get; set; } = false;
}
