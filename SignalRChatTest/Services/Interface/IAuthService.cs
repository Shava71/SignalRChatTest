using SignalRChatTest.Contracts;
using SignalRChatTest.Migrations;

namespace SignalRChatTest.Service;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest registerRequest);
    Task<User?> LoginAsync(LoginRequest loginRequest);

}