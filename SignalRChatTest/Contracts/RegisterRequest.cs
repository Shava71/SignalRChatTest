using System.ComponentModel.DataAnnotations;

namespace SignalRChatTest.Contracts;

public class RegisterRequest
{
    [Display(Name = "Имя пользователя")] 
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email обязателен")]               
    [EmailAddress(ErrorMessage = "Неверный формат email")]      
    [Display(Name = "Почта")]
    public string Email { get; set; }
    
    [Display(Name = "Пароль")]
    [Required(ErrorMessage = "Пароль обязателен")]       
    [MinLength(1, ErrorMessage = "Минимум 1 символ")]    
    public string Password { get; set; }
}
    