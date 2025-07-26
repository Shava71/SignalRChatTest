using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Hub;

[Authorize]
public class VoiceHub : Microsoft.AspNetCore.SignalR.Hub
{
    // private IRedisService _redis;
    private IConnectionManager _connectionManager;
    
    public VoiceHub(IConnectionManager connectionManager)
    {
        // _redis = redis;
        _connectionManager = connectionManager;
    }

    // public async Task SendSignal(string message)
    // {
    //     var username = Context.User.FindFirst(ClaimTypes.Name)?.Value;
    //     await Clients.Others.SendAsync("ReceiveSignal", username, message);
    // }
    public async Task SendSignal(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Received null or empty signal from client.");
            return;
        }

        var username = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        await Clients.Others.SendAsync("ReceiveSignal", username, message);
    }

    public async Task CallUser(string toUserId)
    {
        string fromUserName = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        string? fromUserId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // ChatUser toUser = await _redis.GetUserByIdAsync(toUserId);
        ChatUser? toUser = await _connectionManager.GetUserByIdAsync(toUserId);
        Console.WriteLine($"You call to: {toUser.Username} ({toUser.Id}): conId = {toUser.ConnectionId}");
        foreach (string con in await toUser.GetConnectionIds(HubType.Voice))
        {
            await Clients.User(con).SendAsync("IncomingCall", fromUserName, fromUserId);   
        }
    }

    public async Task AcceptCall(string callerId)
    {
        
        // ChatUser toUser = await _redis.GetUserByIdAsync(callerId);
        // await Clients.User(toUser.ConnectionId).SendAsync("CallAccepted");
        ChatUser? toUser = await _connectionManager.GetUserByIdAsync(callerId);
        foreach (string con in await toUser.GetConnectionIds(HubType.Voice))
        {
            await Clients.User(con).SendAsync("CallAccepted");
        }
    }

    public async Task DeclineCall(string callerId)
    {
        // ChatUser toUser = await _redis.GetUserByIdAsync(callerId);
        // await Clients.User(toUser.ConnectionId).SendAsync("CallDeclined");
        
        ChatUser? toUser = await _connectionManager.GetUserByIdAsync(callerId);
        foreach (string con in await toUser.GetConnectionIds(HubType.Voice))
        {
            await Clients.User(con).SendAsync("CallDeclined");
        }
    }

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.Identity == null || !Context.User.Identity.IsAuthenticated)
        {
            Console.WriteLine("Пользователь не аутентифицирован");
            throw new HubException("Unauthorized");
        }
        
        string userid = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        string username = Context.User.FindFirst(ClaimTypes.Name)!.Value;
        
        var connectionId = Context.ConnectionId;
        ChatUser chatUser; ChatUser? prevUser = await _connectionManager.GetUserByIdAsync(userid);
        if (prevUser != null)
        {
            // chatUser = prevUser;
            // await chatUser.AddConnectionId(HubType.Chat,connectionId);
            await _connectionManager.AddConnectionAsync(userid, HubType.Voice, connectionId);
        }
        else
        {
            chatUser = new ChatUser()
            {
                Id = Guid.Parse(userid),
                Username = username,
            };
            await chatUser.AddConnectionId(HubType.Voice,connectionId);
            await _connectionManager.AddUserAsync(chatUser);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string userid = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        _connectionManager.RemoveConnectionAsync(userid, HubType.Voice, Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}