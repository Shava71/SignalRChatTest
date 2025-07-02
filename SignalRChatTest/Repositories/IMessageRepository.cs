using SignalRChatTest.DTO;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;

namespace SignalRChatTest.Repositories;

public interface IMessageRepository
{
    Task AddChatMessage(ChatMessage message);
    Task<List<ChatMessage>> GetAllMessages();
    Task<List<User>> GettAllUsers();
    Task<List<MessageHistoryDto>> GetChatHistory();
}