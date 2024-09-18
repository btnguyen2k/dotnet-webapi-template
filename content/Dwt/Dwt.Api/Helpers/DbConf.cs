namespace Dwt.Api.Helpers;

/// <summary>
/// Represents the database configuration bock in the appsettings.json
/// </summary>
public class DbConf
{
	public const bool DEFAULT_USE_DB_CONTEXT_POOL = false;
	public const int DEFAULT_POOL_SIZE = 128;

	/// <summary>
	/// The type of database backend.
	/// </summary>
	public DbType Type { get; set; }

	/// <summary>
	/// Point to the connection string defined in the ConnectionStrings block.
	/// </summary>
	public string ConnectionString { get; set; } = String.Empty;

	/// <summary>
	/// Should the DbContext be pooled?
	/// </summary>
	public bool UseDbContextPool { get; set; } = DEFAULT_USE_DB_CONTEXT_POOL;

	/// <summary>
	/// Maximum number of DbContext instances in the pool.
	/// </summary>
	public int PoolSize { get; set; } = DEFAULT_POOL_SIZE;
}

public enum DbType
{
	NULL,
	INMEMORY, MEMORY,
	SQLITE,
	SQLSERVER,
}
