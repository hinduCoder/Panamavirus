using System.ComponentModel.DataAnnotations.Schema;

namespace Panamavirus.Bot.Console.Entities
{
    public class SubscribedChat
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public long ChatId { get; set; }
    }
}
