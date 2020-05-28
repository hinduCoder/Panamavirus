using Microsoft.EntityFrameworkCore.Query;
using Panamavirus.Bot.Console.Telegram;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Panamavirus.Bot.Console
{
    public class AttendGameScenario : IMessageHandler, ICallbackQueryHandler
    {
        public bool CanHandleMessage(Message message)
        {
            throw new NotImplementedException();
        }

        public bool CanHandleCallbackQuery(ICallbackQueryHandler callbackQuery)
        {
            throw new NotImplementedException();
        }

        public Task HandleCallbackQuery(CallbackQuery callbackQuery)
        {
            throw new NotImplementedException();
        }

        public Task HandleMessage(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
