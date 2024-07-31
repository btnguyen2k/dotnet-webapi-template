using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
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
[Table("todos")]
public class TodoItem
{
    [JsonPropertyName("id")]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("user_id")]
    [Column("user_id")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("name")]
    [Column("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("created")]
    [Column("tcreated")]
    public DateTime Created { get; set; } = DateTime.Now;

    [JsonPropertyName("completed")]
    [Column("tcompleted")]
    public DateTime? Completed { get; set; }
}
