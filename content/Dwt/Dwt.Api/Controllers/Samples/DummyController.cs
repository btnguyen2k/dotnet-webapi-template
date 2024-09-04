﻿using Dwt.Shared.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Controllers.Samples;

public class DummyController(UserManager<DwtUser> userNamager) : ApiBaseController
{
	private readonly Random _random = new();

	/// <summary>
	/// Throws a random test exception.
	/// </summary>
	[HttpGet("/exception")]
	public ActionResult<ApiResp<object>> Health()
	{
		return _random.Next(0, 3) switch
		{
			0 => throw new UnauthorizedAccessException("This is a test UnauthorizedAccessException."),
			1 => throw new KeyNotFoundException("This is a test KeyNotFoundException."),
			2 => throw new NotImplementedException("This is a test NotImplementedException."),
			_ => throw new Exception("This is a test exception."),
		};
	}
}
