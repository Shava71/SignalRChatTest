using SignalRChatTest.DTO;
using SignalRChatTest.Hub;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;
using SignalRChatTest.Repositories;

namespace SignalRChatTest.Service;

public class ChatHistoryService : IChatHistoryService
{
    private readonly IMessageRepository _messageRepository;

    public ChatHistoryService(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<List<MessageHistoryDto>?> GetChatHistory(Guid userId)
    {
        // List<ChatMessage> messages = await _messageRepository.GetAllMessages();
        // List<User> users = await _messageRepository.GettAllUsers();
        //
        // List<MessageHistoryDto> chatHistory = new List<MessageHistoryDto>();
        //
        // var result = messages
        //     .Join(users, message => message.UserId, user => user.Id, (chatMessage, userFrom) => (chatMessage, userFrom))
        //     .Join(users, message => message.chatMessage.Recipient, user => user.Id, (a, userTo) => (a.chatMessage, a.userFrom, userTo))
        //     .Select(mes => new MessageHistoryDto
        //     {
        //         Id = mes.chatMessage.Id,
        //         UserName = mes.userFrom.Username,
        //         Message = mes.chatMessage.Message,
        //         Timestamp = mes.chatMessage.Timestamp,
        //         IsPrivate = mes.chatMessage.IsPrivate,
        //         Recipient = mes.userTo.Username,
        //     })
        //     .ToList();
        var messages = await _messageRepository.GetChatHistory();
        
        messages = messages.Where(res => 
            (res.IsPrivate & (res.UserId == userId || res.RecipientID == userId))
            || !res.IsPrivate)
            .ToList();
        
        return messages.Count > 0 ? messages : null;
    }

    public async Task AddMessage(ChatMessage message)
    {
        await _messageRepository.AddChatMessage(message);
    }
}