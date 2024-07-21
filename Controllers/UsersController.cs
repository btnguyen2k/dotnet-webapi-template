using dwt.Helpers;
using dwt.Services;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

[ApiController]
[Consumes("application/json")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("/api/users")]
    [JwtAuthorize]
    public IActionResult GetAll()
    {
        return Ok(new
        {
            status = 200,
            message = "ok",
            data = userService.GetAll()
        });
    }

    [HttpGet("/api/users/{id}")]
    [JwtAuthorize]
    public IActionResult Get(string id)
    {
        var user = userService.GetUser(id);
        if (user == null)
        {
            return NotFound(new
            {
                status = 404,
                message = "User not found"
            });
        }
        return Ok(new
        {
            status = 200,
            message = "ok",
            data = user
        });
    }
}
