using dwt.Helpers;
using dwt.Models;
using Microsoft.AspNetCore.Mvc;

namespace dwt.Controllers;

[JwtAuthorize]
public class TodosController(IUserRepository userRepo, ITodoRepository todoRepo) : DwtBaseController
{
    [HttpGet("/api/todos")]
    public ActionResult<List<TodoItem>> GetMyTodo()
    {
        if (HttpContext.Items.TryGetValue(Global.HTTP_CTX_ITEM_USERID, out var userId) && userId != null)
        {
            var user = userRepo.GetUser(userId.ToString());
            if (user != null)
            {
                var todoList = todoRepo.GetMyTodos(user);
                return ResponseOk(todoList);
            }
            return _respAuthenticationRequired;
        }
        return _respNotFound;
    }

    [HttpGet("/api/todos/{id}")]
    public ActionResult<TodoItem> Get(string id)
    {
        if (HttpContext.Items.TryGetValue(Global.HTTP_CTX_ITEM_USERID, out var userId) && userId != null)
        {
            var todoItem = todoRepo.Get(id);
            if (todoItem != null && todoItem.UserId == userId.ToString())
            {
                // return the todo item only if it exists and belongs to the user
                return ResponseOk(todoItem);
            }
        }
        return _respNotFound;
    }

    [HttpPost("/api/todos")]
    public ActionResult<TodoItem> Create([FromBody] NewTodoReq req)
    {
        if (HttpContext.Items.TryGetValue(Global.HTTP_CTX_ITEM_USERID, out var userId) && userId != null)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            var todoItem = todoRepo.Create(new TodoItem
            {
                UserId = userId.ToString(),
                Name = req.Name,
            });
#pragma warning restore CS8601 // Possible null reference assignment.
            return ResponseOk(todoItem);
        }
        return _respAuthenticationRequired;
    }
}
