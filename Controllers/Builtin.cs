using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace dwt.Controllers;

[ApiController]
public class BuiltinController : ControllerBase
{
    private readonly IConfiguration _conf;

    private readonly IWebHostEnvironment _env;

    private readonly Dictionary<string, object> _objApp;

    public BuiltinController(IConfiguration config, IWebHostEnvironment env)
    {
        _conf = config;
        _env = env;
        _objApp = new Dictionary<string, object>
        {
            { "name", _conf["App:Name"]??"" },
            { "version", _conf["App:Version"]??"" },
            { "description", _conf["App:Description"]??"" },
        };
    }

    private static readonly Dictionary<string, object> _ok = new Dictionary<string, object> { { "status", 200 } };
    private static readonly Dictionary<string, object> _notReady = new Dictionary<string, object> {
        { "status", 503 }, { "message", "Server is not ready to handle requests." }
    };

    /// <summary>
    /// Checks if the server is running.
    /// </summary>
    /// <response code="200">Server is running.</response>
    [HttpGet("/health")]
    [HttpGet("/healthz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(_ok);
    }

    /// <summary>
    /// Checks if server is ready to handle requests.
    /// </summary>
    /// <response code="200">Server is ready to handle requests.</response>
    /// <response code="503">Server is running but NOT yet ready to handle requests.</response>
    [HttpGet("/ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IResult Ready()
    {
        return Global.Ready ? Results.Ok(_ok) : Results.Json(_notReady, JsonSerializerOptions.Default, null, 503);
    }

    /// <summary>
    /// Returns service's information.
    /// </summary>
    /// <response code="200">Server's information.</response>
    [HttpGet("/info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Info()
    {
        var data = new Dictionary<string, object>
        {
            { "ready", Global.Ready },
            { "app", _objApp },
            { "server", new{
                env = _env.EnvironmentName,
                time = DateTime.Now,
            }},
        };
        if (Global.RSAPubKey != null)
        {
            data["crypto"] = new Dictionary<string, object>
            {
                { "pub_key", Convert.ToBase64String(Global.RSAPubKey.ExportRSAPublicKey()) },
                { "pub_key_type", "RSA-PKCS#1" },
            };
        }

        return Ok(new Dictionary<string, object>
        {
            { "status", 200 },
            { "data", data },
        });
    }
}
