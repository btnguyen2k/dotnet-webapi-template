using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace dwt.Services;


/// <summary>
/// Response to an API call.
/// </summary>
public class ApiResp<T>
{
    //public static ApiResp<object> Ok() => new ApiResp<object>
    //{
    //    Status = 200,
    //    Message = "Ok.",
    //};

    //public static ApiResp<object> Ok(string message) => new ApiResp<object>
    //{
    //    Status = 200,
    //    Message = message,
    //};

    //public static ApiResp<T> Ok(string message, T data) => new ApiResp<T>
    //{
    //    Status = 200,
    //    Message = message,
    //    Data = data,
    //};

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
