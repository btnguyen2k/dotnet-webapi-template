namespace Dwt.Shared.Models;

/// <summary>
/// Generic interface for repositories.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenericRepository<TEntity, TKey> where TEntity : class, new() where TKey : IEquatable<TKey>
{
	TEntity Create(TEntity t);

	TEntity? GetByID(TKey id);

	IEnumerable<TEntity> GetAll();

	bool Update(TEntity t);

	bool Delete(TEntity t);

	ValueTask<TEntity> CreateAsync(TEntity t);

	ValueTask<TEntity?> GetByIDAsync(TKey id);

	IAsyncEnumerable<TEntity> GetAllAsync();

	ValueTask<bool> UpdateAsync(TEntity t);

	ValueTask<bool> DeleteAsync(TEntity t);
}
