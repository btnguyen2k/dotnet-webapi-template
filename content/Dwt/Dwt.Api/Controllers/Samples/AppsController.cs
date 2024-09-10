using Dwt.Api.Services;
using Dwt.Shared.Identity;
using Dwt.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Dwt.Api.Controllers.Samples;

public class AppsController : ApiBaseController
{
	private readonly IApplicationRepository appRepo;

	public AppsController(IApplicationRepository appRepo)
	{
		ArgumentNullException.ThrowIfNull(appRepo, nameof(appRepo));

		this.appRepo = appRepo;
	}

	public struct AppResponse
	{
		public string Id { get; set; }

		[JsonPropertyName("display_name")]
		public string DisplayName { get; set; }

		[JsonPropertyName("public_key_pem")]
		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? PublicKeyPEM { get; set; }

		[JsonPropertyName("creation_time")]
		public DateTimeOffset CreationTime { get; set; }
	}

	private static AppResponse ToAppResponse(Application app)
	{
		return new AppResponse
		{
			Id = app.Id,
			DisplayName = app.DisplayName,
			PublicKeyPEM = app.PublicKeyPEM,
			CreationTime = app.CreationTime
		};
	}

	public struct CreateAppReq
	{
		[JsonPropertyName("display_name")]
		public string DisplayName { get; set; }

		[JsonPropertyName("public_key_pem")]
		public string? PublicKeyPEM { get; set; }
	}

	/// <summary>
	/// Creates a new application.
	/// </summary>
	/// <param name="req"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpPost("/api/apps")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[Authorize(Policy = DwtIdentity.POLICY_NAME_ADMIN_OR_CREATE_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResponse>>> CreateApp(
		[FromBody] CreateAppReq req,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var result = await appRepo.CreateAsync(new Application
		{
			DisplayName = req.DisplayName,
			PublicKeyPEM = req.PublicKeyPEM
		});
		return ResponseOk(ToAppResponse(result));
	}

	/// <summary>
	/// Fetches all applications.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/apps")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResp<IEnumerable<AppResponse>>>> GetAll()
	{
		var apps = appRepo.GetAllAsync();
		var result = new List<AppResponse>();
		await foreach (var item in apps)
		{
			result.Add(ToAppResponse(item));
		}
		return ResponseOk(result);
	}

	/// <summary>
	/// Fetches an application by ID.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	[HttpGet("/api/apps/{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<ApiResp<AppResponse>>> GetById(string id)
	{
		var app = await appRepo.GetByIDAsync(id);
		if (app == null)
		{
			return _respNotFound;
		}
		return ResponseOk(ToAppResponse(app));
	}

	/// <summary>
	/// Deletes an existing application.
	/// </summary>
	/// <param name="id"></param>
	/// <param name="authenticator"></param>
	/// <param name="authenticatorAsync"></param>
	/// <returns></returns>
	[HttpDelete("/api/apps/{id}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[Authorize(Policy = DwtIdentity.POLICY_NAME_ADMIN_OR_DELETE_APP_PERM)]
	public async Task<ActionResult<ApiResp<AppResponse>>> CreateApp(string id,
		IAuthenticator? authenticator, IAuthenticatorAsync? authenticatorAsync)
	{
		if (authenticator == null && authenticatorAsync == null)
		{
			throw new ArgumentNullException("No authenticator defined.", (Exception?)null);
		}

		var jwtToken = GetAuthToken();
		var tokenValidationResult = await ValidateAuthTokenAsync(authenticator, authenticatorAsync, jwtToken!);
		if (tokenValidationResult.Status != 200)
		{
			return ResponseNoData(403, tokenValidationResult.Error);
		}

		var app = await appRepo.GetByIDAsync(id);
		if (app == null)
		{
			return _respNotFound;
		}
		var result = await appRepo.DeleteAsync(app);
		if (!result)
		{
			return ResponseNoData(500, "Failed to delete application.");
		}
		return ResponseOk(ToAppResponse(app));
	}
}
