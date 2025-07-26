using Moq;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Tests;

public class connectionManagerTests
{
    
    private const string Prefix = "user:";
    private const string Postfix = ":connection";
    private string GenerateRedisKey(string key) => $"{Prefix}{key}{Postfix}";

    [Fact]
    public async Task GetUserByIdAsync_WithExistrignUser_ShouldReturnUser()
    {
        Mock<IRedisService> mock = new Mock<IRedisService>();
        Guid userId = Guid.NewGuid();
        string key = GenerateRedisKey(userId.ToString());
        ChatUser expectedUser = new ChatUser()
        {
            Id = userId,
            Username = "user"
        }
        
        ConnectionManager manager = new ConnectionManager(mock.Object);
    }
}