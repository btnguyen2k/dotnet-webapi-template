using System.IO.Compression;

namespace Dwt.Api.Helpers;

/// <summary>
/// Represents the cache configuration block in the appsettings.json
/// </summary>
public class CacheConf
{
	public const int DEFAULT_SIZE_LIMIT = 100 * 1024 * 1024; // ~100mb

	/// <summary>
	/// The type of cache to use.
	/// </summary>
	public CacheType Type { get; set; }

	/// <summary>
	/// Cache key prefix (Redis caches only).
	/// </summary>
	public string KeyPrefix { get; set; } = String.Empty;

	/// <summary>
	/// The size limit of the cache (in bytes, in-memory caches only).
	/// </summary>
	public int SizeLimit { get; set; } = DEFAULT_SIZE_LIMIT;

	/// <summary>
	/// Point to the connection string defined in the ConnectionStrings block (Redis caches only).
	/// </summary>
	public string ConnectionString { get; set; } = String.Empty;

	/// <summary>
	/// Cache entries expire after the specified period, in seconds. Set to 0 to disable expiration.
	/// </summary>
	public int ExpirationAfterAccess { get; set; } = 0;

	/// <summary>
	/// Cache entries expire after the specified period of no access, in seconds. Set to 0 to disable expiration.
	/// </summary>
	public int ExpirationAfterWrite { get; set; } = 0;

	public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;
}

public enum CacheType
{
	NONE,
	INMEMORY, MEMORY,
	REDIS,
}
