using SignalRChatTest.Models;
namespace SignalRChatTest.Tests;

public class ChatUserTests
{
    private Task CheckTwoUserEquals(ChatUser wantedUser, ChatUser actual)
    {
        // Assert.Equal(wantedUser.Id, actual.Id);
        // Assert.Equal(wantedUser.Username, actual.Username);
        Assert.Equal(wantedUser, actual, Comparer.Get<ChatUser>((u1, u2) => u1.Id == u2.Id && u1.Username == u2.Username));
        Assert.NotNull(actual.ConnectionId);

        foreach (var ids in wantedUser.ConnectionId)
        {
            Assert.True(actual.ConnectionId.ContainsKey(ids.Key));
            Assert.Equal(ids.Value, actual.ConnectionId[ids.Key]);
        }

        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task AddConnectionId_Correctly()
    {
        Guid idUser = Guid.NewGuid();
        string username = "user";

        ChatUser user = new ChatUser()
        {
            Id = idUser,
            Username = username
        };
        await user.AddConnectionId(HubType.Chat, "12345");
        await user.AddConnectionId(HubType.Chat, "1234");

        
        Dictionary<HubType, List<string>> wantedConnectionIds = new Dictionary<HubType, List<string>>()
        {
            { HubType.Chat, new List<string>(){"12345", "1234"} },
        };

        ChatUser wantedUser = new ChatUser()
        {
            Id = idUser,
            Username = username,
            ConnectionId = wantedConnectionIds
        };
        
        await CheckTwoUserEquals(wantedUser, user);
    }
    
    [Fact]
    public async Task RemoveConnectionId_Correctly()
    {
        Guid idUser = Guid.NewGuid();
        string username = "user";

        ChatUser user = new ChatUser()
        {
            Id = idUser,
            Username = username,
            ConnectionId = new Dictionary<HubType, List<string>>()
            {
                { HubType.Chat, new List<string>(){"12345"} },
                { HubType.Voice, new List<string>(){"12345"}}
            }
        };
        await user.RemoveConnectionId(HubType.Chat, "12345");
        
        ChatUser wantedUser = new ChatUser()
        {
            Id = idUser,
            Username = username,
            ConnectionId = new Dictionary<HubType, List<string>>()
            {
                { HubType.Voice, new List<string>(){"12345"}}
            }
        };
        
        await CheckTwoUserEquals(wantedUser, user);
    }
   
}