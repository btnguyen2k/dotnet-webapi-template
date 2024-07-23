using dwt.Helpers;
using dwt.Models;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

public class UsersController(IUserRepository userRepo) : DwtBaseController
{
    [HttpGet("/api/users")]
    [JwtAuthorize]
    public IActionResult GetAll()
    {
        return ResponseOk(userRepo.GetAll());
    }

    [HttpGet("/api/users/{id}")]
    [JwtAuthorize]
    public IActionResult Get(string id)
    {
        var user = userRepo.GetUser(id);
        return user != null ? ResponseOk(user) : _respNotFound;
    }
}
