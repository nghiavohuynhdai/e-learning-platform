using System.Text.Json;
using Cursus.Services.Interfaces;
using StackExchange.Redis;
using System.IO;
using System.Text;

namespace Cursus.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _db;

    public RedisService(ConfigurationOptions configurationOptions)
    {
        var redis = ConnectionMultiplexer.Connect(configurationOptions);
        _db = redis.GetDatabase();
    }

    public RedisService(string connectionString)
    {
        var redis = ConnectionMultiplexer.Connect(connectionString);
        _db = redis.GetDatabase();
    }

    public IDatabase RedisDb => _db;

    public async Task<T?> GetDataAsync<T>(string key, CommandFlags flags, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _db.HashGetAsync(key, "data");
            if (!value.HasValue)
                return default;
            T? deserializedValue;
            await using (Stream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(value)))
            {
                deserializedValue =
                    await JsonSerializer.DeserializeAsync<T>(memoryStream, cancellationToken: cancellationToken);
            }

            return deserializedValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return default;
        }
    }

    public async Task<bool> SetDataAsync<T>(string key, T value, CommandFlags flags = CommandFlags.None,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedValue = "";
            await using (Stream memoryStream = new MemoryStream())
            {
                await JsonSerializer.SerializeAsync(memoryStream, value, cancellationToken: cancellationToken);
                memoryStream.Position = 0;
                using (var streamReader = new StreamReader(memoryStream))
                {
                    serializedValue = await streamReader.ReadToEndAsync();
                }
            }

            await _db.HashSetAsync(key, new HashEntry[]
            {
                new("data", serializedValue)
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }


    public async Task<bool> SetDataAsync<T>(string key, T value, TimeSpan expiredAfter, CommandFlags flags,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await SetDataAsync(key, value, flags, cancellationToken);
            await _db.KeyExpireAsync(key, DateTime.Now.Add(expiredAfter).ToUniversalTime());
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
    }

    public Task RemoveDataAsync(string key, CommandFlags flags, CancellationToken cancellationToken = default)
    {
        return _db.KeyDeleteAsync(key);
    }
}