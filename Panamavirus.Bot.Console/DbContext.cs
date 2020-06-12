using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panamavirus.Bot.Console.Entities;

namespace Panamavirus.Bot.Console
{
    public class BotContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<SubscribedChat> SubscribedChat { get; set; } = null!;
        public DbSet<Participance> Participance { get; set; } = null!;

        public BotContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetValue<string>("db:connectionString"));
        }
    }
}
