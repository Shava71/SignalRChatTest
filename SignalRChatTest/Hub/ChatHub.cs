using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Hub;

[Authorize]
public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    private static Dictionary<string, ChatUser> _connections = new Dictionary<string, ChatUser>();
    private static readonly IRedisService _redis;

    // public async Task RegisterUser(string userid, string username)
    // {
    //     var connectionId = Context.ConnectionId;
    //     var chatUser = new ChatUser()
    //     {
    //         Id = Guid.Parse(userid),
    //         Username = username,
    //         ConnectionId = connectionId
    //     };
    //     _connections[connectionId] = chatUser;
    //     
    //     await Clients.All.SendAsync("UpdateUserList", _connections.Values.Select(u => new
    //     {
    //         Username = u.Username,
    //         Id = u.Id.ToString(),
    //     }));
    // }
    
    public async Task SendMessage(string name, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", name, message);
    }

    public async Task SendPrivateMessage(string toUser, string message)
    {
        ChatUser _fromUser = _connections.FirstOrDefault(x => x.Key == Context.ConnectionId).Value;
        
        ChatUser _toUser = _connections.FirstOrDefault(x => x.Value.Id == Guid.Parse(toUser)).Value;
        
        await Clients.Client(_toUser.ConnectionId).SendAsync("ReceivePrivateMessage", _fromUser, message);
        await Clients.Caller.SendAsync("ReceivePrivateMessage", _fromUser, message);
       
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