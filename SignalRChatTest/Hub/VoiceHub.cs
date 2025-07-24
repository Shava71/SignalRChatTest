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
    private IRedisService _redis;

    
    public VoiceHub(IRedisService redis)
    {
        _redis = redis;
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

        ChatUser toUser = await _redis.GetUserByIdAsync(toUserId);
        
        await Clients.User(toUser.ConnectionId).SendAsync("IncomingCall", fromUserName, toUser);
    }

    public async Task AcceptCall(string callerId)
    {
        string fromUserName = Context.User.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
        
        ChatUser toUser = await _redis.GetUserByIdAsync(callerId);
        await Clients.User(toUser.ConnectionId).SendAsync("CallAccepted", fromUserName);
    }

    public async Task DeclineCall(string callerId)
    {
        ChatUser toUser = await _redis.GetUserByIdAsync(callerId);
        await Clients.User(toUser.ConnectionId).SendAsync("CallDeclined");
    }
}