namespace SignalRChatTest.DTO;

public class MessageHistoryDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsPrivate { get; set; } = false;
    public string Recipient { get; set; }
}