namespace Dwt.Api.Helpers;

/// <summary>
/// An application-wide global variables and repository.
/// </summary>
/// <remarks>
///		Use DI whenever possible. This global repository is meant for items that are needed in places where DI is not optimal.
/// </remarks>
public static class GlobalVars
{
	/// <summary>
	/// Reference to the WebApplication instance.
	/// </summary>
	public static WebApplication? App { get; set; }

	/// <summary>
	/// Set to true when the server is ready to handle requests.
	/// </summary>
	public static bool Ready { get; set; }

	///// <summary>
	///// Reference to the authenticator instance to authenticate requests.
	///// </summary>
	//public static IAuthenticator? Authenticator { get; set; }

	/// <summary>
	/// Default name of the HttpContext item for storing the user ID.
	/// </summary>
	public const string HTTP_CTX_ITEM_USERID_DEFAULT = "UserId";

	/// <summary>
	/// Name of the Environment variable for enabling Swagger UI.
	/// </summary>
	public const string ENV_ENABLE_SWAGGER_UI = "ENABLE_SWAGGER_UI";

	/// <summary>
	/// Name of the Environment variable for initializing the database.
	/// </summary>
	public const string ENV_INIT_DB = "INIT_DB";
}

public enum DbType
{
	NULL,
	INMEMORY, MEMORY,
	SQLITE,
	SQLSERVER,
}
