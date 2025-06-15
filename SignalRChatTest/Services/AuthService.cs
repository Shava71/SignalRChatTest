using SignalRChatTest.Contracts;
using SignalRChatTest.Migrations;
using SignalRChatTest.Repositories;

namespace SignalRChatTest.Service;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserRepository userRepository, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> RegisterAsync(RegisterRequest registerRequest)
    {
        var userExists = await _userRepository.CheckUserIsExist(registerRequest.Username ,registerRequest.Email);

        if (userExists is not null)
        {
            return false;
        }

        User user = new User()
        {
            Id = Guid.NewGuid(),
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            Password = registerRequest.Password,
        };
        
        await _userRepository.AddUserAsync(user);
        return true;
    }

    public async Task<User?> LoginAsync(LoginRequest loginRequest)
    {
        var userExist = await _userRepository.CheckUserIsRegistered(loginRequest.Email, loginRequest.Password);
        return userExist;
    }
}