using Microsoft.EntityFrameworkCore;
using SignalRChatTest.Context;
using SignalRChatTest.Migrations;

namespace SignalRChatTest.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SignalRDbContext _dbContext;

    public UserRepository(SignalRDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<User?> CheckUserIsExist(string userName, string email)
    {
        return await _dbContext.Users.Where(u => u.Username == userName || u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> CheckUserIsRegistered(string email, string password)
    {
        return await _dbContext.Users.Where(u => u.Email == email && u.Password == password).FirstOrDefaultAsync();
    }

    public async Task AddUserAsync(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }
}