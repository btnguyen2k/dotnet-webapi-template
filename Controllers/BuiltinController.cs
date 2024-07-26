using dwt.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using static dwt.Controllers.BuiltinController;

namespace dwt.Controllers;

public class BuiltinController : DwtBaseController
{
    private readonly IConfiguration _conf;

    private readonly IWebHostEnvironment _env;

    public BuiltinController(IConfiguration config, IWebHostEnvironment env)
    {
        _conf = config;
        _env = env;
        appInfo = new AppInfo
        {
            Name = _conf["App:Name"] ?? "",
            Version = _conf["App:Version"] ?? "",
            Description = _conf["App:Description"] ?? "",
        };
        if (Global.RSAPubKey != null)
        {
            cryptoInfo = new CryptoInfo
            {
                PubKey = Convert.ToBase64String(Global.RSAPubKey.ExportRSAPublicKey()),
                PubKeyType = "RSA-PKCS#1",
            };
        }
    }

    /// <summary>
    /// Checks if the server is running.
    /// </summary>
    /// <response code="200">Server is running.</response>
    [HttpGet("/health")]
    [HttpGet("/healthz")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResp<object>> Health()
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
    public ActionResult<ApiResp<bool>> Ready()
    {
        return Global.Ready ? ResponseOk(true) : _respServiceUnavailable;
    }

    public class AppInfo
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
        [JsonPropertyName("version")]
        public string? Version { get; set; }
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
    private readonly AppInfo appInfo;

    public class ServerInfo
    {
        [JsonPropertyName("env")]
        public string? Env { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class CryptoInfo
    {
        [JsonPropertyName("pub_key")]
        public string? PubKey { get; set; }
        [JsonPropertyName("pub_key_type")]
        public string? PubKeyType { get; set; }
    }
    private readonly CryptoInfo? cryptoInfo;

    public class InfoResp
    {
        [JsonPropertyName("ready")]
        public bool Ready { get; set; }

        [JsonPropertyName("app")]
        public AppInfo? App { get; set; }

        [JsonPropertyName("server")]
        public ServerInfo? Server { get; set; }

        [JsonPropertyName("crypto")]
        public CryptoInfo? Crypto { get; set; }
    }

    /// <summary>
    /// Returns service's information.
    /// </summary>
    /// <response code="200">Server's information.</response>
    [HttpGet("/info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResp<InfoResp>> Info()
    {
        var data = new InfoResp
        {
            Ready = Global.Ready,
            App = appInfo,
            Server = new ServerInfo
            {
                Env = _env.EnvironmentName,
                Time = DateTime.Now,
            },
            Crypto = cryptoInfo,
        };
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
    [ProducesResponseType(typeof(ApiResp<AuthResp>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("/auth")]
    public ActionResult<ApiResp<AuthResp>> Authenticate([FromBody] AuthReq authReq)
    {
        var resp = Global.Authenticator?.Authenticate(authReq);
        return resp == null
            ? ResponseNoData(500, "No authenticator defined or error while authenticating.")
            : resp.Status == 200
                ? ResponseOk(resp)
                : ResponseNoData(resp.Status, resp.Error);
    }
}