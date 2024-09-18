using Microsoft.Extensions.Caching.Distributed;
using System.IO.Compression;

namespace Dwt.Shared.Cache;

public class CacheFacadeOptions
{
	public static CacheFacadeOptions DEFAULT { get; set; } = new CacheFacadeOptions();

	public DistributedCacheEntryOptions DefaultDistributedCacheEntryOptions { get; set; } = new DistributedCacheEntryOptions();
	public ICacheEntrySerializer CacheEntrySerializer { get; set; } = new JsonCacheEntrySerializer();
	public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.NoCompression;
	public string KeyPrefix { get; set; } = "";
}

public class CacheFacade<TCategory> : ICacheFacade<TCategory>
{
	private readonly IDistributedCache dcache;
	private readonly ICacheEntrySerializer serializer;
	private readonly DistributedCacheEntryOptions defaultOptions;
	private readonly ICompressor compressor;
	private readonly string keyPrefix;

	public CacheFacade(IDistributedCache distributedCache) : this(distributedCache, CacheFacadeOptions.DEFAULT) { }

	public CacheFacade(IDistributedCache distributedCache, CacheFacadeOptions options)
	{
		ArgumentNullException.ThrowIfNull(distributedCache, nameof(distributedCache));

		this.dcache = distributedCache;
		this.serializer = options?.CacheEntrySerializer ?? new JsonCacheEntrySerializer();
		this.defaultOptions = options?.DefaultDistributedCacheEntryOptions ?? new DistributedCacheEntryOptions();
		this.compressor = options == null || options.CompressionLevel == CompressionLevel.NoCompression
			? new NoCompressionCompressor()
			: options.CompressionLevel == CompressionLevel.Fastest || options.CompressionLevel == CompressionLevel.Optimal
				? new BrotliCompressor(options.CompressionLevel)
				: new DeflateCompressor(options.CompressionLevel);
		this.keyPrefix = options?.KeyPrefix ?? "";
	}

	/// <inheritdoc/>
	public byte[]? Get(string key)
	{
		var cached = dcache.Get($"{keyPrefix}{key}");
		return cached == null ? null : compressor.DecompressAsync(cached).Result;
	}

	/// <inheritdoc/>
	public async Task<byte[]?> GetAsync(string key, CancellationToken token = default)
	{
		var cached = await dcache.GetAsync($"{keyPrefix}{key}", token);
		return cached == null ? null : await compressor.DecompressAsync(cached, token);
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
		return cached == null ? default : await serializer.DeserializeAsync<T>(cached, token);
	}

	/// <inheritdoc/>
	public void Refresh(string key)
	{
		dcache.Refresh($"{keyPrefix}{key}");
	}

	/// <inheritdoc/>
	public async Task RefreshAsync(string key, CancellationToken token = default)
	{
		await dcache.RefreshAsync($"{keyPrefix}{key}", token);
	}

	/// <inheritdoc/>
	public void Remove(string key)
	{
		dcache.Remove($"{keyPrefix}{key}");
	}

	/// <inheritdoc/>
	public async Task RemoveAsync(string key, CancellationToken token = default)
	{
		await dcache.RemoveAsync($"{keyPrefix}{key}", token);
	}

	/// <inheritdoc/>
	public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
	{
		dcache.Set($"{keyPrefix}{key}", compressor.CompressAsync(value).Result, options ?? defaultOptions);
	}

	/// <inheritdoc/>
	public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
	{
		await dcache.SetAsync($"{keyPrefix}{key}", await compressor.CompressAsync(value, token), options ?? defaultOptions, token);
	}

	/// <inheritdoc/>
	public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
	{
		Set(key, serializer.Serialize(value), options);
	}

	/// <inheritdoc/>
	public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
	{
		await SetAsync(key, await serializer.SerializeAsync(value, token), options, token);
	}
}

/*------------------------------------------------------------*/

internal interface ICompressor
{
	Task<byte[]> CompressAsync(byte[] data, CancellationToken token = default);
	Task<byte[]> DecompressAsync(byte[] data, CancellationToken token = default);
}

internal sealed class NoCompressionCompressor : ICompressor
{
	public async Task<byte[]> CompressAsync(byte[] data, CancellationToken token = default) => await Task.FromResult(data);
	public async Task<byte[]> DecompressAsync(byte[] data, CancellationToken token = default) => await Task.FromResult(data);
}

internal sealed class BrotliCompressor(CompressionLevel compressionLevel) : ICompressor
{
	public async Task<byte[]> CompressAsync(byte[] data, CancellationToken token = default)
	{
		using var stream = new MemoryStream();
		using var compressor = new BrotliStream(stream, compressionLevel);
		await compressor.WriteAsync(data, token);
		compressor.Close();
		return stream.ToArray();
	}

	public async Task<byte[]> DecompressAsync(byte[] data, CancellationToken token = default)
	{
		using var stream = new MemoryStream();
		using var decompressor = new BrotliStream(new MemoryStream(data), CompressionMode.Decompress);
		await decompressor.CopyToAsync(stream, token);
		return stream.ToArray();
	}
}

internal sealed class DeflateCompressor(CompressionLevel compressionLevel) : ICompressor
{
	public async Task<byte[]> CompressAsync(byte[] data, CancellationToken token = default)
	{
		using var stream = new MemoryStream();
		using var compressor = new DeflateStream(stream, compressionLevel);
		await compressor.WriteAsync(data, token);
		compressor.Close();
		return stream.ToArray();
	}

	public async Task<byte[]> DecompressAsync(byte[] data, CancellationToken token = default)
	{
		using var stream = new MemoryStream();
		using var decompressor = new DeflateStream(new MemoryStream(data), CompressionMode.Decompress);
		await decompressor.CopyToAsync(stream, token);
		return stream.ToArray();
	}
}
