using System.Text.Json;
using Moq;
using SignalRChatTest.Hub;
using SignalRChatTest.Models;
using SignalRChatTest.Service;

namespace SignalRChatTest.Tests;

public class connectionManagerTests
{
    
    private const string Prefix = "user:";
    private const string Postfix = ":connection";
    private string GenerateRedisKey(string key) => $"{Prefix}{key}{Postfix}";

    [Fact]
    public async Task AddUserAsync_Test()
    {
        // Arrange
        Mock<IRedisService> RedisMock = new Mock<IRedisService>();
        Guid userId = Guid.NewGuid();
        string key = GenerateRedisKey(userId.ToString());
        ChatUser userToAdd = new ChatUser()
        {
            Id = userId,
            Username = "user",
            ConnectionId = new Dictionary<HubType, List<string>>()
            {
                { HubType.Chat, new List<string>(){"12345"}}
            },
        };

        string callbackKey = null;
        string callbackSerializedValue = null;
        RedisMock.Setup(r => r.SetValueAsync(It.Is<string>(i => i.StartsWith("user:") && i.EndsWith(":connection")), It.IsAny<object>() )).Callback<string, object>(
            (key, value) =>
            {
                callbackKey = key;
                callbackSerializedValue = value.ToString()!;
            })
            .Returns(Task.CompletedTask);
        
        ConnectionManager connectionManager = new ConnectionManager(RedisMock.Object);
        // Act
        await connectionManager.AddUserAsync(userToAdd);
        RedisMock.Verify(r => r.SetValueAsync(It.Is<string>(i => i.StartsWith("user:") && i.EndsWith(":connection")), It.IsAny<object>()), Times.Once());
        Assert.Equal(key, callbackKey);
        Assert.Equal(callbackSerializedValue, JsonSerializer.Serialize(userToAdd));
    }
    
    //Test for return valid value
    [Fact]
    public async Task GetUserByIdAsync_WithExistrignUser_ShouldReturnUser()
    {
        // Arrange 
        Mock<IRedisService> RedisMock = new Mock<IRedisService>();
        Guid userId = Guid.NewGuid();
        string key = GenerateRedisKey(userId.ToString());
        ChatUser expectedUser = new ChatUser()
        {
            Id = userId,
            Username = "user",
            ConnectionId = new Dictionary<HubType, List<string>>()
            {
                { HubType.Chat, new List<string>(){"12345"}}
            },
        };
        RedisMock.Setup(m => m.GetValueAsync<ChatUser>(key)).ReturnsAsync(expectedUser);
        
        ConnectionManager manager = new ConnectionManager(RedisMock.Object);

        // Act
        ChatUser? result = await manager.GetUserByIdAsync(userId.ToString());
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedUser, result, Comparer.Get<ChatUser>((u1, u2) => u1.Id == u2.Id && u1.Username == u2.Username)!);

        foreach (var dict in expectedUser.ConnectionId)
        {
            Assert.True(result.ConnectionId.ContainsKey(dict.Key));
            Assert.Equal(dict.Value, result.ConnectionId[dict.Key]);
        }
        RedisMock.Verify(m => m.GetValueAsync<ChatUser>(key), Times.Once);
    }

    [Fact]
    public async Task AddConnectionAsync_WithValidConnectnion_Test()
    {
        // Arrange
        Mock<IRedisService> RedisMock = new Mock<IRedisService>();
        Guid userId = Guid.NewGuid();
        string key = GenerateRedisKey(userId.ToString());
        ChatUser expectedUser = new ChatUser()
        {
            Id = userId,
            Username = "user",
            ConnectionId = new Dictionary<HubType, List<string>>()
            {
                { HubType.Chat, new List<string>(){"12345"}}
            },
        };
        
        string newConnectionId = "1234";
        HubType newChatHub = HubType.Voice;
        
        RedisMock.Setup(r => r.GetValueAsync<ChatUser>(key)).ReturnsAsync(expectedUser);
        // expectedUser.AddConnectionId(newChatHub, newConnectionId).Wait();
        
        string capturedKey = null;
        ChatUser capturedUser = null;
        RedisMock.Setup(r => r.SetValueAsync(It.Is<string>(i => i.StartsWith("user:") && i.EndsWith(":connection") && i == key),
            It.IsAny<object>()))
            .Callback<string,object>((key, value) =>
            {
                capturedKey = key;
                capturedUser = JsonSerializer.Deserialize<ChatUser>(value.ToString())!;
            })
            .Returns(Task.CompletedTask);
        
        ConnectionManager manager = new ConnectionManager(RedisMock.Object);
        
        // Act
        await manager.AddConnectionAsync(userId.ToString(), newChatHub, newConnectionId);
        
        // Assert
        RedisMock.Verify(r => r.GetValueAsync<ChatUser>(key), Times.Once);
        RedisMock.Verify(r => r.SetValueAsync(It.Is<string>(i => i.StartsWith("user:") && i.EndsWith(":connection") && i == key), It.IsAny<object>()), Times.Once);
        
        Assert.Equal(capturedKey, key);
        await EqualsMethods.EqualsMethods.CheckTwoUserEquals(expectedUser, capturedUser);
    }
}