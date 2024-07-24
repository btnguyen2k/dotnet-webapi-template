using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace dwt.Models;

/// <summary>
/// A request to add new Note.
/// </summary>
public class NewNoteReq
{
    [BindProperty(Name = "title")]
    public string Title { get; set; } = "";

    [BindProperty(Name = "content")]
    public string Content { get; set; } = "";
}

/// <summary>
/// (Sample) This entity represents a note.
/// </summary>
public class Note
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("owner_id")]
    public string OwnerId { get; set; } = "";

    [JsonPropertyName("title")]
    public string Title { get; set; } = "";


    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("created")]
    public DateTime Created { get; set; } = DateTime.Now;

    [JsonPropertyName("updated")]
    public DateTime? Updated { get; set; }
}
