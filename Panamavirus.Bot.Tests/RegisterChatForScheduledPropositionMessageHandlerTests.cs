using Panamavirus.Bot.Console.Scenarios;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Xunit;

namespace Panamavirus.Bot.Tests
{
    public class RegisterChatForScheduledPropositionMessageHandlerTests
    {
        public RegisterChatForScheduledPropositionMessageHandlerTests()
        {

        }

        [Fact]
        public async Task AddedToSubscribedChats()
        {
            var fakeDbContext = new FakeDbContext(null);
            var handeler = new RegisterChatForScheduledPropositionMessageHandler(fakeDbContext, new FakeTelegramBotClient());
            
            await handeler.HandleMessage(new Message { Text = "/start@alksjdf", Chat = new Chat { Id = 1 } });

            var result = fakeDbContext.SubscribedChat.ToList();

            Assert.Collection(result, x => Assert.Equal(1, x.ChatId));
            fakeDbContext.Database.EnsureDeleted();
        }

        [Fact]
        public async Task SubscribedChatNotAdded()
        {
            var fakeDbContext = new FakeDbContext(null);
            var handeler = new RegisterChatForScheduledPropositionMessageHandler(fakeDbContext, new FakeTelegramBotClient());

            await handeler.HandleMessage(new Message { Text = "1234", Chat = new Chat { Id = 1 } });

            var result = fakeDbContext.SubscribedChat.ToList();

            Assert.Empty(result);
        }
    }
}
