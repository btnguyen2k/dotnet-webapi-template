namespace Dwt.Shared.Cache;

/// <summary>
/// API for serializing and deserializing cache entries.
/// </summary>
public interface ICacheEntrySerializer
{
	byte[] Serialize<T>(T value);

	Task<byte[]> SerializeAsync<T>(T value, CancellationToken token = default);

	T? Deserialize<T>(byte[] bytes);

	Task<T?> DeserializeAsync<T>(byte[] bytes, CancellationToken token = default);
}
