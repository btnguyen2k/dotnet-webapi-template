using Microsoft.AspNetCore.Identity;

namespace Dwt.Shared.Identity;

public class DwtUser : IdentityUser
{
	public IEnumerable<DwtRole>? Roles { get; set; } = default!;
	public IEnumerable<IdentityUserClaim<string>>? Claims { get; set; } = default!;
}
