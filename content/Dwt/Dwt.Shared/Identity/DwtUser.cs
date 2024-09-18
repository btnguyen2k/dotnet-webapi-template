using Microsoft.AspNetCore.Identity;

namespace Dwt.Shared.Identity;

public class DwtUser : IdentityUser
{
	public IEnumerable<DwtRole>? Roles { get; set; } = default!;
	public IEnumerable<IdentityUserClaim<string>>? Claims { get; set; } = default!;

	/// <summary>
	/// Notifies that the data has changed, updating the <see cref="ConcurrencyStamp"/>.
	/// </summary>
	public void Touch() => ConcurrencyStamp = Guid.NewGuid().ToString();
}
