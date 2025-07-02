using System.ComponentModel.DataAnnotations;

namespace SignalRChatTest.Models;

public class ChatMessage
{
    [Key]
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsPrivate { get; set; } = false;
    public Guid? Recipient { get; set; }
}