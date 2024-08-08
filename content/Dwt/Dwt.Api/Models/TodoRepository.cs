using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Models;

/// <summary>
/// (Sample) An implementation of ITodoRepository that use Entity Framework to store todo items.
/// </summary>
public class TodoDbContextRepository(DbContextOptions<TodoDbContextRepository> options) : DbContext(options), ITodoRepository
{
    protected DbSet<TodoItem> TodoItems { get; set; } = null!;

    /// <summary>
    /// Async version of Create(TodoItem)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<TodoItem> CreateAsync(TodoItem item)
    {
        TodoItems.Add(item);
        await SaveChangesAsync();
        return item;
    }

    /// <inheritdoc/>
    public TodoItem Create(TodoItem item)
    {
        return CreateAsync(item).Result;
    }

    /// <summary>
    /// Async version of GetByID(string)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TodoItem?> GetByIDAsync(string id)
    {
        return await TodoItems.FindAsync(id);
    }

    /// <inheritdoc />
    public TodoItem? GetByID(string id)
    {
        return GetByIDAsync(id).Result;
    }

    /// <summary>
    /// Async version of GetAll()
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TodoItem>> GetAllAsync()
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public IEnumerable<TodoItem> GetAll()
    {
        return GetAllAsync().Result;
    }


    /// <summary>
    /// Async version of GetMyTodos(User)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TodoItem>> GetMyTodosAsync(User user)
    {
        return await TodoItems.Where(x => x.UserId == user.Id).ToListAsync();
    }

    /// <inheritdoc />
    public IEnumerable<TodoItem> GetMyTodos(User user)
    {
        return GetMyTodosAsync(user).Result;
    }

    /// <summary>
    /// Async version of Update(TodoItem)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(TodoItem item)
    {
        TodoItems.Update(item);
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Update(TodoItem item)
    {
        return UpdateAsync(item).Result;
    }

    /// <summary>
    /// Async version of Delete(TodoItem)
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(TodoItem item)
    {
        TodoItems.RemoveRange(TodoItems.Where(x => x.Id == item.Id));
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Delete(TodoItem item)
    {
        return DeleteAsync(item).Result;
    }
}
