using dwt.Services;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

public class BuiltinController : DwtBaseController
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

    /// <summary>
    /// Checks if the server is running.
    /// </summary>
    /// <response code="200">Server is running.</response>
    [HttpGet("/health")]
    [HttpGet("/healthz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return ResponseOk();
    }

    /// <summary>
    /// Checks if server is ready to handle requests.
    /// </summary>
    /// <response code="200">Server is ready to handle requests.</response>
    /// <response code="503">Server is running but NOT yet ready to handle requests.</response>
    [HttpGet("/ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Ready()
    {
        return Global.Ready ? ResponseOk() : Response503("");
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
        return ResponseOk(data);
    }

    private static readonly Dictionary<string, object> _noAuthenticatorOrErrorAuthenticating = new()
    {
        { "status", 500 },
        { "message", "No authenticator defined or error while authenticating." },
    };

    /// <summary>
    /// Authenticates the client.
    /// </summary>
    /// <response code="200">Authentication was succesful.</response>
    /// <response code="403">Authentication failed.</response>
    /// <response code="500">No authenticator defined or error while authenticating.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("/auth")]
    public IActionResult Authenticate([FromBody] AuthReq authReq)
    {
        var resp = Global.Authenticator?.Authenticate(authReq);
        return resp == null
            ? ResponseNotOk(500, "No authenticator defined or error while authenticating.")
            : resp.Status == 200
                ? ResponseOk(new { token = resp.Token, expiry = resp.Expiry })
                : ResponseNotOk(resp.Status, resp.Error);
    }
}
