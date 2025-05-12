namespace Identity.Api.Services;

public class RedisService : IDisposable
{
    private readonly ConnectionMultiplexer _redis;

    public IDatabase DefaultDatabase { get => GetDatabase(); }

    public RedisService(IConfiguration configuration)
    {
        _redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string is null"));
    }

    public IDatabase GetDatabase(int db = -1, object? asyncState = null)
    {
        return _redis.GetDatabase(db, asyncState);
    }

    public void Dispose()
    {
        _redis.Dispose();
    }
}

public static class RedisKeys
{
    private const string PREFIX = "Identity.Api:";
    public static string Wrap(string key) => $"{PREFIX}key";
}