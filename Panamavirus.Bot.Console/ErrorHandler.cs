using Serilog;
using Panamavirus.Bot.Console.Telegram;
using System;
using Telegram.Bot.Exceptions;

namespace Panamavirus.Bot.Console
{
    public class ErrorHandler : IErrorHandler
    {
        private readonly ILogger logger;

        public ErrorHandler(ILogger logger)
        {
            this.logger = logger;
        }

        public void HandleApiRequestError(ApiRequestException e) => LogException(e);
        public void HandleGeneralError(Exception e) => LogException(e);
        private void LogException(Exception e) => logger.Error(e, "Error during updates request");

    }
}