
namespace SmartHome.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SmartHomeContext _smartHomeContext;

    public IUnitOfWork UnitOfWork => _smartHomeContext;

    public UserRepository(SmartHomeContext smartHomeContext)
    {
        _smartHomeContext = smartHomeContext ?? throw new ArgumentNullException(nameof(smartHomeContext));
    }

    public User Add(User user)
    {
        return _smartHomeContext.Users.Add(user).Entity;
    }

    public void Update(User user)
    {
        _smartHomeContext.Entry(user).State = EntityState.Modified;
    }

    public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Users.FirstOrDefaultAsync(u => u.Name == name, cancellationToken);
    }

    public Task<List<User>> GetByIdsAsync(long[] ids, CancellationToken cancellationToken = default)
    {
        return _smartHomeContext.Users.Where(u => ids.Contains(u.Id)).ToListAsync();
    }
}
