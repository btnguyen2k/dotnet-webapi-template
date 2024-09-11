using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Dwt.Shared.Cache;

public class JsonCacheEntrySerializer : ICacheEntrySerializer
{
	/// <inheritdoc/>
	public byte[] Serialize<T>(T value) => JsonSerializer.SerializeToUtf8Bytes(value);

	/// <inheritdoc/>
	public async Task<byte[]> SerializeAsync<T>(T value) => await Task.FromResult(JsonSerializer.SerializeToUtf8Bytes(value));

	/// <inheritdoc/>
	public T? Deserialize<T>(byte[] bytes) => JsonSerializer.Deserialize<T>(bytes);

	/// <inheritdoc/>
	public async Task<T?> DeserializeAsync<T>(byte[] bytes) => await Task.FromResult(JsonSerializer.Deserialize<T>(bytes));
}

public class CacheFacade<TCategory> : ICacheFacade<TCategory>
{
	private readonly IDistributedCache dcache;
	private readonly ICacheEntrySerializer serializer;
	private readonly DistributedCacheEntryOptions defaultOptions;

	public CacheFacade(DistributedCacheEntryOptions defaultOptions, IDistributedCache distributedCache)
		: this(defaultOptions, distributedCache, new JsonCacheEntrySerializer())
	{
	}

	public CacheFacade(DistributedCacheEntryOptions defaultOptions, IDistributedCache distributedCache, ICacheEntrySerializer cacheEntrySerializer)
	{
		ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));
		ArgumentNullException.ThrowIfNull(cacheEntrySerializer, nameof(cacheEntrySerializer));

		this.dcache = distributedCache;
		this.serializer = cacheEntrySerializer;
		this.defaultOptions = defaultOptions ?? new DistributedCacheEntryOptions();
	}

	/// <inheritdoc/>
	public byte[]? Get(string key)
	{
		return dcache.Get(key);
	}

	/// <inheritdoc/>
	public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
	{
		return await dcache.GetAsync(key, token);
	}

	/// <inheritdoc/>
	public T? Get<T>(string key)
	{
		var cached = Get(key);
		return cached == null ? default : serializer.Deserialize<T>(cached);
	}

	/// <inheritdoc/>
	public async Task<T?> GetAsync<T>(string key, CancellationToken token = default)
	{
		var cached = await GetAsync(key, token);
		return cached == null ? default : await serializer.DeserializeAsync<T>(cached);
	}

	/// <inheritdoc/>
	public void Refresh(string key)
	{
		dcache.Refresh(key);
	}

	/// <inheritdoc/>
	public async Task RefreshAsync(string key, CancellationToken token = default)
	{
		await dcache.RefreshAsync(key, token);
	}

	/// <inheritdoc/>
	public void Remove(string key)
	{
		dcache.Remove(key);
	}

	/// <inheritdoc/>
	public async Task RemoveAsync(string key, CancellationToken token = default)
	{
		await dcache.RemoveAsync(key, token);
	}

	/// <inheritdoc/>
	public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
	{
		dcache.Set(key, value, options ?? defaultOptions);
	}

	/// <inheritdoc/>
	public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
	{
		await dcache.SetAsync(key, value, options ?? defaultOptions, token);
	}

	/// <inheritdoc/>
	public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
	{
		Set(key, serializer.Serialize(value), options);
	}

	/// <inheritdoc/>
	public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
	{
		await SetAsync(key, await serializer.SerializeAsync(value), options, token);
	}
}
