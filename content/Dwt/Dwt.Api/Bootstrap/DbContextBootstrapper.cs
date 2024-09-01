using Dwt.Api.Helpers;
using Dwt.Api.Models;
using Dwt.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Dwt.Api.Bootstrap;

/// <summary>
/// Built-in bootstrapper that initializes DbContext/DbContextPool services.
/// </summary>
[Bootstrapper]
public class DbContextBootstrapper
{
	public static void ConfigureBuilder(WebApplicationBuilder appBuilder)
	{
		var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<DbContextBootstrapper>();

		logger.LogInformation("Configuring DbContext services...");

		appBuilder.Services.AddSingleton<IUserRepository, StaticConfigUserRepository>();
		logger.LogInformation("StaticConfigUserRepository --> IUserRepository.");

		var tryParse = bool.TryParse(Environment.GetEnvironmentVariable(GlobalVars.ENV_INIT_DB), out var initDb);

		appBuilder.Services.AddDbContext<ITodoRepository, TodoDbContextRepository>(options =>
		{
			options.UseInMemoryDatabase("TodoList");
			if (appBuilder.Environment.IsDevelopment() || tryParse && initDb)
			{
				var optBuilder = (DbContextOptionsBuilder<TodoDbContextRepository>)options;
				using var dbContext = new TodoDbContextRepository(optBuilder.Options);
				dbContext.Database.EnsureCreatedAsync();
				logger.LogInformation("EnsureCreated() is called for TodoDbContextRepository.");
			}
		});
		logger.LogInformation("TodoDbContextRepository --> ITodoRepository.");

		appBuilder.Services.AddDbContextPool<INoteRepository, NoteDbContextRepository>(options =>
		{
			var connStr = appBuilder.Configuration.GetConnectionString("NotesDbContext");
			options.UseSqlite(connStr);
			if (appBuilder.Environment.IsDevelopment() || tryParse && initDb)
			{
				var optBuilder = (DbContextOptionsBuilder<NoteDbContextRepository>)options;
				using var dbContext = new NoteDbContextRepository(optBuilder.Options);
				dbContext.Database.EnsureCreated();
				logger.LogInformation("EnsureCreated() is called for NoteDbContextRepository.");
			}
		});
		logger.LogInformation("NoteDbContextRepository --> INoteRepository.");
	}
}
