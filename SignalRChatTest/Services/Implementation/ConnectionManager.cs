using System.Text.Json;
using SignalRChatTest.Models;

namespace SignalRChatTest.Service;

public class ConnectionManager : IConnectionManager
{
    private IRedisService _redisService;

    private const string Prefix = "user:";
    private const string Postfix = ":connection";

    public ConnectionManager(IRedisService redisService)
    {
        _redisService = redisService;
    }

    public async Task AddUserAsync(ChatUser user)
    {
        // var serialized = JsonSerializer.Serialize(user);
        await _redisService.SetValueAsync($"{Prefix}{user.Id.ToString()}{Postfix}", user);
    }

    public async Task RemoveUserAsync(string userId)
    {
        await _redisService.RemoveValueAsync($"{Prefix}{userId}{Postfix}");
    }

    public async Task AddConnectionAsync(string userId, HubType hubType, string connectionId)
    {
        ChatUser? user = await GetUserByIdAsync(userId);
        
        await user.AddConnectionId(hubType, connectionId);
        await _redisService.SetValueAsync($"{Prefix}{userId}{Postfix}", user);
    }
    
    public async Task RemoveConnectionAsync(string userId, HubType hubType, string connectionId)
    {
        ChatUser? user = await GetUserByIdAsync(userId);
        
        await user.RemoveConnectionId(hubType, connectionId);
        await _redisService.SetValueAsync($"{Prefix}{userId}{Postfix}", user);
    }
    
    public async Task<ChatUser?> GetUserByIdAsync(string userId)
    {
        ChatUser? user = await _redisService.GetValueAsync<ChatUser>($"{Prefix}{userId}{Postfix}");
        return user;
    }
    
    
}