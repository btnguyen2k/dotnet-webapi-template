using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

[ApiController]
public class BuiltinController : ControllerBase {

    private readonly IConfiguration _conf;

    private readonly IWebHostEnvironment _env;

    public BuiltinController(IConfiguration config, IWebHostEnvironment env) {
        _conf = config;
        _env = env;
    }

    private static readonly Dictionary<string, object> _ok = new Dictionary<string, object> { { "status", 200 } }; 
    private static readonly Dictionary<string, object> _notReady = new Dictionary<string, object> { 
        { "status", 503 }, { "message", "Server is not ready to handle requests." }
    };

    /// <summary>
    /// Health check handler.
    /// </summary>
    [HttpGet("/health")]
    public IActionResult Health() {
        return Ok(_ok);
    }

    /// <summary>
    /// Readiness check handler.
    /// </summary>
    [HttpGet("/ready")]
    public IResult Ready() {
        return Global.Ready ? Results.Ok(_ok) : Results.Json(_notReady, JsonSerializerOptions.Default, null, 503);
    }

    /// <summary>
    /// Handler that returns service's information.
    /// </summary>
    [HttpGet("/info")]
    public IActionResult Info() {
        return Ok(new Dictionary<string, object> { 
                { "status", 200 }, 
                { "data", new {
                    app = new {
                        name = _conf["App:Name"],
                        version = _conf["App:Version"],
                        description = _conf["App:Description"],
                    },
                    server = new {
                        env = _env.EnvironmentName,
                        time = DateTime.Now,
                    },
                }} 
            });
    }
}
