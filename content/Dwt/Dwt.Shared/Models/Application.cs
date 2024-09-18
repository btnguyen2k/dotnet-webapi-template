namespace Dwt.Shared.Models;
public class Application
{
	public Application()
	{
		Id = Guid.NewGuid().ToString();
	}

	/// <summary>
	/// Application's unique ID.
	/// </summary>
	public string Id { get; set; } = default!;

	/// <summary>
	/// Application's display name.
	/// </summary>
	public string DisplayName { get; set; } = default!;

	/// <summary>
	/// Application's public key in PEM format.
	/// </summary>
	public string? PublicKeyPEM { get; set; }

	/// <summary>
	/// The time at which the application was created.
	/// </summary>
	public DateTimeOffset CreationTime { get; set; } = DateTimeOffset.UtcNow;

	/// <summary>
	/// A random value that must change whenever an application is persisted to the store.
	/// </summary>
	public string? ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

	public override string ToString() => DisplayName ?? string.Empty;

	/// <summary>
	/// Notifies that the data has changed, updating the <see cref="ConcurrencyStamp"/>.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();
}

public interface IApplicationRepository : IGenericRepository<Application, string>
{
}
