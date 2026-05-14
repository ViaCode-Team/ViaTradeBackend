using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Dto
{
    public class TradeRemindDto
    {
        [Required]
        public required int Id { get; set; }

        [Required]
        public required string TextRemind { get; set; }

        [Required]
        public required DateTime DateTime { get; set; }

        [Required]
        public required int TradeCodeId { get; set; }

        [Required]
        public required int UserId { get; set; }
    }
}
