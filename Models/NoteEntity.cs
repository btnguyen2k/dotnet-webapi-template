using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
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
[Table("notes")]
public class Note
{
    [JsonPropertyName("id")]
    [Column("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("owner_id")]
    [Column("owner_id")]
    public string OwnerId { get; set; } = "";

    [JsonPropertyName("title")]
    [Column("title")]
    public string Title { get; set; } = "";


    [JsonPropertyName("content")]
    [Column("content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("created")]
    [Column("tcreated")]
    public DateTime Created { get; set; } = DateTime.Now;

    [JsonPropertyName("updated")]
    [Column("tupdated")]
    public DateTime? Updated { get; set; }
}
