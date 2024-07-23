using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace dwt.Models;

/// <summary>
/// A request to add new TodoItem.
/// </summary>
public class NewTodoReq
{
    [BindProperty(Name = "name")]
    public string Name { get; set; } = "";
}

/// <summary>
/// (Sample) This entity represents an item in a todo list.
/// </summary>
public class TodoItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.Now;

    [JsonPropertyName("completed")]
    public DateTime? Completed { get; set; }
}
