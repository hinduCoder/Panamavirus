using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panamavirus.Bot.Console;

namespace Panamavirus.Bot.Tests
{
    internal class FakeDbContext : BotContext
    {
        public FakeDbContext(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("db");
        }
    }
    
}
