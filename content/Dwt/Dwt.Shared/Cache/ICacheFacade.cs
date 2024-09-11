using Microsoft.Extensions.Caching.Distributed;

namespace Dwt.Shared.Cache;

public interface ICacheEntrySerializer
{
	byte[] Serialize<T>(T value);

	Task<byte[]> SerializeAsync<T>(T value);

	T? Deserialize<T>(byte[] bytes);

	Task<T?> DeserializeAsync<T>(byte[] bytes);
}

public interface ICacheFacade<TCategory> : IDistributedCache
{
	T? Get<T>(string key);

	Task<T?> GetAsync<T>(string key, CancellationToken token = default);

	void Set<T>(string key, T value, DistributedCacheEntryOptions options);

	Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default);
}
