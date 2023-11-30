using StackExchange.Redis;

namespace Cursus.Services.Interfaces;

public interface IRedisService
{
    IDatabase RedisDb { get; }

    Task<T?> GetDataAsync<T>(string key, CommandFlags flags = CommandFlags.None,
        CancellationToken cancellationToken = default);

    Task<bool> SetDataAsync<T>(string key, T value, CommandFlags flags = CommandFlags.None,
        CancellationToken cancellationToken = default);

    Task<bool> SetDataAsync<T>(string key, T value, TimeSpan expiredAfter, CommandFlags flags = CommandFlags.None,
        CancellationToken cancellationToken = default);

    Task RemoveDataAsync(string key, CommandFlags flags = CommandFlags.None,
        CancellationToken cancellationToken = default);
}