using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace Panamavirus.Bot.Console.Telegram
{
	public interface ICallbackQueryHandler
	{
		bool HandlesThis(CallbackQuery callbackQuery);
		Task HandleCallbackQuery(CallbackQuery callbackQuery);
	}
}