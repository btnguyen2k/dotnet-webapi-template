using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Shared.EF;

/// <summary>
/// (Sample) An abstract EF-implementation of IGenericRepository.
/// </summary>
public abstract class GenericRepository<T, TEntity>(DbContextOptions<T> options) : DbContext(options), IGenericRepository<TEntity>
	where T : DbContext
	where TEntity : class, new()
{
	protected DbSet<TEntity> DbSet { get; set; } = null!;

	/// <inheritdoc/>
	public TEntity Create(TEntity t)
	{
		var result = DbSet.Add(t);
		SaveChanges();
		return result.Entity;
	}

	/// <inheritdoc/>
	public TEntity? GetByID(string id) => DbSet.Find(id);

	/// <inheritdoc/>
	public IEnumerable<TEntity> GetAll() => DbSet;

	/// <inheritdoc/>
	public bool Update(TEntity t)
	{
		DbSet.Update(t);
		return SaveChanges() > 0;
	}

	/// <inheritdoc/>
	public bool Delete(TEntity t)
	{
		DbSet.Remove(t);
		return SaveChanges() > 0;
	}

	/// <inheritdoc/>
	public async ValueTask<TEntity> CreateAsync(TEntity t)
	{
		var result = await DbSet.AddAsync(t);
		await SaveChangesAsync();
		return result.Entity;
	}

	/// <inheritdoc/>
	public ValueTask<TEntity?> GetByIDAsync(string id) => DbSet.FindAsync(id);

	/// <inheritdoc/>
	public IAsyncEnumerable<TEntity> GetAllAsync() => DbSet.AsAsyncEnumerable();

	/// <inheritdoc/>
	public async ValueTask<bool> UpdateAsync(TEntity t)
	{
		DbSet.Update(t);
		return await SaveChangesAsync(CancellationToken.None) > 0;
	}

	/// <inheritdoc/>
	public async ValueTask<bool> DeleteAsync(TEntity t)
	{
		DbSet.Remove(t);
		return await SaveChangesAsync(CancellationToken.None) > 0;
	}
}
