using Microsoft.Extensions.Caching.Distributed;

namespace Dwt.Shared.Cache;

/// <summary>
/// Extends IDistributedCache functionality with typed methods.
/// </summary>
/// <typeparam name="TCategory"></typeparam>
public interface ICacheFacade<TCategory> : IDistributedCache
{
	T? Get<T>(string key);

	Task<T?> GetAsync<T>(string key, CancellationToken token = default);

	void Set<T>(string key, T value, DistributedCacheEntryOptions options);

	Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default);
}
