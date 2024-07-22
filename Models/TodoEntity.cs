using System.Text.Json.Serialization;

namespace dwt.Models;

/// <summary>
/// (Sample) This entity represents an item in a todo list.
/// </summary>
public class TodoItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.Now;

    [JsonPropertyName("completed")]
    public DateTime? Completed { get; set; }
}
