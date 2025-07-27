using SignalRChatTest.Models;

namespace SignalRChatTest.Tests.EqualsMethods;

public class EqualsMethods
{
    public static Task CheckTwoUserEquals(ChatUser wantedUser, ChatUser actual)
    {
        // Assert.Equal(wantedUser.Id, actual.Id);
        // Assert.Equal(wantedUser.Username, actual.Username);
        // Assert
        Assert.Equal(wantedUser, actual, Comparer.Get<ChatUser>((u1, u2) => u1.Id == u2.Id && u1.Username == u2.Username));
        Assert.NotNull(actual.ConnectionId);

        foreach (var ids in wantedUser.ConnectionId)
        {
            Assert.True(actual.ConnectionId.ContainsKey(ids.Key));
            Assert.Equal(ids.Value, actual.ConnectionId[ids.Key]);
        }

        return Task.CompletedTask;
    }
}