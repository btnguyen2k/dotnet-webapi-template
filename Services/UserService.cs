using dwt.Entities;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace dwt.Services;

public interface IUserService
{
    public User? GetUser(string id);

    public IEnumerable<User> GetAll();
}

/// <summary>
/// (Sample) An implementation of IUserService that fetches user accounts from appsettings.json file.
/// </summary>
public class StaticConfigUserService : IUserService
{
    private readonly User[] users;

    public StaticConfigUserService(IConfiguration config)
    {
        users = config.GetSection("Users").Get<User[]>(options => options.BindNonPublicProperties = true) ?? [];
    }

    public User? GetUser(string id)
    {
        return users.FirstOrDefault(u => u?.Id == id);
    }

    public IEnumerable<User> GetAll()
    {
        return users;
    }
}
