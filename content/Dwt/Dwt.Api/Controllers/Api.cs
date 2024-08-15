using System.Text.Json.Serialization;

namespace Dwt.Api.Controllers;

/// <summary>
/// Response to an API call.
/// </summary>
public class ApiResp<T>
{
    /// <summary>
    /// Status of the API call, following HTTP status codes convention.
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }

    /// <summary>
    /// Extra information if any (e.g. the detailed error message).
    /// </summary>
    [JsonPropertyName("message")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }

    /// <summary>
    /// The data returned by the API call (specific to individual API).
    /// </summary>
    [JsonPropertyName("data")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; set; }

    /// <summary>
    /// Extra data if any.
    /// </summary>
    [JsonPropertyName("extras")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Extras { get; set; }

    /// <summary>
    /// Debug information if any.
    /// </summary>
    [JsonPropertyName("debug_info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? DebugInfo { get; set; }
}
