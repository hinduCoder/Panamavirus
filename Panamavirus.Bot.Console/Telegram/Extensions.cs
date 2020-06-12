using Telegram.Bot.Exceptions;

namespace Panamavirus.Bot.Console.Telegram
{
    public static class Extensions
    {
        public static string ToCustomString(this ApiRequestException e)
        {
            return $@"ErrorCode: {e.ErrorCode}
MigrateToChatId: {e.Parameters?.MigrateToChatId}
RetryAfter: {e.Parameters?.RetryAfter} 
Exception: {e}";
        }
    }
}
