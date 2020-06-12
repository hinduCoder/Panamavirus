using Panamavirus.Bot.Console.Telegram;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Panamavirus.Bot.Console.Scenarios
{
    public class GamePropositionTask : IScheduledTask
    {
        private readonly BotContext _botContext;
        private readonly ITelegramBotClient _telegram;

        public string Cron => Hangfire.Cron.Daily(10);

        public string Code => "game-proposition";

        public GamePropositionTask(BotContext botContext, ITelegramBotClient telegram)
        {
            _botContext = botContext;
            _telegram = telegram;
        }
        public async Task Execute()
        {
            var chats = _botContext.SubscribedChat.Select(c => c.ChatId).Distinct().ToList();
            foreach (var chat in chats)
            {
                await _telegram.SendTextMessageAsync(chat, "Хочешь сегодня сыграть?",
                    replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "Я в деле", CallbackData = CallbackButtonDataContants.OptIn }));
            }
        }
    }
}
