using System;
using Telegram.Bot.Exceptions;

namespace Panamavirus.Bot.Console.Telegram
{
    public interface IErrorHandler
    {
        void HandleApiRequestError(ApiRequestException e);
        void HandleGeneralError(Exception e);
    }
}
