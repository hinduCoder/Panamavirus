using Autofac;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Panamavirus.Bot.Console.Entities;
using Panamavirus.Bot.Console.Telegram;
using Panamavirus.Bot.Console.WhatTheFuck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Console;

namespace Panamavirus.Bot.Console
{
    class Program
    {
        private static readonly string Token = "662430664:AAGxK8FB8DCxYg1Eo-xDijhYJyqyTdIQZIA";
        public static readonly string PostgresConnectionString = $"User ID=postgres;Password=npgsqlpasswd;Host={"10.0.0.4" ?? "localhost"};Port=5432;Database=postgres;";
        private static readonly long DebugChannelId = -1001365087009;

        static void Main()
        {
            var containerBuilder = new AutofacContainerBuilder();
            var container = containerBuilder.Build();
            var x = container.Resolve<IEnumerable<ICallbackQueryHandler>>();
            var y = container.Resolve<IEnumerable<IMessageHandler>>();

            WriteLine("Started");
            using var dbContext = new Context();
            dbContext.Database.Migrate();

            WriteLine("Migrations completed");

            GlobalConfiguration.Configuration.UsePostgreSqlStorage(PostgresConnectionString);

            var telegram = new TelegramBotClient(Token);

            telegram.SendTextMessageAsync(DebugChannelId, "Поехали").Wait();

            var program = new Program();
            telegram.OnMessage += program.Telegram_OnMessage;
            telegram.OnCallbackQuery += Telegram_OnCallbackQuery;
            telegram.OnReceiveError += (sender, e) => LogError(e.ApiRequestException);
            telegram.OnReceiveGeneralError += (sender, e) => LogError(e.Exception);
            telegram.StartReceiving();

            using var server = new BackgroundJobServer();
            RecurringJob.AddOrUpdate(() => program.SendProposition(), Cron.Daily(7));
            RecurringJob.RemoveIfExists("Text");
            Thread.Sleep(Timeout.Infinite);
        }

        private static void LogError(Exception e) => WriteLine("{0} | {1}", DateTime.Now, e);

        public static void CallTelegram(Action<TelegramBotClient> action)
        {
            var telegram = new TelegramBotClient(Token);
            try
            {
                action(telegram);
            } catch (ApiRequestException e)
            {
                WriteLine(e);
                telegram.SendTextMessageAsync(DebugChannelId, 
$@"ErrorCode: {e.ErrorCode}
MigrateToChatId: {e.Parameters.MigrateToChatId}
RetryAfter: {e.Parameters.RetryAfter} 
Exception: {e}").Wait();
            } catch (Exception e)
            {
                WriteLine(e);
                telegram.SendTextMessageAsync(DebugChannelId, e.ToString()).Wait();
            }
        }

        public void SendProposition()
        {
            using var dbContext = new Context();
            var chats = dbContext.SubscribedChat.Select(c => c.ChatId).Distinct().ToList();
            chats.ForEach(chat => CallTelegram(tg => tg.SendTextMessageAsync(chat, "Хочешь сегодня сыграть?", 
                replyMarkup: new InlineKeyboardMarkup(new InlineKeyboardButton { Text = "Я в деле", CallbackData = "opt-in" })).Wait()));
        }

        private void Telegram_OnMessage(object sender, MessageEventArgs e)
        {
            WriteLine("Получено сообщение: {0}", e.Message.Text);
            if (e.Message.Text == "/start@panamavirus_squiz_bot")
            {
                Message message=null;
                CallTelegram(tg => message = tg.SendTextMessageAsync(e.Message.Chat.Id, "Хватит это писать!").Result);
                CallTelegram(tg => tg.PinChatMessageAsync(message.Chat.Id, message.MessageId, disableNotification: true).Wait());
                using var dbContext = new Context();
                dbContext.Add(new SubscribedChat { ChatId = e.Message.Chat.Id });
                dbContext.SaveChanges();
            }
            if (e.Message.Text == "Go")
            {
                RecurringJob.Trigger("Program.SendProposition");
            }
        }
        private static void Telegram_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            using var dbContext = new Context();
            if (e.CallbackQuery.Data == "opt-in")
            {
                var p = dbContext.Participance.SingleOrDefault(p => p.UserId == e.CallbackQuery.From.Id && p.MessageId == e.CallbackQuery.Message.MessageId);
                if (p is null)
                {
                    dbContext.Participance.Add(new Participance
                    {
                        UserId = e.CallbackQuery.From.Id,
                        MessageId = e.CallbackQuery.Message.MessageId,
                        UserName = $"{e.CallbackQuery.From.FirstName} {e.CallbackQuery.From.LastName}"
                    });
                    dbContext.SaveChanges();
                }
            }
            else
            {
                var p = dbContext.Participance.SingleOrDefault(p => p.UserId == e.CallbackQuery.From.Id && p.MessageId == e.CallbackQuery.Message.MessageId);
                if (p != null)
                {
                    dbContext.Participance.Remove(p);
                    dbContext.SaveChanges();
                }

            }

            var users = dbContext.Participance.Where(p => p.MessageId == e.CallbackQuery.Message.MessageId).Select(p => p.UserName).ToList();

            var messageText = users.Count > 0 ? $"Сегодня играют: {String.Join(", ", users)}" : "Кто был, уже все успели отказаться";
            if (e.CallbackQuery.Message.Text == messageText)
                return;
            CallTelegram(telegram => 
                telegram.EditMessageTextAsync(
                    e.CallbackQuery.Message.Chat.Id, 
                    e.CallbackQuery.Message.MessageId,
                    messageText,
                    replyMarkup: new InlineKeyboardButton[] {
                        InlineKeyboardButton.WithCallbackData("Я в деле", "opt-in"),
                        InlineKeyboardButton.WithCallbackData("Я передумал", "opt-out")
                    }).Wait());
        }
    }
}
