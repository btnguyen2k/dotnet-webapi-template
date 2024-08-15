using Dwt.Api.Helpers;
using Dwt.Api.Services;
using Dwt.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Dwt.Api.Controllers.Samples;

[JwtAuthorize]
public class TodosController(IUserRepository userRepo, ITodoRepository todoRepo) : ApiBaseController
{
	/// <summary>
	/// Fetches all todos for the authenticated user.
	/// </summary>
	/// <returns></returns>
	[HttpGet("/api/todos")]
	public ActionResult<ApiResp<List<TodoItem>>> GetMyTodos()
	{
		var user = userRepo.GetByID(GetRequestUserId() ?? "");
		if (user != null)
		{
			var todoList = todoRepo.GetMyTodos(user);
			return ResponseOk(todoList);
		}
		return _respAuthenticationRequired;
	}

	/// <summary>
	/// Fetches a todo item by ID.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	/// <remarks>Users can only see their own todo items.</remarks>
	[HttpGet("/api/todos/{id}")]
	public ActionResult<ApiResp<TodoItem>> GetByID(string id)
	{
		var user = userRepo.GetByID(GetRequestUserId() ?? "");
		if (user != null)
		{
			var todoItem = todoRepo.GetByID(id);
			if (todoItem != null && todoItem.UserId == user.Id)
			{
				// return the todo item only if it exists and belongs to the user
				return ResponseOk(todoItem);
			}
			return _respNotFound;
		}
		return _respAuthenticationRequired;
	}

	/// <summary>
	/// Marks a todo item as completed.
	/// </summary>
	/// <param name="id"></param>
	/// <returns></returns>
	/// <remarks>Users can only mark their own todo items as completed.</remarks>
	[HttpPost("/api/todos/{id}/completed")]
	public ActionResult<ApiResp<TodoItem>> MarkCompleted(string id)
	{
		var user = userRepo.GetByID(GetRequestUserId() ?? "");
		if (user != null)
		{
			var todoItem = todoRepo.GetByID(id);
			if (todoItem != null && todoItem.UserId == user.Id)
			{
				todoItem.Completed = DateTime.Now;
				if (todoRepo.Update(todoItem))
				{
					return ResponseOk(todoItem);
				}
				return ResponseNoData(500, "Unknow error occured while updating todo.");
			}
			return _respNotFound;
		}
		return _respAuthenticationRequired;
	}

	/// <summary>
	/// Creates a new todo item.
	/// </summary>
	/// <param name="req"></param>
	/// <returns></returns>
	[HttpPost("/api/todos")]
	public ActionResult<ApiResp<TodoItem>> Create([FromBody] NewTodoReq req)
	{
		var user = userRepo.GetByID(GetRequestUserId() ?? "");
		if (user != null)
		{
			var todoItem = todoRepo.Create(new TodoItem
			{
				UserId = user.Id,
				Name = req.Name,
			});
			return ResponseOk(todoItem);
		}
		return _respAuthenticationRequired;
	}

	/* No DELETE action is supported for now */
}
