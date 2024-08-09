using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Shared.EF;

/// <summary>
/// (Sample) An implementation of INoteRepository that use Entity Framework to store notes.
/// </summary>
public abstract class GenericRepository<T, TEntity>(DbContextOptions<T> options) : DbContext(options), IGenericRepository<TEntity>
	where T : DbContext
	where TEntity : class, new()
{
	protected DbSet<TEntity> DbSet { get; set; } = null!;

	public TEntity Create(TEntity t)
	{
		var result = DbSet.Add(t);
		SaveChanges();
		return result.Entity;
	}

	public TEntity? GetByID(string id) => DbSet.Find(id);

	public IEnumerable<TEntity> GetAll() => DbSet;

	public bool Update(TEntity t)
	{
		DbSet.Update(t);
		return SaveChanges() > 0;
	}

	public bool Delete(TEntity t)
	{
		DbSet.Remove(t);
		return SaveChanges() > 0;
	}

	public async ValueTask<TEntity> CreateAsync(TEntity t)
	{
		var result = await DbSet.AddAsync(t);
		await SaveChangesAsync();
		return result.Entity;
	}

	public ValueTask<TEntity?> GetByIDAsync(string id) => DbSet.FindAsync(id);

	public IAsyncEnumerable<TEntity> GetAllAsync() => DbSet.AsAsyncEnumerable();

	public async ValueTask<bool> UpdateAsync(TEntity t)
	{
		DbSet.Update(t);
		return await SaveChangesAsync(CancellationToken.None) > 0;
	}

	public async ValueTask<bool> DeleteAsync(TEntity t)
	{
		DbSet.Remove(t);
		return await SaveChangesAsync(CancellationToken.None) > 0;
	}
}
