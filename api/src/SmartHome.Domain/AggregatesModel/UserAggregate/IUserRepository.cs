namespace SmartHome.Domain.AggregatesModel.UserAggregate;

public interface IUserRepository : IRepository<User>
{
    User Add(User user);

    void Update(User user);

    Task<User?> FindByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<List<User>> GetByIdsAsync(long[] ids, CancellationToken cancellationToken = default);
}
