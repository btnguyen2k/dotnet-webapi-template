namespace Dwt.Shared.Models;

/// <summary>
/// Generic interface for repositories.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenericRepository<T> where T : class, new()
{
    T Create(T t);

    T? GetByID(string id);

    IEnumerable<T> GetAll();

    bool Update(T t);

	bool Delete(T t);


	ValueTask<T> CreateAsync(T t);

    ValueTask<T?> GetByIDAsync(string id);

    IAsyncEnumerable<T> GetAllAsync();

    ValueTask<bool> UpdateAsync(T t);

    ValueTask<bool> DeleteAsync(T t);
}
