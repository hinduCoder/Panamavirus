using Autofac;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Panamavirus.Bot.Console.Telegram;
using Panamavirus.Bot.Console.WhatTheFuck;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Panamavirus.Bot.Console
{
    class Program
    {
        private static ILogger logger = null!;
        static void Main()
        {
            var containerBuilder = new AutofacContainerBuilder();
            var container = containerBuilder.Build();

            logger = container.Resolve<ILogger>();

            logger.Information("Dependencies built");

            MigrateDatabase(container.Resolve<BotContext>());

            SetupHangfire(container);
            SetupTelegram(container);

            logger.Warning("Service started and ready");

            Thread.Sleep(Timeout.Infinite);
        }

        private static void SetupTelegram(IContainer container)
        {
            var telegram = container.Resolve<ITelegramBotClient>();

            var messageHandlers = container.Resolve<IEnumerable<IMessageHandler>>();
            telegram.OnMessage += async (sender, eventArgs) =>
            {
                var message = eventArgs.Message;
                logger.Information("Message recieved from {From} ({Type}): {@Text}", message.From.Id, message.Type, message.Type switch
                {
                    MessageType.Text => message.Text,
                    MessageType.Dice => message.Dice.Value,
                    _ => message
                });

                var candidateHandlers = messageHandlers.Where(h => h.HandlesThis(eventArgs.Message)).ToList();
                if (candidateHandlers.Count == 0)
                    return;
                
                if (candidateHandlers.Count > 1)
                {
                    logger.Error("Conflict between multiple message handlers {Handlers}. Message {@Message}", candidateHandlers.Select(h => h.GetType().FullName), eventArgs.Message);
                    return;
                }

                var foundHander = candidateHandlers[0];
                try
                {
                    await foundHander.HandleMessage(message);

                } catch (Exception e)
                {
                    logger.ForContext(foundHander.GetType()).Error(e, "Error during message handling");
                }
            };

            var callbackQueryHandlers = container.Resolve<IEnumerable<ICallbackQueryHandler>>();
            telegram.OnCallbackQuery += async (sender, eventArgs) =>
            {
                var callbackQuery = eventArgs.CallbackQuery;

                logger.Information("Callback query from {@From} ({@Data}) for message {@MessageId}", callbackQuery.From.Id, callbackQuery.Data, callbackQuery.Message.MessageId);

                var candidateHandlers = callbackQueryHandlers.Where(h => h.HandlesThis(eventArgs.CallbackQuery)).ToList();
                if (candidateHandlers.Count == 0)
                    return;

                if (candidateHandlers.Count > 1)
                {
                    logger.Error("Conflict between multiple callback query handlers {Handlers}. Callback {@Callback}", candidateHandlers.Select(h => h.GetType().FullName), callbackQuery);
                    return;
                }

                var foundHander = candidateHandlers[0];
                try
                {
                    await foundHander.HandleCallbackQuery(callbackQuery);

                }
                catch (Exception e)
                {
                    logger.ForContext(foundHander.GetType()).Error(e, "Error during CallbackQuery handling");
                }
            };

            var errorHandlers = container.Resolve<IEnumerable<IErrorHandler>>();
            telegram.OnReceiveError += (sender, e) =>
            {
                foreach (var errorHandler in errorHandlers)
                {
                    errorHandler.HandleApiRequestError(e.ApiRequestException);
                }
            };
            telegram.OnReceiveGeneralError += (sender, e) =>
            {
                foreach (var errorHandler in errorHandlers)
                {
                    errorHandler.HandleGeneralError(e.Exception);
                }
            };

            telegram.StartReceiving();
            logger.Information("Telegram listening started");
        }

        private static void MigrateDatabase(BotContext botContext)
        {
            botContext.Database.Migrate();
            logger.Information("Migration completed");
        }

        private static void SetupHangfire(IContainer container)
        {
            GlobalConfiguration.Configuration.UseAutofacActivator(container);

            GlobalConfiguration.Configuration.UsePostgreSqlStorage(container.Resolve<IConfiguration>().GetValue<string>("db:connectionString"));

            RecurringJob.RemoveIfExists("Program.SendProposition"); //TODO remove
            var server = new BackgroundJobServer();
            logger.Information("Hangfire server started");
            var tasks = container.Resolve<IEnumerable<IScheduledTask>>();
            foreach (var task in tasks)
            {
                RecurringJob.AddOrUpdate(task.Code, () => task.Execute(), task.Cron);
            }
        }
    }
}
