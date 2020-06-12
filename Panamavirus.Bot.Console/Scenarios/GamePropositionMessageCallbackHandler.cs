using Panamavirus.Bot.Console.Entities;
using Panamavirus.Bot.Console.Telegram;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace Panamavirus.Bot.Console.Scenarios
{
    public class GamePropositionMessageCallbackHandler : ICallbackQueryHandler
    {
        private readonly BotContext _botContext;
        private readonly ITelegramBotClient _telegram;

        public GamePropositionMessageCallbackHandler(BotContext botContext, ITelegramBotClient telegram)
        {
            _botContext = botContext;
            _telegram = telegram;
        }

        public bool HandlesThis(CallbackQuery callbackQuery) => callbackQuery.Data?.StartsWith("opt") == true;

        public async Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            var userFrom = callbackQuery.From;
            var originalMessageId = callbackQuery.Message.MessageId;
            if (callbackQuery.Data == CallbackButtonDataContants.OptIn)
            {
                var userParticipance = _botContext.Participance
                    .SingleOrDefault(p => p.UserId == userFrom.Id && p.MessageId == originalMessageId);
                if (userParticipance is null)
                {
                    _botContext.Participance.Add(new Participance
                    {
                        UserId = userFrom.Id,
                        MessageId = originalMessageId,
                        UserName = $"{userFrom.FirstName} {userFrom.LastName}"
                    });
                    _botContext.SaveChanges();
                }
            }
            if (callbackQuery.Data == CallbackButtonDataContants.OptOut)
            {
                var p = _botContext.Participance.SingleOrDefault(p => p.UserId == userFrom.Id && p.MessageId == originalMessageId);
                if (p != null)
                {
                    _botContext.Participance.Remove(p);
                    _botContext.SaveChanges();
                }

            }

            var users = _botContext.Participance.Where(p => p.MessageId == originalMessageId).Select(p => p.UserName).ToList();

            var messageText = users.Count > 0 
                ? $"Сегодня играют: {string.Join(", ", users)}" 
                : "Кто был, уже все успели отказаться";
            if (callbackQuery.Message.Text == messageText)
                return;
            await _telegram.EditMessageTextAsync(
                    callbackQuery.Message.Chat.Id,
                    originalMessageId,
                    messageText,
                    replyMarkup: new InlineKeyboardButton[] {
                        InlineKeyboardButton.WithCallbackData("Я в деле", CallbackButtonDataContants.OptIn),
                        InlineKeyboardButton.WithCallbackData("Я передумал", CallbackButtonDataContants.OptOut)
                    });
        }
    }
}
