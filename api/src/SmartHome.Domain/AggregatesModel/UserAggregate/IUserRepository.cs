namespace SmartHome.Domain.AggregatesModel.UserAggregate;

public interface IUserRepository : IRepository<User>
{
    User Add(User user);

    void Update(User user);

    Task<User?> FindByNameAsync(string name);

    Task<User?> FindByEmailAsync(string email);
}
