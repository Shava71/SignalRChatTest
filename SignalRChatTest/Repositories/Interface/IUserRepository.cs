using SignalRChatTest.Migrations;

namespace SignalRChatTest.Repositories;

public interface IUserRepository
{
    Task<User?> CheckUserIsExist(string userName, string email);
    Task<User?> CheckUserIsRegistered(string email, string password);
    Task AddUserAsync(User user);
    
}