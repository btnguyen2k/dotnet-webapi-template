namespace dwt.Models;

public interface IUserRepository
{
    public User? GetUser(string id);

    public IEnumerable<User> GetAll();
}

/// <summary>
/// (Sample) An implementation of IUserRepository that fetches user accounts from appsettings.json file.
/// </summary>
public class StaticConfigUserRepository : IUserRepository
{
    private readonly User[] users;

    public StaticConfigUserRepository(IConfiguration config)
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
