using Microsoft.EntityFrameworkCore;

namespace dwt.Models;

public interface INoteRepository
{
    /// <summary>
    /// Creates a new note.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public Note Create(Note note);

    /// <summary>
    /// Gets a note by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Note? Get(string id);

    /// <summary>
    /// Updates an existing note.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool Update(Note note);

    /// <summary>
    /// Deletes a note by ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool Delete(string id);

    /// <summary>
    /// Deletes an existing note.
    /// </summary>
    /// <param name="note"></param>
    /// <returns></returns>
    public bool Delete(Note note);

    /// <summary>
    /// Fetches all notes.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Note> GetAll();
}

public class NoteDbContextRepository(DbContextOptions<NoteDbContextRepository> options) : DbContext(options), INoteRepository
{
    protected DbSet<Note> Notes { get; set; } = null!;

    protected async Task<Note> CreateAsync(Note note)
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

    protected async Task<int> DeleteAsync(string id)
    {
        Notes.RemoveRange(Notes.Where(x => x.Id == id));
        return await SaveChangesAsync();
    }

    /// <inheritdoc />
    public bool Delete(string id)
    {
        return DeleteAsync(id).Result > 0;
    }

    /// <inheritdoc />
    public bool Delete(Note note)
    {
        return Delete(note.Id);
    }

    private async Task<Note?> GetAsync(string id)
    {
        return await Notes.FindAsync(id);
    }

    /// <inheritdoc />
    public Note? Get(string id)
    {
        return GetAsync(id).Result;
    }

    protected async Task<bool> UpdateAsync(Note note)
    {
        Notes.Update(note);
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Update(Note note)
    {
        return UpdateAsync(note).Result;
    }

    protected async Task<IEnumerable<Note>> GetAllAsync()
    {
        return await Notes.ToListAsync();
    }

    /// <inheritdoc />
    public IEnumerable<Note> GetAll()
    {
        return GetAllAsync().Result;
    }
}
