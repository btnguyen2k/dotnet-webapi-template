using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Services;

/// <summary>
/// A request to add new Note or update an existing one.
/// </summary>
public class NewOrUpdateNoteReq
{
    [BindProperty(Name = "title")]
    public string Title { get; set; } = "";

    [BindProperty(Name = "content")]
    public string Content { get; set; } = "";
}
