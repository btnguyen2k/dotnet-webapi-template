using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Dwt.Shared.Models;

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

/// <summary>
/// Repository interface for todo items.
/// </summary>
/// <remarks>Entity and repository interface are in the same file just for simplicity only.</remarks>
public interface ITodoRepository : IGenericRepository<TodoItem>
{
    public IAsyncEnumerable<TodoItem> GetMyTodos(User user);
}
