using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dwt.Shared.Identity;
public class DwtUserManager : UserManager<DwtUser>
{
	public DwtUserManager(
		IUserStore<DwtUser> store,
		IOptions<IdentityOptions> optionsAccessor,
		IPasswordHasher<DwtUser> passwordHasher,
		IEnumerable<IUserValidator<DwtUser>> userValidators,
		IEnumerable<IPasswordValidator<DwtUser>> passwordValidators,
		ILookupNormalizer keyNormalizer,
		IdentityErrorDescriber errors,
		IServiceProvider services,
		ILogger<UserManager<DwtUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
	{
	}
}
