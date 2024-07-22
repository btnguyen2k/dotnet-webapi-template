using dwt.Helpers;
using dwt.Models;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

[ApiController]
[Consumes("application/json")]
public class UsersController(IUserRepository userRepo) : ControllerBase
{
    [HttpGet("/api/users")]
    [JwtAuthorize]
    public IActionResult GetAll()
    {
        return Ok(new
        {
            status = 200,
            message = "ok",
            data = userRepo.GetAll()
        });
    }

    private static readonly Dictionary<string, object> _notFound = new()
    {
        { "status", 404 }, { "message", "User not found" }
    };

    [HttpGet("/api/users/{id}")]
    [JwtAuthorize]
    public IActionResult Get(string id)
    {
        var user = userRepo.GetUser(id);
        if (user == null)
        {
            return NotFound(_notFound);
        }
        return Ok(new
        {
            status = 200,
            message = "ok",
            data = user
        });
    }
}
