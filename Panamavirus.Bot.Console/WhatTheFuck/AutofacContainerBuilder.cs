using Autofac;
using Autofac.Features.Decorators;
using Microsoft.Extensions.Configuration;
using Panamavirus.Bot.Console.Telegram;
using Serilog;
using System;
using System.Reflection;
using Telegram.Bot;

namespace Panamavirus.Bot.Console.WhatTheFuck
{
    internal class AutofacContainerBuilder
    {
        public IContainer Build()
        {
            var builder = new ContainerBuilder();

            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                    .AddEnvironmentVariables("CONFIG_")
                    .Build();
            builder.RegisterInstance(configuration).As<IConfiguration>().SingleInstance();

            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            builder.RegisterInstance(logger).As<ILogger>().SingleInstance();

            builder.Register(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                return new TelegramBotClient(configuration.GetValue<string>("telegram:token"));
            }).As<ITelegramBotClient>().SingleInstance();

            builder.RegisterType<BotContext>().AsSelf().InstancePerDependency();

            var thisAssembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<IMessageHandler>().As<IMessageHandler>();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<ICallbackQueryHandler>().As<ICallbackQueryHandler>();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<IErrorHandler>().As<IErrorHandler>().SingleInstance();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<IScheduledTask>().As<IScheduledTask>().AsSelf();

            return builder.Build();
        }
    }
}
