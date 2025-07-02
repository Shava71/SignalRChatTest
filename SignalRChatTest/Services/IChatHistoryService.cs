using SignalRChatTest.DTO;
using SignalRChatTest.Models;

namespace SignalRChatTest.Service;

public interface IChatHistoryService
{
    Task<List<MessageHistoryDto>?> GetChatHistory();
    
    Task AddMessage(ChatMessage message);
}