using dwt.Helpers;
using dwt.Models;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

[JwtAuthorize]
public class NotesController(IUserRepository userRepo, INoteRepository noteRepo) : DwtBaseController
{
    /// <summary>
    /// Fetches all notes.
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/notes")]
    public ActionResult<List<Note>> GetAll()
    {
        var notes = noteRepo.GetAll();
        return ResponseOk(notes);
    }
}
