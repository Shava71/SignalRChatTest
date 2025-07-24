using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
}