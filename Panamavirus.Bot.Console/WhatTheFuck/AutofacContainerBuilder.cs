using Autofac;
using Microsoft.Extensions.Configuration;
using Panamavirus.Bot.Console.Telegram;
using System.Reflection;
using Telegram.Bot;

namespace Panamavirus.Bot.Console.WhatTheFuck
{
    internal class AutofacContainerBuilder
    {
        public IComponentContext Build()
        {
            var builder = new ContainerBuilder();
            builder.Register<IConfiguration>(context =>
            {
                return new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();
            }).SingleInstance();

            builder.Register<TelegramBotClient>(context =>
            {
                var configuration = context.Resolve<IConfiguration>();
                return new TelegramBotClient(configuration.GetValue<string>("telegram:token"));
            }).SingleInstance();

            builder.RegisterType<Context>().AsSelf().InstancePerDependency();

            var thisAssembly = Assembly.GetExecutingAssembly();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<IMessageHandler>().As<IMessageHandler>().SingleInstance();
            builder.RegisterAssemblyTypes(thisAssembly).AssignableTo<ICallbackQueryHandler>().As<ICallbackQueryHandler>().SingleInstance();
            return builder.Build();
        }
    }
}
