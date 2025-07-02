using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRChatTest.DTO;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Hub;

[Authorize]
public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    private static Dictionary<string, ChatUser> _connections = new Dictionary<string, ChatUser>();
    private IRedisService _redis;
    private IChatHistoryService _chatHistoryService;

    public ChatHub(IChatHistoryService chatHistoryService)
    {
        // _redis = redis;
        _chatHistoryService = chatHistoryService;
    }
    
    public async Task GetMessageHistory()
    {
        List<MessageHistoryDto>? messages = await _chatHistoryService.GetChatHistory();
        await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);
    }
    
    public async Task SendMessage(string name, string message)
    {
        ChatUser _fromUser = _connections.FirstOrDefault(x => x.Key == Context.ConnectionId).Value;
        ChatMessage chatMessage = new()
        {
            Id = Guid.NewGuid(),
            UserId = _fromUser.Id,
            Message = message,
            Timestamp = DateTime.UtcNow,
            IsPrivate = false,
            Recipient = null,
        };
        
        await _chatHistoryService.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", name, message, chatMessage.Timestamp);
    }

    public async Task SendPrivateMessage(string toUser, string message)
    {
        ChatUser _fromUser = _connections.FirstOrDefault(x => x.Key == Context.ConnectionId).Value;
        
        ChatUser _toUser = _connections.FirstOrDefault(x => x.Value.Id == Guid.Parse(toUser)).Value;

        ChatMessage chatMessage = new()
        {
            Id = Guid.NewGuid(),
            UserId = _fromUser.Id,
            Message = message,
            Timestamp = DateTime.UtcNow,
            IsPrivate = true,
            Recipient = _toUser.Id,
        };
        
        await _chatHistoryService.AddMessage(chatMessage);
        
        await Clients.Client(_toUser.ConnectionId).SendAsync("ReceivePrivateMessage", _fromUser, message, chatMessage.Timestamp);
        await Clients.Caller.SendAsync("ReceivePrivateMessage", _fromUser, message, chatMessage.Timestamp);
       
    }
    
    public override async Task OnConnectedAsync()
    {
        string userid = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string username = Context.User.FindFirst(ClaimTypes.Name)!.Value;
        
        var connectionId = Context.ConnectionId;
        var chatUser = new ChatUser()
        {
            Id = Guid.Parse(userid),
            Username = username,
            ConnectionId = connectionId
        };
        _connections[connectionId] = chatUser;
        
        await Clients.All.SendAsync("UpdateUserList", _connections.Values.Select(u => new
        {
            Username = u.Username,
            Id = u.Id.ToString(),
        }));
        
        await Clients.All.SendAsync("UserConnected", chatUser.Username);
        await base.OnConnectedAsync();
    }
        
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        // await Clients.All.SendAsync("UserDisconnected", Context.ConnectionId);
        // await base.OnDisconnectedAsync(exception);
        if (_connections.TryGetValue(Context.ConnectionId, out var user))
        {
            _connections.Remove(Context.ConnectionId);
            await Clients.All.SendAsync("UpdateUserList", _connections.Values.Select(u => new
            {
                Username = u.Username,
                Id = u.Id.ToString(),
            }));
         
            await Clients.All.SendAsync("UserDisconnected", user.Username);
        }

        await base.OnDisconnectedAsync(exception);
    }
    
    // Добавление клиента в группу
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        await Clients.Group(roomName).SendAsync("UserJoined", Context.ConnectionId);
    }
}