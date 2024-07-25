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

    /// <summary>
    /// Fetches a note by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("/api/notes/{id}")]
    public ActionResult<Note> GetById(string id)
    {
        var note = noteRepo.Get(id);
        return note != null ? ResponseOk(note) : _respNotFound;
    }

    /// <summary>
    /// Creates a new note.
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpPost("/api/notes")]
    public ActionResult<Note> Create([FromBody] NewOrUpdateNoteReq req)
    {
        var user = userRepo.GetUser(GetRequestUserId() ?? "");
        if (user != null)
        {
            var note = noteRepo.Create(new Note
            {
                OwnerId = user.Id,
                Title = req.Title,
                Content = req.Content,
            });
            return ResponseOk(note);
        }
        return _respAuthenticationRequired;
    }

    /// <summary>
    /// Update an existing note.
    /// </summary>
    /// <param name="req"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("/api/notes/{id}")]
    public ActionResult<Note> Update([FromBody] NewOrUpdateNoteReq req, string id)
    {
        var user = userRepo.GetUser(GetRequestUserId() ?? "");
        if (user != null)
        {
            var note = noteRepo.Get(id);
            if (note != null)
            {
                note.Updated = DateTime.Now;
                note.LastUpdatedUserId = user.Id;
                note.Title = req.Title;
                note.Content = req.Content;
                if (noteRepo.Update(note))
                {
                    return ResponseOk(note);
                }
                return ResponseNotOk(500, "Unknow error occured while updating note.");
            }
            return _respNotFound;
        }
        return _respAuthenticationRequired;
    }

    /// <summary>
    /// Delete an existing note.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <remarks>Only note's owner can delete the note.</remarks>
    [HttpDelete("/api/notes/{id}")]
    public ActionResult<bool> Delete(string id)
    {
        var note = noteRepo.Get(id);
        if (note == null)
        {
            return _respNotFound;
        }

        var user = userRepo.GetUser(GetRequestUserId() ?? "");
        if (user == null || user.Id != note.OwnerId)
        {
            return _respAccessDenied;
        }

        if (noteRepo.Delete(note))
        {
            return ResponseOk(true);
        }

        return ResponseNotOk(500, "Unknow error occured while deleting note.");
    }
}
