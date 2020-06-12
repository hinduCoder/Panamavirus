using System.Threading.Tasks;

namespace Panamavirus.Bot.Console.Telegram
{
    public interface IScheduledTask
    {
        string Cron { get; }
        string Code { get; }
        Task Execute();
    }
}
