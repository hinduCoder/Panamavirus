using System.ComponentModel.DataAnnotations.Schema;

namespace Panamavirus.Bot.Console.Entities
{
    public class Participance
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int MessageId { get; set; }
    }
}
