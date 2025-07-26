namespace SignalRChatTest.Models;

public class ChatUser
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public Dictionary<HubType, List<string>> ConnectionId { get; set; } = new();

    public async Task AddConnectionId(HubType hubType, string newConnectionId)
    {
        if (!ConnectionId.ContainsKey(hubType))
        {
            ConnectionId[hubType] = new List<string>();
        }

        if (!ConnectionId[hubType].Contains(newConnectionId))
        {
            ConnectionId[hubType].Add(newConnectionId);
        }
    }

    public async Task RemoveConnectionId(HubType hubType, string oldConnectionId)
    {
        if (ConnectionId.TryGetValue(hubType, out List<string> connections))
        {
            connections.Remove(oldConnectionId);
            if (connections.Count == 0)
            {
                ConnectionId.Remove(hubType);
            }
        }
    }

    public async Task<List<string>> GetConnectionIds(HubType hubType)
    {
        return ConnectionId.TryGetValue(hubType, out var connectionId) ? connectionId : new List<string>();
    }
}

public enum HubType
{
    Chat,
    Voice
}