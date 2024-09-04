using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dwt.Shared.Identity;
public class DwtRoleManager : RoleManager<DwtRole>
{
	public DwtRoleManager(
		IRoleStore<DwtRole> store,
		IEnumerable<IRoleValidator<DwtRole>> roleValidators,
		ILookupNormalizer keyNormalizer,
		IdentityErrorDescriber errors,
		ILogger<RoleManager<DwtRole>> logger) : base(store, roleValidators, keyNormalizer, errors, logger)
	{
	}
}
