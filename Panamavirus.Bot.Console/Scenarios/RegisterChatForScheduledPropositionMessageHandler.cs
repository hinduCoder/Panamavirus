using Panamavirus.Bot.Console.Entities;
using Panamavirus.Bot.Console.Telegram;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Panamavirus.Bot.Console.Scenarios
{
    public class RegisterChatForScheduledPropositionMessageHandler : IMessageHandler
    {
        private readonly BotContext _botContext;
        private readonly ITelegramBotClient _telegram;

        public RegisterChatForScheduledPropositionMessageHandler(BotContext botContext, ITelegramBotClient telegram)
        {
            _botContext = botContext;
            _telegram = telegram;
        }

        public bool HandlesThis(Message message) => message.Text?.StartsWith("/start") == true;

        public async Task HandleMessage(Message message)
        {
            if (!message.Text.StartsWith("/start"))
                return;
            var chatId = message.Chat.Id;
            _ = await _telegram.SendTextMessageAsync(chatId, "Хватит это писать!");

            _botContext.Add(new SubscribedChat { ChatId = message.Chat.Id });
            _botContext.SaveChanges();
        }

    }
}
