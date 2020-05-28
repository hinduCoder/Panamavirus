using Telegram.Bot.Types;
using System.Threading.Tasks;

namespace Panamavirus.Bot.Console.Telegram
{
	public interface ICallbackQueryHandler
	{
		bool CanHandleCallbackQuery(ICallbackQueryHandler callbackQuery);
		Task HandleCallbackQuery(CallbackQuery callbackQuery);
	}
}