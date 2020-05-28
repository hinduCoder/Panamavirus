using Microsoft.EntityFrameworkCore;
using Panamavirus.Bot.Console.Entities;

namespace Panamavirus.Bot.Console
{
    public class Context : DbContext
    {
        public DbSet<SubscribedChat> SubscribedChat { get; set; }
        public DbSet<Participance> Participance { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseSqlServer(Program.DbConnectionString);
            optionsBuilder.UseNpgsql(Program.PostgresConnectionString);
        }
    }
}
