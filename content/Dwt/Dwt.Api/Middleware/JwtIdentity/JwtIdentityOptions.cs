using Dwt.Api.Helpers;

namespace Dwt.Api.Middleware.Jwt;

public class JwtIdentityOptions
{
	public string UserIdKey { get; set; } = GlobalVars.HTTP_CTX_ITEM_USERID;
}
