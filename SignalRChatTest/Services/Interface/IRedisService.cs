using Microsoft.EntityFrameworkCore.Storage;

namespace SignalRChatTest.Service;

public interface IRedisService
{
    // private readonly IDatabase _redisDatabase;
    public Task SetValueAsync<T>(string key, T value);
    public Task<T?> GetValueAsync<T>(string key);
}