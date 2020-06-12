using Hangfire;
using Panamavirus.Bot.Console.Telegram;
using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Panamavirus.Bot.Console.Scenarios.Administration
{
    public class TriggerJobCommand : IMessageHandler
    {
        public Task HandleMessage(Message message)
        {
            var jobName = message.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).ElementAtOrDefault(1);
            RecurringJob.Trigger(jobName);
            return Task.CompletedTask;
        }

        public bool HandlesThis(Message message) => message.Text?.StartsWith("/trigger") == true;
    }
}
