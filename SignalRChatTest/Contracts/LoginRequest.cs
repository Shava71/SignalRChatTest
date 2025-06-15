using System.ComponentModel.DataAnnotations;

namespace SignalRChatTest.Contracts;

public class LoginRequest
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; }
 
    [Required(ErrorMessage = "Пароль обязателен")]
    [MinLength(1, ErrorMessage = "Минимум 1 символ")]

    public string Password { get; set; }
    
    public List<string> Roles { get; set; }
}