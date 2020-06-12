using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Panamavirus.Bot.Console.Telegram
{
    public interface IMessageHandler
    {
        bool HandlesThis(Message message);
        Task HandleMessage(Message message);
    }
}
