using Dwt.Shared.EF;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Models;

/// <summary>
/// (Sample) An implementation of INoteRepository that use Entity Framework to store notes.
/// </summary>
public sealed class NoteDbContextRepository(DbContextOptions<NoteDbContextRepository> options)
	: GenericRepository<NoteDbContextRepository, Note>(options), INoteRepository
{
}
