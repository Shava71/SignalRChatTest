using System.Text.Json;
using SignalRChatTest.Models;
using StackExchange.Redis;
using IDatabase = Microsoft.EntityFrameworkCore.Storage.IDatabase;

namespace SignalRChatTest.Service;

public class RedisService : IRedisService
{
    private readonly StackExchange.Redis.IDatabase _redis;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task SetValueAsync<T>(string key, T value)
    {
        await _redis.StringSetAsync(key, JsonSerializer.Serialize(value));
    }

    public async Task<T?> GetValueAsync<T>(string key)
    {
        var value = await _redis.StringGetAsync(key);

        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
        return default;
    }

    public async Task RemoveValueAsync(string key)
    {
        await _redis.KeyDeleteAsync(key);
    }

    // public async Task<ChatUser> GetUserByIdAsync(string id)
    // {
    //     var value = await _redis.StringGetAsync($"user:{id}:connection");
    //     if (value.HasValue)
    //     {
    //         return JsonSerializer.Deserialize<ChatUser>(value);
    //     }
    //     return default;
    // }
}