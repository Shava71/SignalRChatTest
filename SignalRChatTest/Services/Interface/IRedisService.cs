using Microsoft.EntityFrameworkCore.Storage;
using SignalRChatTest.Models;

namespace SignalRChatTest.Service;

public interface IRedisService
{
    // private readonly IDatabase _redisDatabase;
    public Task SetValueAsync<T>(string key, T value);
    public Task<T?> GetValueAsync<T>(string key);
    public Task<ChatUser> GetUserByIdAsync(string id);
}