using SignalRChatTest.Models;

namespace SignalRChatTest.Service;

public interface IConnectionManager
{
   public Task AddUserAsync(ChatUser user);
   public Task RemoveUserAsync(string userId);
   public Task<ChatUser?> GetUserByIdAsync(string userId);
   public Task AddConnectionAsync(string userId, HubType hubType, string connectionId);
   public Task RemoveConnectionAsync(string userId, HubType hubType, string connectionId);
}