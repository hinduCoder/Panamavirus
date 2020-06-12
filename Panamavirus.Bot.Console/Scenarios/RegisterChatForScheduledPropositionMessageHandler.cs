using Panamavirus.Bot.Console.Entities;
using Panamavirus.Bot.Console.Telegram;
using System.Linq;
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

        public bool HandlesThis(Message message) => message.Text?.StartsWith("/start") == true || message.Text?.StartsWith("/stop") == true;

        public async Task HandleMessage(Message message)
        {
            var chatId = message.Chat.Id;
            if (message.Text.StartsWith("/start"))
            {
                if (await AddSubscriber(message.Chat))
                    await _telegram.SendTextMessageAsync(chatId, "Теперь ты будешь получать уведомления с предложением сыграть");
                else
                    await _telegram.SendTextMessageAsync(chatId, "Хватит это писать!");
            }

            if (message.Text.StartsWith("/stop"))
            {
                if (await RemoveSubscriber(chatId))
                    await _telegram.SendTextMessageAsync(chatId, "Всё, я заткнулся");
                else
                    await _telegram.SendTextMessageAsync(chatId, "Хватит это писать!");
            }
        }

        private async Task<bool> AddSubscriber(ChatId chatId)
        {
            var existingSubscribedChat = _botContext.SubscribedChat.FirstOrDefault(s => s.ChatId == chatId.Identifier);
            if (existingSubscribedChat != null)
                return false;
            _botContext.Add(new SubscribedChat { ChatId = chatId.Identifier });
            await _botContext.SaveChangesAsync();
            return true;
        }

        private async Task<bool> RemoveSubscriber(ChatId chatId)
        {
            var subscribedChats = _botContext.SubscribedChat.Where(s => s.ChatId == chatId.Identifier).ToList();
            if (subscribedChats.Count == 0)
                return false;
            _botContext.RemoveRange(subscribedChats);
            await _botContext.SaveChangesAsync();
            return true;
        }
    }
}
