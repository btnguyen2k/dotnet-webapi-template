using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Services;

/// <summary>
/// A request to add new TodoItem.
/// </summary>
public class NewTodoReq
{
    [BindProperty(Name = "name")]
    public string Name { get; set; } = "";
}
