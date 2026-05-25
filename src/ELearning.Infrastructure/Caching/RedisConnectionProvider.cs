using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ELearning.Infrastructure.Caching;

public interface IRedisConnectionProvider
{
    IDatabase? GetDatabase();
    IServer? GetServer();
}

public sealed class RedisConnectionProvider(
    IOptions<RedisOptions> options,
    ILogger<RedisConnectionProvider> logger)
    : IRedisConnectionProvider, IDisposable
{
    private readonly object gate = new();
    private ConnectionMultiplexer? connection;
    private bool connectAttempted;

    public IDatabase? GetDatabase()
    {
        var mux = GetConnection();
        return mux?.IsConnected == true ? mux.GetDatabase() : null;
    }

    public IServer? GetServer()
    {
        var mux = GetConnection();
        if (mux?.IsConnected != true) return null;

        var endpoint = mux.GetEndPoints().FirstOrDefault();
        return endpoint is null ? null : mux.GetServer(endpoint);
    }

    public void Dispose() => connection?.Dispose();

    private ConnectionMultiplexer? GetConnection()
    {
        if (connection?.IsConnected == true)
            return connection;

        lock (gate)
        {
            if (connection?.IsConnected == true)
                return connection;

            var connectionString = options.Value.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                if (!connectAttempted)
                    logger.LogWarning("Redis connection string is not configured.");
                connectAttempted = true;
                return null;
            }

            try
            {
                connection?.Dispose();
                connection = ConnectionMultiplexer.Connect(connectionString);
                connectAttempted = true;
                logger.LogInformation("Redis connection established.");
                return connection;
            }
            catch (Exception ex)
            {
                connectAttempted = true;
                logger.LogWarning(ex, "Redis connection is unavailable.");
                return null;
            }
        }
    }
}
