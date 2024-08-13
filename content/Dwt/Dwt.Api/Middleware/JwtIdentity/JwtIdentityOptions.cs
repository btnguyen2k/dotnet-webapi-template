using Dwt.Api.Helpers;

namespace Dwt.Api.Middleware.JwtIdentity;

public class JwtIdentityOptions
{
	public string UserIdKey { get; set; } = GlobalVars.HTTP_CTX_ITEM_USERID;
}
