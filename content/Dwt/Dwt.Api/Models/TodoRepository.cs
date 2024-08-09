using Dwt.Shared.EF;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Models;

/// <summary>
/// (Sample) An implementation of ITodoRepository that use Entity Framework to store todo items.
/// </summary>
public sealed class TodoDbContextRepository(DbContextOptions<TodoDbContextRepository> options)
	: GenericRepository<TodoDbContextRepository, TodoItem>(options), ITodoRepository
{
	public IAsyncEnumerable<TodoItem> GetMyTodos(User user) => DbSet.Where(x => x.UserId == user.Id).AsAsyncEnumerable();
}