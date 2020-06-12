using Panamavirus.Bot.Console.Entities;
using Panamavirus.Bot.Console.Telegram;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

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
            var userFullName = $"{userFrom.FirstName} {userFrom.LastName}";

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
                        UserName = userFullName
                    });
                    _botContext.SaveChanges();

                    await SendRegisteredNotification(callbackQuery.Message, userFullName);
                }
            }
            if (callbackQuery.Data == CallbackButtonDataContants.OptOut)
            {
                var p = _botContext.Participance.SingleOrDefault(p => p.UserId == userFrom.Id && p.MessageId == originalMessageId);
                if (p != null)
                {
                    _botContext.Participance.Remove(p);
                    _botContext.SaveChanges();

                    await SendChangedMindNotification(callbackQuery.Message, userFullName);
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

        private async Task SendRegisteredNotification(Message callMessage, string name) => await Reply(callMessage, $"*{name}* записался на игру");

        private async Task SendChangedMindNotification(Message callMessage, string name) => await Reply(callMessage, $"*{name}* передумал играть");

        private async Task Reply(Message callMessage, string message)
        {
            await _telegram.SendTextMessageAsync(callMessage.Chat, message, ParseMode.MarkdownV2, replyToMessageId: callMessage.MessageId);
        }
    }
}
