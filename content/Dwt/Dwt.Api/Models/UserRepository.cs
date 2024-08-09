using Dwt.Shared.Models;

namespace Dwt.Api.Models;

/// <summary>
/// (Sample) An implementation of IUserRepository that fetches user accounts from appsettings.json file.
/// </summary>
public class StaticConfigUserRepository(IConfiguration config) : IUserRepository
{
    private readonly User[] users = config.GetSection("Users").Get<User[]>(
        options => options.BindNonPublicProperties = true) ?? [];

    /// <inheritdoc/>
    public User Create(User t)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public User? GetByID(string id)
    {
        return users.FirstOrDefault(u => u?.Id == id);
    }

    /// <inheritdoc/>
    public IEnumerable<User> GetAll()
    {
        return users;
    }

    /// <inheritdoc/>
    public bool Update(User t)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public bool Delete(User t)
    {
        throw new NotImplementedException();
    }

    public ValueTask<User> CreateAsync(User t)
    {
	    throw new NotImplementedException();
    }

    public ValueTask<User?> GetByIDAsync(string id)
    {
	    throw new NotImplementedException();
    }

    public IAsyncEnumerable<User> GetAllAsync()
    {
	    throw new NotImplementedException();
    }

    public ValueTask<bool> UpdateAsync(User t)
    {
	    throw new NotImplementedException();
    }

    public ValueTask<bool> DeleteAsync(User t)
    {
	    throw new NotImplementedException();
    }
}
