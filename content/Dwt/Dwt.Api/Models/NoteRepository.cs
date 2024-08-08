using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Models;


/// <summary>
/// (Sample) An implementation of INoteRepository that use Entity Framework to store notes.
/// </summary>
public class NoteDbContextRepository(DbContextOptions<NoteDbContextRepository> options) : DbContext(options), INoteRepository
{
    protected DbSet<Note> Notes { get; set; } = null!;

    /// <summary>
    /// Async version of Create(Note)
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public async Task<Note> CreateAsync(Note note)
    {
        Notes.Add(note);
        await SaveChangesAsync();
        return note;
    }

    /// <inheritdoc />
    public Note Create(Note note)
    {
        return CreateAsync(note).Result;
    }

    /// <summary>
    /// Async version of GetByID(string)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Note?> GetByIDAsync(string id)
    {
        return await Notes.FindAsync(id);
    }

    /// <inheritdoc />
    public Note? GetByID(string id)
    {
        return GetByIDAsync(id).Result;
    }

    /// <summary>
    /// Async version of GetAll()
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Note>> GetAllAsync()
    {
        return await Notes.ToListAsync();
    }

    /// <inheritdoc />
    public IEnumerable<Note> GetAll()
    {
        return GetAllAsync().Result;
    }

    /// <summary>
    /// Async version of Update(Note)
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(Note note)
    {
        Notes.Update(note);
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Update(Note note)
    {
        return UpdateAsync(note).Result;
    }

    /// <summary>
    /// Async version of Delete(string)
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(Note note)
    {
        Notes.RemoveRange(Notes.Where(x => x.Id == note.Id));
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Delete(Note note)
    {
        return Delete(note);
    }
}
