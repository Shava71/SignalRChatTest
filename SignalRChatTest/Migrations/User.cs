using System.ComponentModel.DataAnnotations;

namespace SignalRChatTest.Migrations;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}