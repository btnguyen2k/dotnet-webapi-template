using Microsoft.EntityFrameworkCore;

namespace dwt.Models;

public interface ITodoRepository
{
    public TodoItem Create(TodoItem item);

    public TodoItem? Get(string id);

    public bool Update(TodoItem item);

    public bool Delete(string id);

    public bool Delete(TodoItem item);

    public IEnumerable<TodoItem> GetMyTodos(User user);
}

public class TodoDbContextRepository(DbContextOptions<TodoDbContextRepository> options) : DbContext(options), ITodoRepository
{
    protected DbSet<TodoItem> TodoItems { get; set; } = null!;

    protected async Task<TodoItem> CreateAsync(TodoItem item)
    {
        TodoItems.Add(item);
        await SaveChangesAsync();
        return item;
    }

    /// <inheritdoc />
    public TodoItem Create(TodoItem item)
    {
        return CreateAsync(item).Result;
    }

    protected async Task<int> DeleteAsync(string id)
    {
        TodoItems.RemoveRange(TodoItems.Where(x => x.Id == id));
        return await SaveChangesAsync();
    }

    /// <inheritdoc />
    public bool Delete(string id)
    {
        return DeleteAsync(id).Result > 0;
    }

    /// <inheritdoc />
    public bool Delete(TodoItem item)
    {
        return Delete(item.Id);
    }

    private async Task<TodoItem?> GetAsync(string id)
    {
        return await TodoItems.FindAsync(id);
    }

    /// <inheritdoc />
    public TodoItem? Get(string id)
    {
        return GetAsync(id).Result;
    }

    protected async Task<bool> UpdateAsync(TodoItem item)
    {
        TodoItems.Update(item);
        return await SaveChangesAsync() > 0;
    }

    /// <inheritdoc />
    public bool Update(TodoItem item)
    {
        return UpdateAsync(item).Result;
    }

    protected async Task<IEnumerable<TodoItem>> GetMyTodosAsync(User user)
    {
        return await TodoItems.Where(x => x.UserId == user.Id).ToListAsync();
    }

    /// <inheritdoc />
    public IEnumerable<TodoItem> GetMyTodos(User user)
    {
        return GetMyTodosAsync(user).Result;
    }
}
