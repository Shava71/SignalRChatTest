using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SignalRChatTest.Context;
using SignalRChatTest.DTO;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;

namespace SignalRChatTest.Repositories;

public class MessageReposiory : IMessageRepository
{
    private readonly SignalRDbContext dbContext;
    private readonly IConfiguration _configuration;

    public MessageReposiory(SignalRDbContext context, IConfiguration configuration)
    {
        this.dbContext = context;
        this._configuration = configuration;
    }
    
    public async Task AddChatMessage(ChatMessage message)
    {
        await dbContext.ChatMessages.AddAsync(message);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<ChatMessage>> GetAllMessages()
    {
        return await dbContext.ChatMessages.AsNoTracking().ToListAsync();
    }

    public async Task<List<User>> GettAllUsers()
    {
        return await dbContext.Users.AsNoTracking().ToListAsync();
    }

    public async Task<List<MessageHistoryDto>> GetChatHistory()
    {
        // var result = dbContext.ChatMessages
        //     .Join(dbContext.Users, 
        //         message => message.UserId, 
        //         user => user.Id, 
        //         (chatMessage, userFrom) => new {chatMessage, userFrom})
        //     .Join(dbContext.Users, 
        //         message => message.chatMessage.Recipient, 
        //         user => user.Id, 
        //         (a, userTo) => new {a.chatMessage, a.userFrom, userTo})
        //     .Select(mes => new MessageHistoryDto
        //     {
        //         Id = mes.chatMessage.Id,
        //         UserName = mes.userFrom.Username,
        //         Message = mes.chatMessage.Message,
        //         Timestamp = mes.chatMessage.Timestamp,
        //         IsPrivate = mes.chatMessage.IsPrivate,
        //         Recipient = mes.userTo.Username,
        //     })
        //     .AsNoTracking()
        //     .ToList();
        
        List<MessageHistoryDto>? result = new List<MessageHistoryDto>();
        
        string connString = this._configuration.GetConnectionString("DefaultConnection")!;
        using (IDbConnection db = new NpgsqlConnection(connString))
        {
            string sql =
                @"SELECT c.""Id"", u.""Username"" AS ""UserName"",c.""UserId"" , c.""Message"", 
                  c.""Timestamp"", c.""IsPrivate"", u0.""Username"" AS ""Recipient"", c.""Recipient"" AS ""RecipientID""
                FROM ""ChatMessages"" AS c
                         Left JOIN ""Users"" AS u ON c.""UserId"" = u.""Id""
                         Left JOIN ""Users"" AS u0 ON c.""Recipient"" = u0.""Id"";";

            result = await db.QueryAsync<MessageHistoryDto>(sql) as List<MessageHistoryDto>;
        }
        
        return result;
    }
}