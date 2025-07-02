using Microsoft.EntityFrameworkCore;
using SignalRChatTest.Migrations;
using SignalRChatTest.Models;

namespace SignalRChatTest.Context;

public class SignalRDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    
    // private readonly IConfiguration _configuration;

    public SignalRDbContext(DbContextOptions<SignalRDbContext> options) : base(options)
    { }

    // SignalRDbContext(IConfiguration configuration)
    // {
    //     _configuration = configuration;
    // }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
    // }
}